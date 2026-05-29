using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class WorkflowGateService(
    ComplianceCoreDbContext db,
    InternalRuleEvaluationService internalRuleEvaluationService,
    RuleEvaluationService ruleEvaluationService,
    ComplianceWaiverService waiverService,
    IComplianceCoreAuditService auditService)
{
    public const string CheckActionScope = "compliancecore.workflow.gates.check";

    public const int MaxBatchItems = 50;

    public async Task<IReadOnlyList<WorkflowGateDefinitionResponse>> ListDefinitionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var gates = await db.WorkflowGateDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Join(
                db.RulePacks.AsNoTracking(),
                gate => gate.RulePackId,
                pack => pack.Id,
                (gate, pack) => new { gate, pack })
            .OrderBy(x => x.gate.GateKey)
            .ToListAsync(cancellationToken);

        return gates.Select(x => MapDefinitionResponse(x.gate, x.pack.PackKey)).ToList();
    }

    public async Task<WorkflowGateDefinitionResponse> CreateDefinitionAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateWorkflowGateDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.RulePackId && x.IsActive,
            cancellationToken);

        if (rulePack is null)
        {
            throw new StlApiException("rule_packs.not_found", "Rule pack was not found.", 404);
        }

        var gateKey = NormalizeGateKey(request.GateKey);
        var exists = await db.WorkflowGateDefinitions.AnyAsync(
            x => x.TenantId == tenantId && x.GateKey == gateKey,
            cancellationToken);

        if (exists)
        {
            throw new StlApiException(
                "workflow_gates.duplicate",
                "A workflow gate with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var gate = new WorkflowGateDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = request.RulePackId,
            GateKey = gateKey,
            Label = request.Label.Trim(),
            Description = request.Description.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkflowGateDefinitions.Add(gate);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "workflow_gates.create",
            tenantId,
            actorUserId,
            "workflow_gate_definition",
            gate.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapDefinitionResponse(gate, rulePack.PackKey);
    }

    public async Task<WorkflowGateCheckResponse> CheckForUserAsync(
        Guid tenantId,
        Guid? actorUserId,
        WorkflowGateCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var gate = await LoadActiveGateAsync(tenantId, request.GateKey, cancellationToken);
        var rulePack = await db.RulePacks.AsNoTracking()
            .FirstAsync(x => x.Id == gate.RulePackId, cancellationToken);

        var facts = request.Facts ?? new Dictionary<string, bool>();
        var evaluation = await ruleEvaluationService.EvaluateAsync(
            tenantId,
            actorUserId,
            gate.RulePackId,
            new EvaluateRulePackRequest(facts, request.EmitFindings),
            cancellationToken);

        return await FinalizeCheckAsync(
            tenantId,
            gate,
            rulePack.PackKey,
            evaluation.EvaluationRunId,
            evaluation.OverallResult,
            [],
            evaluation.RuleResults,
            request.Context,
            request.EmitFindings,
            evaluation.FindingsEmitted ?? [],
            cancellationToken,
            gateKey: gate.GateKey);
    }

    public async Task<WorkflowGateBatchCheckResponse> CheckBatchForUserAsync(
        Guid tenantId,
        Guid? actorUserId,
        WorkflowGateBatchCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = NormalizeBatchItems(request.Items);
        ValidateBatchSize(items.Count);

        var batchId = Guid.NewGuid();
        var checkTasks = items.Select(item =>
            CheckForUserAsync(
                tenantId,
                actorUserId,
                new WorkflowGateCheckRequest(
                    item.GateKey,
                    MergeFacts(request.Facts, item.Facts),
                    MergeContext(request.Context, item.Context),
                    request.EmitFindings),
                cancellationToken));

        var results = await Task.WhenAll(checkTasks);
        var summary = BuildBatchSummary(results);

        await auditService.WriteAsync(
            "workflow_gates.check_batch",
            tenantId,
            actorUserId,
            "workflow_gate_check_batch",
            batchId.ToString(),
            $"{summary.AllowCount} allow, {summary.WarnCount} warn, {summary.BlockCount} block",
            reasonCode: summary.BlockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new WorkflowGateBatchCheckResponse(batchId, results, summary);
    }

    public async Task<WorkflowGateBatchCheckResponse> CheckBatchInternalAsync(
        InternalWorkflowGateBatchCheckRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var items = NormalizeInternalBatchItems(request.Items);
        ValidateBatchSize(items.Count);

        var batchId = Guid.NewGuid();
        var checkTasks = items.Select(item =>
            CheckInternalAsync(
                new InternalWorkflowGateCheckRequest(
                    request.TenantId,
                    item.GateKey,
                    MergeContext(request.Context, item.Context),
                    request.EmitFindings),
                sourceProductKey,
                cancellationToken));

        var results = await Task.WhenAll(checkTasks);
        var summary = BuildBatchSummary(results);

        await auditService.WriteAsync(
            "workflow_gates.check_batch",
            request.TenantId,
            actorUserId: null,
            "workflow_gate_check_batch",
            batchId.ToString(),
            $"{summary.AllowCount} allow, {summary.WarnCount} warn, {summary.BlockCount} block",
            reasonCode: summary.BlockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new WorkflowGateBatchCheckResponse(batchId, results, summary);
    }

    public async Task<WorkflowGateCheckResponse> CheckInternalAsync(
        InternalWorkflowGateCheckRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        var gate = await LoadActiveGateAsync(request.TenantId, request.GateKey, cancellationToken);
        var rulePack = await db.RulePacks.AsNoTracking()
            .FirstAsync(x => x.Id == gate.RulePackId, cancellationToken);

        var evaluation = await internalRuleEvaluationService.EvaluateAsync(
            new InternalEvaluateRulePackRequest(
                request.TenantId,
                rulePack.PackKey,
                request.Context,
                request.EmitFindings),
            sourceProductKey,
            cancellationToken);

        return await FinalizeCheckAsync(
            request.TenantId,
            gate,
            rulePack.PackKey,
            evaluation.EvaluationRunId,
            evaluation.EvaluationResult,
            evaluation.UnresolvedFactKeys,
            evaluation.RuleResults,
            request.Context,
            request.EmitFindings,
            evaluation.FindingsEmitted ?? [],
            cancellationToken,
            evaluation.Outcome,
            evaluation.ReasonCode,
            evaluation.Message,
            gateKey: gate.GateKey,
            appliedWaiverId: evaluation.AppliedWaiverId,
            appliedWaiverKey: evaluation.AppliedWaiverKey);
    }

    private async Task<WorkflowGateCheckResponse> FinalizeCheckAsync(
        Guid tenantId,
        WorkflowGateDefinition gate,
        string packKey,
        Guid? evaluationRunId,
        string evaluationResult,
        IReadOnlyList<string> unresolvedFactKeys,
        IReadOnlyList<RuleEvaluationItemResponse> ruleResults,
        IReadOnlyDictionary<string, string>? context,
        bool emitFindings,
        IReadOnlyList<ComplianceFindingResponse> findingsEmitted,
        CancellationToken cancellationToken,
        string? precomputedOutcome = null,
        string? precomputedReasonCode = null,
        string? precomputedMessage = null,
        string? gateKey = null,
        Guid? appliedWaiverId = null,
        string? appliedWaiverKey = null)
    {
        var (outcome, reasonCode, message, reasons) = precomputedOutcome is null
            ? RuleEvaluationOutcomeMapper.Map(evaluationResult, unresolvedFactKeys, ruleResults)
            : (precomputedOutcome, precomputedReasonCode!, precomputedMessage!, BuildReasons(unresolvedFactKeys, ruleResults));

        if (appliedWaiverId is null)
        {
            var waiverApplied = await waiverService.ApplyWaiverIfEligibleAsync(
                tenantId,
                gate.RulePackId,
                packKey,
                outcome,
                reasonCode,
                message,
                ruleResults,
                gateKey ?? gate.GateKey,
                context,
                cancellationToken);
            outcome = waiverApplied.Outcome;
            reasonCode = waiverApplied.ReasonCode;
            message = waiverApplied.Message;
            appliedWaiverId = waiverApplied.WaiverId;
            appliedWaiverKey = waiverApplied.WaiverKey;
        }

        if (evaluationRunId is not null && appliedWaiverId is not null)
        {
            var linkedRun = await db.RuleEvaluationRuns
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == evaluationRunId.Value,
                    cancellationToken);
            if (linkedRun is not null)
            {
                linkedRun.AppliedWaiverId = appliedWaiverId;
                linkedRun.AppliedWaiverKey = appliedWaiverKey;
            }
        }

        var now = DateTimeOffset.UtcNow;
        var checkResult = new WorkflowGateCheckResult
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkflowGateDefinitionId = gate.Id,
            RuleEvaluationRunId = evaluationRunId,
            GateKey = gate.GateKey,
            Outcome = outcome,
            ReasonCode = reasonCode,
            Message = message,
            ReasonsJson = JsonSerializer.Serialize(reasons, RuleEvaluationJson.Options),
            ContextJson = JsonSerializer.Serialize(context ?? new Dictionary<string, string>(), RuleEvaluationJson.Options),
            AppliedWaiverId = appliedWaiverId,
            AppliedWaiverKey = appliedWaiverKey,
            CreatedAt = now,
        };

        db.WorkflowGateCheckResults.Add(checkResult);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "workflow_gates.check",
            tenantId,
            actorUserId: null,
            "workflow_gate_check_result",
            checkResult.Id.ToString(),
            outcome,
            reasonCode: reasonCode,
            cancellationToken: cancellationToken);

        return new WorkflowGateCheckResponse(
            checkResult.Id,
            gate.GateKey,
            gate.Label,
            gate.RulePackId,
            packKey,
            outcome,
            reasonCode,
            message,
            evaluationRunId,
            reasons,
            emitFindings ? findingsEmitted : [],
            now,
            appliedWaiverId,
            appliedWaiverKey);
    }

    private async Task<WorkflowGateDefinition> LoadActiveGateAsync(
        Guid tenantId,
        string gateKey,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizeGateKey(gateKey);
        var gate = await db.WorkflowGateDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.GateKey == normalized && x.IsActive,
            cancellationToken);

        if (gate is null)
        {
            throw new StlApiException("workflow_gates.not_found", "Workflow gate was not found.", 404);
        }

        return gate;
    }

    private static IReadOnlyList<WorkflowGateReasonResponse> BuildReasons(
        IReadOnlyList<string> unresolvedFactKeys,
        IReadOnlyList<RuleEvaluationItemResponse> ruleResults) =>
        RuleEvaluationOutcomeMapper.Map(
            RuleEvaluationResults.Fail,
            unresolvedFactKeys,
            ruleResults).Reasons;

    private static WorkflowGateDefinitionResponse MapDefinitionResponse(
        WorkflowGateDefinition gate,
        string packKey) =>
        new(
            gate.Id,
            gate.GateKey,
            gate.Label,
            gate.Description,
            gate.RulePackId,
            packKey,
            gate.IsActive,
            gate.CreatedAt,
            gate.UpdatedAt);

    private static string NormalizeGateKey(string gateKey)
    {
        var normalized = gateKey.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException("workflow_gates.validation", "Gate key is required.", 400);
        }

        return normalized;
    }

    private static void ValidateBatchSize(int count)
    {
        if (count == 0)
        {
            throw new StlApiException(
                "workflow_gates.batch_validation",
                "At least one gate is required for a batch workflow gate check.",
                400);
        }

        if (count > MaxBatchItems)
        {
            throw new StlApiException(
                "workflow_gates.batch_validation",
                $"Batch workflow gate checks are limited to {MaxBatchItems} gates per request.",
                400);
        }
    }

    private static IReadOnlyList<WorkflowGateBatchCheckItem> NormalizeBatchItems(
        IReadOnlyList<WorkflowGateBatchCheckItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            return [];
        }

        return items
            .GroupBy(item => NormalizeGateKey(item.GateKey))
            .Select(group => group.Last())
            .ToList();
    }

    private static IReadOnlyList<InternalWorkflowGateBatchCheckItem> NormalizeInternalBatchItems(
        IReadOnlyList<InternalWorkflowGateBatchCheckItem>? items)
    {
        if (items is null || items.Count == 0)
        {
            return [];
        }

        return items
            .GroupBy(item => NormalizeGateKey(item.GateKey))
            .Select(group => group.Last())
            .ToList();
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

    private static WorkflowGateBatchCheckSummary BuildBatchSummary(
        IReadOnlyList<WorkflowGateCheckResponse> results)
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

        return new WorkflowGateBatchCheckSummary(results.Count, allowCount, warnCount, blockCount, waivedCount);
    }
}
