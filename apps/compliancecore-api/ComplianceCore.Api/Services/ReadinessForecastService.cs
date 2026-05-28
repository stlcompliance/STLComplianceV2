using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ReadinessForecastService(
    ComplianceCoreDbContext db,
    RiskScoringService riskScoringService,
    MissingEvidenceWarningService missingEvidenceWarningService,
    ControlEffectivenessService controlEffectivenessService,
    IComplianceCoreAuditService auditService)
{
    public async Task<EvaluateReadinessForecastResponse> EvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateReadinessForecastRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopeKey = ResolveScopeKey(request);
        var evaluateRequest = new EvaluateRiskScoresRequest(
            request.ScopeKey,
            request.RulePackKey,
            request.Context);

        var riskResult = await riskScoringService.EvaluateAsync(
            tenantId,
            actorUserId,
            evaluateRequest,
            cancellationToken);
        var missingEvidenceResult = await missingEvidenceWarningService.EvaluateAsync(
            tenantId,
            actorUserId,
            new EvaluateMissingEvidenceWarningsRequest(
                request.ScopeKey,
                request.RulePackKey,
                request.Context),
            cancellationToken);
        var effectivenessResult = await controlEffectivenessService.EvaluateAsync(
            tenantId,
            actorUserId,
            new EvaluateControlEffectivenessRequest(
                request.ScopeKey,
                request.RulePackKey,
                request.Context),
            cancellationToken);

        var riskByPack = riskResult.Scores.ToDictionary(x => x.PackKey, StringComparer.OrdinalIgnoreCase);
        var effectivenessByPack = effectivenessResult.Records.ToDictionary(
            x => x.PackKey,
            StringComparer.OrdinalIgnoreCase);
        var warningsByPack = missingEvidenceResult.Warnings
            .GroupBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.ToList(),
                StringComparer.OrdinalIgnoreCase);

        var packKeys = riskByPack.Keys
            .Union(effectivenessByPack.Keys, StringComparer.OrdinalIgnoreCase)
            .Union(warningsByPack.Keys, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (packKeys.Count == 0)
        {
            throw new StlApiException(
                "readiness_forecasts.no_packs",
                "No published rule packs were available to forecast readiness.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var run = new ReadinessForecastRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScopeKey = scopeKey,
            ActorUserId = actorUserId,
            RiskScoreRunId = riskResult.RunId,
            MissingEvidenceWarningRunId = missingEvidenceResult.RunId,
            ControlEffectivenessRunId = effectivenessResult.RunId,
            ForecastedAt = now,
            HighestRiskScore = riskResult.HighestRiskScore,
            MissingEvidenceWarningCount = missingEvidenceResult.WarningsEmittedCount,
            AverageEffectivenessScore = effectivenessResult.AverageEffectivenessScore,
        };

        var forecasts = new List<ReadinessForecast>();
        var lowestScore = 100;
        var lowestLevel = ReadinessForecastLevels.Ready;
        var scoreSum = 0;

        foreach (var packKey in packKeys)
        {
            riskByPack.TryGetValue(packKey, out var risk);
            effectivenessByPack.TryGetValue(packKey, out var effectiveness);
            warningsByPack.TryGetValue(packKey, out var warnings);
            warnings ??= [];

            var riskScore = risk?.RiskScore ?? 50;
            var riskLevel = risk?.RiskLevel ?? RiskScoreLevels.Medium;
            var effectivenessScore = effectiveness?.EffectivenessScore ?? 50;
            var effectivenessLevel = effectiveness?.EffectivenessLevel ?? ControlEffectivenessLevels.PartiallyEffective;
            var warningCount = warnings.Count;
            var highestSeverity = warnings.Count == 0
                ? MissingEvidenceWarningSeverities.Low
                : warnings
                    .Select(x => x.Severity)
                    .Aggregate(MissingEvidenceWarningSeverities.Low, MissingEvidenceWarningSeverities.Max);

            var readinessScore = ReadinessForecastRules.ComputePackReadinessScore(
                riskScore,
                effectivenessScore,
                warningCount,
                highestSeverity);
            var readinessLevel = ReadinessForecastLevels.FromScore(readinessScore);

            if (readinessScore < lowestScore)
            {
                lowestScore = readinessScore;
                lowestLevel = readinessLevel;
            }
            else if (readinessScore == lowestScore)
            {
                lowestLevel = ReadinessForecastLevels.Min(lowestLevel, readinessLevel);
            }

            scoreSum += readinessScore;

            var rulePackId = risk?.RulePackId
                ?? effectiveness?.RulePackId
                ?? warnings[0].RulePackId;

            forecasts.Add(new ReadinessForecast
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RunId = run.Id,
                ScopeKey = scopeKey,
                RulePackId = rulePackId,
                PackKey = packKey,
                ReadinessScore = readinessScore,
                ReadinessLevel = readinessLevel,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                EffectivenessScore = effectivenessScore,
                EffectivenessLevel = effectivenessLevel,
                MissingEvidenceWarningCount = warningCount,
                HighestMissingEvidenceSeverity = highestSeverity,
                Summary = ReadinessForecastRules.BuildSummary(
                    packKey,
                    readinessScore,
                    readinessLevel,
                    riskScore,
                    effectivenessScore,
                    warningCount),
                ForecastedAt = now,
            });
        }

        run.PacksForecastCount = forecasts.Count;
        run.LowestReadinessScore = forecasts.Count == 0 ? 0 : lowestScore;
        run.AverageReadinessScore = forecasts.Count == 0
            ? 0
            : (int)Math.Round((double)scoreSum / forecasts.Count);
        run.ReadinessScore = run.AverageReadinessScore;
        run.ReadinessLevel = ReadinessForecastLevels.FromScore(run.ReadinessScore);

        db.ReadinessForecastRuns.Add(run);
        db.ReadinessForecasts.AddRange(forecasts);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "readiness_forecasts.evaluate",
            tenantId,
            actorUserId,
            "readiness_forecast_run",
            run.Id.ToString(),
            "success",
            reasonCode: $"{scopeKey}:{run.ReadinessScore}:{run.ReadinessLevel}",
            cancellationToken: cancellationToken);

        return new EvaluateReadinessForecastResponse(
            run.Id,
            scopeKey,
            run.PacksForecastCount,
            run.ReadinessScore,
            run.ReadinessLevel,
            run.LowestReadinessScore,
            run.AverageReadinessScore,
            run.HighestRiskScore,
            run.MissingEvidenceWarningCount,
            run.AverageEffectivenessScore,
            run.RiskScoreRunId,
            run.MissingEvidenceWarningRunId,
            run.ControlEffectivenessRunId,
            run.ForecastedAt,
            forecasts.Select(MapResponse).ToList());
    }

    public async Task<IReadOnlyList<ReadinessForecastResponse>> ListForecastsAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? readinessLevel,
        Guid? runId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? 50, 1, ReadinessForecastRules.MaxListLimit);
        var query = db.ReadinessForecasts.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            query = query.Where(x => x.ScopeKey == ProductFactMirrorRules.NormalizeScopeKey(scopeKey));
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalizedPack = rulePackKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPack);
        }

        if (!string.IsNullOrWhiteSpace(readinessLevel))
        {
            var normalizedLevel = readinessLevel.Trim().ToLowerInvariant();
            query = query.Where(x => x.ReadinessLevel == normalizedLevel);
        }

        if (runId.HasValue)
        {
            query = query.Where(x => x.RunId == runId.Value);
        }
        else
        {
            var latestRunId = await db.ReadinessForecastRuns
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.ForecastedAt)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestRunId is null)
            {
                return [];
            }

            query = query.Where(x => x.RunId == latestRunId.Value);
        }

        var rows = await query
            .OrderBy(x => x.PackKey)
            .Take(cappedLimit)
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(x => x.ReadinessScore)
            .ThenBy(x => x.PackKey)
            .Select(MapResponse)
            .ToList();
    }

    public async Task<ReadinessForecastSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var latestRunId = await db.ReadinessForecastRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ForecastedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRunId is null)
        {
            return new ReadinessForecastSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                ReadinessForecastLevels.Unknown,
                0,
                0,
                null,
                DateTimeOffset.UtcNow);
        }

        var forecasts = await db.ReadinessForecasts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == latestRunId.Value)
            .ToListAsync(cancellationToken);

        var run = await db.ReadinessForecastRuns
            .AsNoTracking()
            .Where(x => x.Id == latestRunId.Value)
            .FirstAsync(cancellationToken);

        var lowestLevel = forecasts.Count == 0
            ? ReadinessForecastLevels.Unknown
            : forecasts
                .Select(x => x.ReadinessLevel)
                .Aggregate(ReadinessForecastLevels.Ready, ReadinessForecastLevels.Min);

        return new ReadinessForecastSummaryResponse(
            forecasts.Count,
            forecasts.Select(x => x.ScopeKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Ready),
            forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Caution),
            forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.NotReady),
            forecasts.Count(x => x.ReadinessLevel == ReadinessForecastLevels.Unknown),
            run.ReadinessScore,
            run.ReadinessLevel,
            run.LowestReadinessScore,
            run.AverageReadinessScore,
            run.ForecastedAt,
            DateTimeOffset.UtcNow);
    }

    private static string ResolveScopeKey(EvaluateReadinessForecastRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ScopeKey))
        {
            return ProductFactMirrorRules.NormalizeScopeKey(request.ScopeKey);
        }

        return ProductFactMirrorRules.ResolveScopeKeyFromContext(request.Context);
    }

    private static ReadinessForecastResponse MapResponse(ReadinessForecast entity) =>
        new(
            entity.Id,
            entity.RunId,
            entity.ScopeKey,
            entity.RulePackId,
            entity.PackKey,
            entity.ReadinessScore,
            entity.ReadinessLevel,
            entity.RiskScore,
            entity.RiskLevel,
            entity.EffectivenessScore,
            entity.EffectivenessLevel,
            entity.MissingEvidenceWarningCount,
            entity.HighestMissingEvidenceSeverity,
            entity.Summary,
            entity.ForecastedAt);
}
