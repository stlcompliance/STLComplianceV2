using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ControlEffectivenessService(
    ComplianceCoreDbContext db,
    InternalRuleEvaluationService internalRuleEvaluationService,
    IComplianceCoreAuditService auditService)
{
    public async Task<EvaluateControlEffectivenessResponse> EvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateControlEffectivenessRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopeKey = ResolveScopeKey(request);
        var context = request.Context ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var packs = await LoadPacksToEvaluateAsync(tenantId, request.RulePackKey, cancellationToken);

        if (packs.Count == 0)
        {
            throw new StlApiException(
                "control_effectiveness.no_packs",
                "No published rule packs with content are available to evaluate.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var run = new ControlEffectivenessRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScopeKey = scopeKey,
            ActorUserId = actorUserId,
            EvaluatedAt = now,
        };

        var records = new List<ControlEffectivenessRecord>();
        var lowestScore = 100;
        var lowestLevel = ControlEffectivenessLevels.Effective;
        var scoreSum = 0;

        foreach (var pack in packs)
        {
            var evaluation = await internalRuleEvaluationService.EvaluateAsync(
                new InternalEvaluateRulePackRequest(
                    tenantId,
                    pack.PackKey,
                    context,
                    EmitFindings: false),
                sourceProductKey: "control_effectiveness",
                cancellationToken);

            var totalRuleCount = evaluation.RuleResults.Count;
            var failedRuleCount = evaluation.RuleResults.Count(item =>
                !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase));
            var passedRuleCount = totalRuleCount - failedRuleCount;
            var scoreValue = ControlEffectivenessRules.ComputeScore(
                evaluation.Outcome,
                evaluation.EvaluationResult,
                evaluation.UnresolvedFactKeys.Count,
                failedRuleCount,
                passedRuleCount,
                totalRuleCount);
            var level = ControlEffectivenessLevels.FromScore(scoreValue);
            var controlStatus = ControlEffectivenessStatuses.FromLevel(level);

            if (scoreValue < lowestScore)
            {
                lowestScore = scoreValue;
                lowestLevel = level;
            }
            else if (scoreValue == lowestScore)
            {
                lowestLevel = ControlEffectivenessLevels.Min(lowestLevel, level);
            }

            scoreSum += scoreValue;

            records.Add(new ControlEffectivenessRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RunId = run.Id,
                ScopeKey = scopeKey,
                RulePackId = pack.Id,
                PackKey = pack.PackKey,
                EffectivenessScore = scoreValue,
                EffectivenessLevel = level,
                ControlStatus = controlStatus,
                RuleOutcome = evaluation.Outcome,
                EvaluationResult = evaluation.EvaluationResult,
                TotalRuleCount = totalRuleCount,
                PassedRuleCount = passedRuleCount,
                FailedRuleCount = failedRuleCount,
                UnresolvedFactCount = evaluation.UnresolvedFactKeys.Count,
                ResolvedFactCount = evaluation.ResolvedFacts.Count,
                Summary = ControlEffectivenessRules.BuildSummary(
                    pack.PackKey,
                    scoreValue,
                    level,
                    controlStatus,
                    evaluation.Outcome,
                    passedRuleCount,
                    totalRuleCount,
                    evaluation.UnresolvedFactKeys.Count),
                EvaluatedAt = now,
            });
        }

        run.PacksEvaluatedCount = records.Count;
        run.LowestEffectivenessScore = records.Count == 0 ? 0 : lowestScore;
        run.LowestEffectivenessLevel = records.Count == 0
            ? ControlEffectivenessLevels.Unknown
            : lowestLevel;
        run.AverageEffectivenessScore = records.Count == 0
            ? 0
            : (int)Math.Round((double)scoreSum / records.Count);

        db.ControlEffectivenessRuns.Add(run);
        db.ControlEffectivenessRecords.AddRange(records);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "control_effectiveness.evaluate",
            tenantId,
            actorUserId,
            "control_effectiveness_run",
            run.Id.ToString(),
            "success",
            reasonCode: $"{scopeKey}:{run.LowestEffectivenessScore}:{run.LowestEffectivenessLevel}",
            cancellationToken: cancellationToken);

        return new EvaluateControlEffectivenessResponse(
            run.Id,
            scopeKey,
            run.PacksEvaluatedCount,
            run.LowestEffectivenessScore,
            run.LowestEffectivenessLevel,
            run.AverageEffectivenessScore,
            run.EvaluatedAt,
            records.Select(MapResponse).ToList());
    }

    public async Task<IReadOnlyList<ControlEffectivenessRecordResponse>> ListRecordsAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        string? effectivenessLevel,
        Guid? runId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? 50, 1, ControlEffectivenessRules.MaxListLimit);
        var query = db.ControlEffectivenessRecords.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            query = query.Where(x => x.ScopeKey == ProductFactMirrorRules.NormalizeScopeKey(scopeKey));
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalizedPack = rulePackKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPack);
        }

        if (!string.IsNullOrWhiteSpace(effectivenessLevel))
        {
            var normalizedLevel = effectivenessLevel.Trim().ToLowerInvariant();
            query = query.Where(x => x.EffectivenessLevel == normalizedLevel);
        }

        if (runId.HasValue)
        {
            query = query.Where(x => x.RunId == runId.Value);
        }
        else
        {
            var latestRunId = await db.ControlEffectivenessRuns
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.EvaluatedAt)
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
            .OrderBy(x => x.EffectivenessScore)
            .ThenBy(x => x.PackKey)
            .Select(MapResponse)
            .ToList();
    }

    public async Task<ControlEffectivenessSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var latestRunId = await db.ControlEffectivenessRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.EvaluatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRunId is null)
        {
            return new ControlEffectivenessSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                ControlEffectivenessLevels.Unknown,
                0,
                null,
                DateTimeOffset.UtcNow);
        }

        var records = await db.ControlEffectivenessRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == latestRunId.Value)
            .ToListAsync(cancellationToken);

        var run = await db.ControlEffectivenessRuns
            .AsNoTracking()
            .Where(x => x.Id == latestRunId.Value)
            .FirstAsync(cancellationToken);

        var lowestLevel = records.Count == 0
            ? ControlEffectivenessLevels.Unknown
            : records
                .Select(x => x.EffectivenessLevel)
                .Aggregate(ControlEffectivenessLevels.Effective, ControlEffectivenessLevels.Min);

        return new ControlEffectivenessSummaryResponse(
            records.Count,
            records.Select(x => x.ScopeKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            records.Count(x => x.EffectivenessLevel == ControlEffectivenessLevels.Effective),
            records.Count(x => x.EffectivenessLevel == ControlEffectivenessLevels.PartiallyEffective),
            records.Count(x => x.EffectivenessLevel == ControlEffectivenessLevels.Ineffective),
            records.Count(x => x.EffectivenessLevel == ControlEffectivenessLevels.Unknown),
            run.LowestEffectivenessScore,
            lowestLevel,
            run.AverageEffectivenessScore,
            run.EvaluatedAt,
            DateTimeOffset.UtcNow);
    }

    private async Task<List<RulePack>> LoadPacksToEvaluateAsync(
        Guid tenantId,
        string? rulePackKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalized = rulePackKey.Trim().ToLowerInvariant();
            var pack = await db.RulePacks
                .Where(x => x.TenantId == tenantId
                    && x.PackKey == normalized
                    && x.IsActive
                    && x.Status == RulePackStatuses.Published
                    && x.RuleContentJson != null
                    && x.RuleContentJson != "")
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return pack is null ? [] : [pack];
        }

        var packs = await db.RulePacks
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && x.Status == RulePackStatuses.Published
                && x.RuleContentJson != null
                && x.RuleContentJson != "")
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        return packs
            .GroupBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(ControlEffectivenessRules.MaxPacksPerEvaluate)
            .ToList();
    }

    private static string ResolveScopeKey(EvaluateControlEffectivenessRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ScopeKey))
        {
            return ProductFactMirrorRules.NormalizeScopeKey(request.ScopeKey);
        }

        return ProductFactMirrorRules.ResolveScopeKeyFromContext(request.Context);
    }

    private static ControlEffectivenessRecordResponse MapResponse(ControlEffectivenessRecord entity) =>
        new(
            entity.Id,
            entity.RunId,
            entity.ScopeKey,
            entity.RulePackId,
            entity.PackKey,
            entity.EffectivenessScore,
            entity.EffectivenessLevel,
            entity.ControlStatus,
            entity.RuleOutcome,
            entity.EvaluationResult,
            entity.TotalRuleCount,
            entity.PassedRuleCount,
            entity.FailedRuleCount,
            entity.UnresolvedFactCount,
            entity.ResolvedFactCount,
            entity.Summary,
            entity.EvaluatedAt);
}
