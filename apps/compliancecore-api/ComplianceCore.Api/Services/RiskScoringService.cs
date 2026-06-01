using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RiskScoringService(
    ComplianceCoreDbContext db,
    InternalRuleEvaluationService internalRuleEvaluationService,
    IComplianceCoreAuditService auditService)
{
    public async Task<EvaluateRiskScoresResponse> EvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateRiskScoresRequest request,
        CancellationToken cancellationToken = default)
    {
        var scopeKey = ResolveScopeKey(request);
        var context = request.Context ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var packs = await LoadPacksToScoreAsync(tenantId, request.RulePackKey, cancellationToken);

        if (packs.Count == 0)
        {
            throw new StlApiException(
                "risk_scores.no_packs",
                "No published rule packs with content are available to score.",
                404);
        }

        var mirrorFactCount = await db.ProductFactMirrors
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId && x.ScopeKey == scopeKey,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var run = new RiskScoreRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ScopeKey = scopeKey,
            ActorUserId = actorUserId,
            EvaluatedAt = now,
        };

        var scores = new List<RiskScore>();
        var highestScore = 0;
        var highestLevel = RiskScoreLevels.Low;

        foreach (var pack in packs)
        {
            var evaluation = await internalRuleEvaluationService.EvaluateAsync(
                new InternalEvaluateRulePackRequest(
                    tenantId,
                    pack.PackKey,
                    context,
                    EmitFindings: false),
                sourceProductKey: "risk_scoring",
                cancellationToken);

            var failedRuleCount = evaluation.RuleResults.Count(item =>
                !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase));
            var requiredFactCount = evaluation.RuleResults.Count + evaluation.UnresolvedFactKeys.Count;
            var scoreValue = RiskScoringRules.ComputeScore(
                evaluation.Outcome,
                evaluation.EvaluationResult,
                evaluation.UnresolvedFactKeys.Count,
                failedRuleCount,
                requiredFactCount,
                mirrorFactCount);
            var level = RiskScoreLevels.FromScore(scoreValue);

            if (scoreValue > highestScore)
            {
                highestScore = scoreValue;
                highestLevel = level;
            }

            var entity = new RiskScore
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RunId = run.Id,
                ScopeKey = scopeKey,
                RulePackId = pack.Id,
                PackKey = pack.PackKey,
                RiskScoreValue = scoreValue,
                RiskLevel = level,
                RuleOutcome = evaluation.Outcome,
                EvaluationResult = evaluation.EvaluationResult,
                UnresolvedFactCount = evaluation.UnresolvedFactKeys.Count,
                FailedRuleCount = failedRuleCount,
                ResolvedFactCount = evaluation.ResolvedFacts.Count,
                MirrorFactCount = mirrorFactCount,
                Summary = RiskScoringRules.BuildSummary(
                    pack.PackKey,
                    scoreValue,
                    level,
                    evaluation.Outcome,
                    evaluation.UnresolvedFactKeys.Count,
                    failedRuleCount,
                    mirrorFactCount),
                EvaluatedAt = now,
            };

            scores.Add(entity);
        }

        run.PacksEvaluatedCount = scores.Count;
        run.HighestRiskScore = highestScore;
        run.HighestRiskLevel = highestLevel;

        db.RiskScoreRuns.Add(run);
        db.RiskScores.AddRange(scores);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "risk_scores.evaluate",
            tenantId,
            actorUserId,
            "risk_score_run",
            run.Id.ToString(),
            "success",
            reasonCode: $"{scopeKey}:{highestScore}:{highestLevel}",
            cancellationToken: cancellationToken);

        return new EvaluateRiskScoresResponse(
            run.Id,
            scopeKey,
            run.PacksEvaluatedCount,
            run.HighestRiskScore,
            run.HighestRiskLevel,
            mirrorFactCount,
            run.EvaluatedAt,
            scores.Select(MapResponse).ToList());
    }

    public async Task<IReadOnlyList<RiskScoreResponse>> ListScoresAsync(
        Guid tenantId,
        string? scopeKey,
        string? rulePackKey,
        Guid? runId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit ?? 50, 1, RiskScoringRules.MaxListLimit);
        var query = db.RiskScores.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(scopeKey))
        {
            query = query.Where(x => x.ScopeKey == ProductFactMirrorRules.NormalizeScopeKey(scopeKey));
        }

        if (!string.IsNullOrWhiteSpace(rulePackKey))
        {
            var normalizedPack = rulePackKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.PackKey == normalizedPack);
        }

        if (runId.HasValue)
        {
            query = query.Where(x => x.RunId == runId.Value);
        }
        else
        {
            var latestRunId = await db.RiskScoreRuns
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

        return await query
            .OrderByDescending(x => x.RiskScoreValue)
            .ThenBy(x => x.PackKey)
            .Take(cappedLimit)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<RiskScoreSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var latestRunId = await db.RiskScoreRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.EvaluatedAt)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRunId is null)
        {
            return new RiskScoreSummaryResponse(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                RiskScoreLevels.Low,
                null,
                DateTimeOffset.UtcNow);
        }

        var scores = await db.RiskScores
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RunId == latestRunId.Value)
            .ToListAsync(cancellationToken);

        var lastEvaluated = await db.RiskScoreRuns
            .AsNoTracking()
            .Where(x => x.Id == latestRunId.Value)
            .Select(x => x.EvaluatedAt)
            .FirstAsync(cancellationToken);

        var highest = scores.Count == 0 ? 0 : scores.Max(x => x.RiskScoreValue);
        return new RiskScoreSummaryResponse(
            scores.Count,
            scores.Select(x => x.ScopeKey).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            scores.Count(x => x.RiskLevel == RiskScoreLevels.Low),
            scores.Count(x => x.RiskLevel == RiskScoreLevels.Medium),
            scores.Count(x => x.RiskLevel == RiskScoreLevels.High),
            scores.Count(x => x.RiskLevel == RiskScoreLevels.Critical),
            highest,
            RiskScoreLevels.FromScore(highest),
            lastEvaluated,
            DateTimeOffset.UtcNow);
    }

    private async Task<List<RulePack>> LoadPacksToScoreAsync(
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
                    && x.RuleContentJson != null)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return pack is null || string.IsNullOrWhiteSpace(pack.RuleContentJson) ? [] : [pack];
        }

        var packs = await db.RulePacks
            .Where(x => x.TenantId == tenantId
                && x.IsActive
                && x.Status == RulePackStatuses.Published
                && x.RuleContentJson != null)
            .OrderByDescending(x => x.VersionNumber)
            .ToListAsync(cancellationToken);

        return packs
            .Where(x => !string.IsNullOrWhiteSpace(x.RuleContentJson))
            .GroupBy(x => x.PackKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(RiskScoringRules.MaxPacksPerEvaluate)
            .ToList();
    }

    private static string ResolveScopeKey(EvaluateRiskScoresRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ScopeKey))
        {
            return ProductFactMirrorRules.NormalizeScopeKey(request.ScopeKey);
        }

        return ProductFactMirrorRules.ResolveScopeKeyFromContext(request.Context);
    }

    private static RiskScoreResponse MapResponse(RiskScore entity) =>
        new(
            entity.Id,
            entity.RunId,
            entity.ScopeKey,
            entity.RulePackId,
            entity.PackKey,
            entity.RiskScoreValue,
            entity.RiskLevel,
            entity.RuleOutcome,
            entity.EvaluationResult,
            entity.UnresolvedFactCount,
            entity.FailedRuleCount,
            entity.ResolvedFactCount,
            entity.MirrorFactCount,
            entity.Summary,
            entity.EvaluatedAt);
}
