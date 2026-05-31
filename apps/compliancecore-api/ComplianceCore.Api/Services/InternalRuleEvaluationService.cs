using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class InternalRuleEvaluationService(
    ComplianceCoreDbContext db,
    FactResolveService factResolveService,
    RuleEvaluationService ruleEvaluationService,
    ComplianceFindingService findingService,
    ComplianceWaiverService waiverService,
    IComplianceCoreAuditService auditService)
{
    public const string EvaluateActionScope = "compliancecore.rules.evaluate";

    public const int MaxBatchItems = 100;

    public async Task<InternalEvaluateRulePackBatchResponse> EvaluateBatchAsync(
        InternalEvaluateRulePackBatchRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var items = NormalizeBatchItems(request.Items);
        ValidateBatchSize(items.Count);

        var batchId = Guid.NewGuid();
        var evaluateTasks = items.Select(item =>
            EvaluateAsync(
                new InternalEvaluateRulePackRequest(
                    request.TenantId,
                    item.RulePackKey,
                    MergeContext(request.Context, item.Context),
                    request.EmitFindings),
                sourceProductKey,
                cancellationToken));

        var results = await Task.WhenAll(evaluateTasks);
        var summary = BuildBatchSummary(results);

        await auditService.WriteAsync(
            "rules.internal_evaluate_batch",
            request.TenantId,
            actorUserId: null,
            "rule_pack_evaluation_batch",
            batchId.ToString(),
            $"{summary.AllowCount} allow, {summary.WarnCount} warn, {summary.BlockCount} block",
            reasonCode: summary.BlockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new InternalEvaluateRulePackBatchResponse(batchId, results, summary);
    }

    public async Task<InternalEvaluateRulePackResponse> EvaluateAsync(
        InternalEvaluateRulePackRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var packKey = NormalizePackKey(request.RulePackKey);
        var rulePack = await db.RulePacks
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.PackKey == packKey && x.IsActive)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (rulePack is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        if (string.IsNullOrWhiteSpace(rulePack.RuleContentJson))
        {
            throw new StlApiException(
                "rule_evaluation.no_content",
                "Rule pack has no rule content to evaluate.",
                409);
        }

        var content = RuleEvaluator.ParseContent(rulePack.RuleContentJson);
        var factKeys = content.Rules
            .Select(rule => rule.FactKey.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var resolveResponse = await factResolveService.ResolveAsync(
            new InternalResolveFactsRequest(request.TenantId, factKeys, request.Context),
            sourceProductKey,
            cancellationToken);

        var resolvedFacts = BuildBooleanFactMap(resolveResponse.Resolved);
        var unresolved = resolveResponse.UnresolvedFactKeys.ToList();
        var (evaluationResult, ruleResults) = RuleEvaluator.Evaluate(content, resolvedFacts);

        var (outcome, reasonCode, message, _) = RuleEvaluationOutcomeMapper.Map(
            evaluationResult,
            unresolved,
            ruleResults);

        var waiverApplied = await waiverService.ApplyWaiverIfEligibleAsync(
            request.TenantId,
            rulePack.Id,
            rulePack.PackKey,
            outcome,
            reasonCode,
            message,
            ruleResults,
            gateKey: null,
            request.Context,
            cancellationToken);
        outcome = waiverApplied.Outcome;
        reasonCode = waiverApplied.ReasonCode;
        message = waiverApplied.Message;

        Guid? evaluationRunId = null;
        IReadOnlyList<ComplianceFindingResponse> findingsEmitted = [];

        var shouldPersistSnapshot = request.PersistSnapshot
            || (request.EmitFindings
                && !string.Equals(outcome, ComplianceEvaluationOutcomes.Allow, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(outcome, ComplianceEvaluationOutcomes.Waived, StringComparison.OrdinalIgnoreCase));

        if (shouldPersistSnapshot)
        {
            var run = await ruleEvaluationService.PersistInternalEvaluationSnapshotAsync(
                request.TenantId,
                rulePack.Id,
                resolvedFacts,
                ruleResults,
                evaluationResult,
                cancellationToken);

            evaluationRunId = run.EvaluationRunId;
            if (request.EmitFindings
                && !string.Equals(outcome, ComplianceEvaluationOutcomes.Allow, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(outcome, ComplianceEvaluationOutcomes.Waived, StringComparison.OrdinalIgnoreCase))
            {
                findingsEmitted = await findingService.EmitFromEvaluationAsync(
                    request.TenantId,
                    rulePack.Id,
                    rulePack.PackKey,
                    run.EvaluationRunId,
                    evaluationResult,
                    unresolved,
                    ruleResults,
                    cancellationToken);
            }
        }

        await auditService.WriteAsync(
            "rules.internal_evaluate",
            request.TenantId,
            actorUserId: null,
            "rule_pack",
            rulePack.Id.ToString(),
            outcome,
            reasonCode: reasonCode,
            cancellationToken: cancellationToken);

        await ruleEvaluationService.WriteEvaluationLifecycleEventsAsync(
            request.TenantId,
            actorUserId: null,
            evaluationRunId,
            rulePack.Id,
            outcome,
            reasonCode,
            unresolved,
            ruleResults,
            cancellationToken);

        if (!string.Equals(outcome, ComplianceEvaluationOutcomes.Allow, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(outcome, ComplianceEvaluationOutcomes.Waived, StringComparison.OrdinalIgnoreCase))
        {
            await ruleEvaluationService.WriteReviewRequiredEventIfNeededAsync(
                request.TenantId,
                actorUserId: null,
                evaluationRunId,
                rulePack.Id,
                outcome,
                ruleResults,
                cancellationToken);

            await ruleEvaluationService.WriteRemediationRequiredEventIfNeededAsync(
                request.TenantId,
                actorUserId: null,
                evaluationRunId,
                rulePack.Id,
                ComplianceEvaluationOutcomes.NeedsRemediation,
                ruleResults,
                cancellationToken);
        }

        return new InternalEvaluateRulePackResponse(
            request.TenantId,
            rulePack.Id,
            rulePack.PackKey,
            outcome,
            reasonCode,
            message,
            evaluationResult,
            unresolved,
            resolvedFacts,
            ruleResults,
            evaluationRunId,
            findingsEmitted,
            waiverApplied.WaiverId,
            waiverApplied.WaiverKey);
    }

    private static Dictionary<string, bool> BuildBooleanFactMap(IReadOnlyList<ResolvedFactValue> resolved)
    {
        var facts = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in resolved)
        {
            if (!string.Equals(item.ValueType, FactValueTypes.Boolean, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (item.Value is null)
            {
                continue;
            }

            if (item.Value.Value.ValueKind == JsonValueKind.True)
            {
                facts[item.FactKey] = true;
            }
            else if (item.Value.Value.ValueKind == JsonValueKind.False)
            {
                facts[item.FactKey] = false;
            }
        }

        return facts;
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

        if (count > MaxBatchItems)
        {
            throw new StlApiException(
                "rules.batch_validation",
                $"Batch rule evaluation is limited to {MaxBatchItems} items per request.",
                400);
        }
    }

    private static IReadOnlyList<InternalEvaluateRulePackBatchItem> NormalizeBatchItems(
        IReadOnlyList<InternalEvaluateRulePackBatchItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            return [];
        }

        return items
            .Select(item => new InternalEvaluateRulePackBatchItem(
                NormalizePackKey(item.RulePackKey),
                item.Context))
            .ToList();
    }

    private static IReadOnlyDictionary<string, string>? MergeContext(
        IReadOnlyDictionary<string, string>? shared,
        IReadOnlyDictionary<string, string>? item)
    {
        if (shared is null || shared.Count == 0)
        {
            return item;
        }

        if (item is null || item.Count == 0)
        {
            return shared;
        }

        var merged = new Dictionary<string, string>(shared, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in item)
        {
            merged[key] = value;
        }

        return merged;
    }

    private static InternalEvaluateRulePackBatchSummary BuildBatchSummary(
        IReadOnlyList<InternalEvaluateRulePackResponse> results)
    {
        var allowCount = 0;
        var warnCount = 0;
        var blockCount = 0;
        var waivedCount = 0;

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
                case ComplianceEvaluationOutcomes.Waived:
                    waivedCount++;
                    break;
                default:
                    blockCount++;
                    break;
            }
        }

        return new InternalEvaluateRulePackBatchSummary(
            results.Count,
            allowCount,
            warnCount,
            blockCount,
            waivedCount);
    }
}
