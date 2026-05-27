using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RulePackBatchEvaluationService(
    ComplianceCoreDbContext db,
    RuleEvaluationService ruleEvaluationService,
    IComplianceCoreAuditService auditService)
{
    public async Task<EvaluateRulePackBatchResponse> EvaluateBatchForUserAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateRulePackBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = NormalizeBatchItems(request.Items);
        ValidateBatchSize(items.Count);

        var batchId = Guid.NewGuid();
        var evaluateTasks = items.Select(item =>
            EvaluateItemAsync(
                tenantId,
                actorUserId,
                item,
                MergeFacts(request.Facts, item.Facts),
                request.EmitFindings,
                cancellationToken));

        var results = await Task.WhenAll(evaluateTasks);
        var summary = BuildBatchSummary(results);

        await auditService.WriteAsync(
            "rules.evaluate_batch",
            tenantId,
            actorUserId,
            "rule_pack_evaluation_batch",
            batchId.ToString(),
            $"{summary.AllowCount} allow, {summary.WarnCount} warn, {summary.BlockCount} block",
            reasonCode: summary.BlockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new EvaluateRulePackBatchResponse(batchId, results, summary);
    }

    private async Task<EvaluateRulePackBatchResultItem> EvaluateItemAsync(
        Guid tenantId,
        Guid? actorUserId,
        EvaluateRulePackBatchItem item,
        IReadOnlyDictionary<string, bool>? facts,
        bool emitFindings,
        CancellationToken cancellationToken)
    {
        var rulePack = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PackKey == item.RulePackKey && x.IsActive)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (rulePack is null)
        {
            throw new StlApiException("rule_packs.not_found", $"Rule pack '{item.RulePackKey}' was not found.", 404);
        }

        var evaluation = await ruleEvaluationService.EvaluateAsync(
            tenantId,
            actorUserId,
            rulePack.Id,
            new EvaluateRulePackRequest(facts ?? new Dictionary<string, bool>(), emitFindings),
            cancellationToken);

        var (outcome, reasonCode, message, _) = RuleEvaluationOutcomeMapper.Map(
            evaluation.OverallResult,
            [],
            evaluation.RuleResults);

        return new EvaluateRulePackBatchResultItem(
            item.RulePackKey,
            rulePack.Id,
            evaluation.PackLabel,
            outcome,
            reasonCode,
            message,
            evaluation.OverallResult,
            evaluation.EvaluationRunId,
            evaluation.RuleResults,
            evaluation.FindingsEmitted);
    }

    private static IReadOnlyList<EvaluateRulePackBatchItem> NormalizeBatchItems(
        IReadOnlyList<EvaluateRulePackBatchItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            return [];
        }

        return items
            .Select(item => new EvaluateRulePackBatchItem(
                NormalizePackKey(item.RulePackKey),
                item.Facts))
            .GroupBy(item => item.RulePackKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToList();
    }

    private static string NormalizePackKey(string rulePackKey)
    {
        var normalized = rulePackKey.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException(
                "rule_packs.validation",
                "Rule pack key is required.",
                400);
        }

        return normalized;
    }

    private static void ValidateBatchSize(int count)
    {
        if (count == 0)
        {
            throw new StlApiException(
                "rules.batch_validation",
                "At least one evaluation item is required for a batch evaluate request.",
                400);
        }

        if (count > InternalRuleEvaluationService.MaxBatchItems)
        {
            throw new StlApiException(
                "rules.batch_validation",
                $"Batch rule evaluation is limited to {InternalRuleEvaluationService.MaxBatchItems} items per request.",
                400);
        }
    }

    private static IReadOnlyDictionary<string, bool>? MergeFacts(
        IReadOnlyDictionary<string, bool>? shared,
        IReadOnlyDictionary<string, bool>? item)
    {
        if (shared is null || shared.Count == 0)
        {
            return item;
        }

        if (item is null || item.Count == 0)
        {
            return shared;
        }

        var merged = new Dictionary<string, bool>(shared, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in item)
        {
            merged[key] = value;
        }

        return merged;
    }

    private static EvaluateRulePackBatchSummary BuildBatchSummary(
        IReadOnlyList<EvaluateRulePackBatchResultItem> results)
    {
        var allowCount = 0;
        var warnCount = 0;
        var blockCount = 0;

        foreach (var result in results)
        {
            switch (result.Outcome)
            {
                case ComplianceEvaluationOutcomes.Allow:
                    allowCount++;
                    break;
                case ComplianceEvaluationOutcomes.Warn:
                    warnCount++;
                    break;
                default:
                    blockCount++;
                    break;
            }
        }

        return new EvaluateRulePackBatchSummary(results.Count, allowCount, warnCount, blockCount);
    }
}
