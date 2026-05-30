using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ProductGateEvaluationService(
    ComplianceCoreDbContext db,
    WorkflowGateService workflowGateService,
    IComplianceCoreAuditService auditService)
{
    public const string EvaluateActionScope = "compliancecore.product_gates.evaluate";

    public async Task<ProductGateEvaluationResponse> EvaluateAsync(
        ProductGateEvaluationRequest request,
        string? sourceProductKey,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException("product_gates.validation", "Tenant id is required.", 400);
        }

        var workflowKey = NormalizeKey(request.WorkflowKey, "Workflow key");
        var actionKey = NormalizeKey(request.ActionKey, "Action key");
        var activityContextKey = NormalizeKey(request.ActivityContextKey, "Activity context key");
        var subjects = NormalizeSubjects(request.SubjectReferences);
        var factSnapshots = NormalizeFactSnapshots(request.FactSnapshotReferences);
        var context = BuildEvaluationContext(
            workflowKey,
            actionKey,
            activityContextKey,
            subjects,
            factSnapshots,
            request.RuleContext);

        var gateCheck = await workflowGateService.CheckInternalAsync(
            new InternalWorkflowGateCheckRequest(
                request.TenantId,
                workflowKey,
                context,
                request.EmitFindings,
                PersistSnapshot: true),
            sourceProductKey,
            cancellationToken);

        var appliedRules = await LoadAppliedRuleVersionsAsync(
            request.TenantId,
            gateCheck.RulePackId,
            gateCheck.RuleEvaluationRunId,
            cancellationToken);
        var citations = await LoadCitationReferencesAsync(
            request.TenantId,
            gateCheck.RulePackId,
            cancellationToken);
        var evidenceRequirements = await LoadEvidenceRequirementsAsync(
            request.TenantId,
            gateCheck.RulePackId,
            cancellationToken);
        var missingFacts = gateCheck.Reasons
            .Where(reason => string.Equals(reason.Code, "fact_unresolved", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(reason.FactKey))
            .Select(reason => reason.FactKey!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(factKey => factKey)
            .ToList();
        var staleFacts = factSnapshots
            .Where(snapshot => snapshot.ExpiresAt is not null && snapshot.ExpiresAt.Value <= gateCheck.CheckedAt)
            .Select(snapshot => new ProductGateStaleFactReference(
                snapshot.FactKey,
                snapshot.SnapshotReference,
                snapshot.CapturedAt,
                snapshot.ExpiresAt!.Value))
            .ToList();
        var remediationHints = BuildRemediationHints(gateCheck.Reasons);

        await auditService.WriteAsync(
            "product_gates.evaluate",
            request.TenantId,
            actorUserId: null,
            "workflow_gate_check_result",
            gateCheck.CheckResultId.ToString(),
            gateCheck.Outcome,
            reasonCode: sourceProductKey,
            cancellationToken: cancellationToken);

        return new ProductGateEvaluationResponse(
            TraceId: gateCheck.CheckResultId,
            request.TenantId,
            gateCheck.GateKey,
            actionKey,
            activityContextKey,
            subjects,
            gateCheck.CheckResultId,
            gateCheck.RuleEvaluationRunId,
            gateCheck.Outcome,
            gateCheck.ReasonCode,
            gateCheck.Message,
            appliedRules,
            citations,
            missingFacts,
            staleFacts,
            evidenceRequirements,
            remediationHints,
            gateCheck.AppliedWaiverId,
            gateCheck.AppliedWaiverKey,
            gateCheck.RuleEvaluationRunId is null
                ? null
                : $"/api/v1/evaluations/{gateCheck.RuleEvaluationRunId}/audit-export",
            gateCheck.CheckedAt);
    }

    private async Task<IReadOnlyList<ProductGateAppliedRuleVersion>> LoadAppliedRuleVersionsAsync(
        Guid tenantId,
        Guid rulePackId,
        Guid? evaluationRunId,
        CancellationToken cancellationToken)
    {
        var version = await db.RulePacks
            .AsNoTracking()
            .Where(pack => pack.TenantId == tenantId && pack.Id == rulePackId)
            .Select(pack => pack.VersionNumber)
            .FirstAsync(cancellationToken);

        if (evaluationRunId is null)
        {
            return [];
        }

        var run = await db.RuleEvaluationRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == evaluationRunId.Value,
                cancellationToken);
        if (run is null || string.IsNullOrWhiteSpace(run.RuleResultsJson))
        {
            return [];
        }

        var rules = JsonSerializer.Deserialize<List<RuleEvaluationItemResponse>>(
                run.RuleResultsJson,
                RuleEvaluationJson.Options)
            ?? [];

        return rules
            .Select(rule => new ProductGateAppliedRuleVersion(
                rule.RuleKey,
                rule.Label,
                rule.Result,
                rule.Message,
                version,
                rule.NonWaivable))
            .ToList();
    }

    private async Task<IReadOnlyList<ProductGateCitationReference>> LoadCitationReferencesAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken)
    {
        var direct = await db.RegulatoryCitations
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive && x.RulePackId == rulePackId)
            .Select(x => new ProductGateCitationReference(
                x.Id,
                x.CitationKey,
                x.SourceReference,
                x.VersionNumber))
            .ToListAsync(cancellationToken);

        var mapped = await (
            from mapping in db.RegulatoryMappings.AsNoTracking()
            join citation in db.RegulatoryCitations.AsNoTracking()
                on mapping.CitationId equals citation.Id
            where mapping.TenantId == tenantId
                && mapping.IsActive
                && mapping.RulePackId == rulePackId
                && citation.IsActive
            select new ProductGateCitationReference(
                citation.Id,
                citation.CitationKey,
                citation.SourceReference,
                citation.VersionNumber))
            .ToListAsync(cancellationToken);

        return direct
            .Concat(mapped)
            .GroupBy(citation => citation.CitationId)
            .Select(group => group.First())
            .OrderBy(citation => citation.CitationKey)
            .ToList();
    }

    private async Task<IReadOnlyList<ProductGateEvidenceRequirement>> LoadEvidenceRequirementsAsync(
        Guid tenantId,
        Guid rulePackId,
        CancellationToken cancellationToken)
    {
        return await (
            from requirement in db.FactRequirements.AsNoTracking()
            join fact in db.FactDefinitions.AsNoTracking()
                on requirement.FactDefinitionId equals fact.Id
            join citation in db.RegulatoryCitations.AsNoTracking()
                on requirement.CitationId equals citation.Id into citationJoin
            from citation in citationJoin.DefaultIfEmpty()
            where requirement.TenantId == tenantId
                && requirement.IsActive
                && requirement.RulePackId == rulePackId
            orderby requirement.RequirementKey
            select new ProductGateEvidenceRequirement(
                requirement.Id,
                requirement.RequirementKey,
                fact.FactKey,
                requirement.Label,
                requirement.Description,
                requirement.IsRequired,
                citation != null ? citation.CitationKey : null))
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<ProductGateRemediationHint> BuildRemediationHints(
        IReadOnlyList<WorkflowGateReasonResponse> reasons)
    {
        return reasons
            .Select(reason => new ProductGateRemediationHint(
                reason.Code,
                reason.Message,
                reason.RuleKey,
                reason.FactKey))
            .ToList();
    }

    private static Dictionary<string, string> BuildEvaluationContext(
        string workflowKey,
        string actionKey,
        string activityContextKey,
        IReadOnlyList<ProductGateSubjectReference> subjects,
        IReadOnlyList<ProductGateFactSnapshotReference> factSnapshots,
        IReadOnlyDictionary<string, string>? ruleContext)
    {
        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["workflow.key"] = workflowKey,
            ["workflow_key"] = workflowKey,
            ["action_key"] = actionKey,
            ["activity_context_key"] = activityContextKey,
        };

        foreach (var subject in subjects)
        {
            var subjectType = NormalizeContextKeySegment(subject.SubjectType);
            context[$"subject.{subjectType}.reference"] = subject.SubjectReference;
            context[$"{subjectType}_reference"] = subject.SubjectReference;

            if (Guid.TryParse(subject.SubjectReference, out _))
            {
                context[$"{subjectType}_id"] = subject.SubjectReference;
            }

            if (!string.IsNullOrWhiteSpace(subject.SourceProduct))
            {
                context[$"subject.{subjectType}.source_product"] = subject.SourceProduct!.Trim().ToLowerInvariant();
            }
        }

        foreach (var snapshot in factSnapshots)
        {
            context[$"fact_snapshot.{snapshot.FactKey}.reference"] = snapshot.SnapshotReference;
            context[$"fact_snapshot.{snapshot.FactKey}.captured_at"] = snapshot.CapturedAt.ToString("O");
            if (snapshot.ExpiresAt is not null)
            {
                context[$"fact_snapshot.{snapshot.FactKey}.expires_at"] = snapshot.ExpiresAt.Value.ToString("O");
            }
        }

        if (ruleContext is null)
        {
            return context;
        }

        foreach (var (key, value) in ruleContext)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            context[key.Trim()] = value.Trim();
        }

        return context;
    }

    private static IReadOnlyList<ProductGateSubjectReference> NormalizeSubjects(
        IReadOnlyList<ProductGateSubjectReference>? subjects)
    {
        if (subjects is null || subjects.Count == 0)
        {
            throw new StlApiException(
                "product_gates.validation",
                "At least one subject reference is required.",
                400);
        }

        return subjects
            .Select(subject => new ProductGateSubjectReference(
                NormalizeKey(subject.SubjectType, "Subject type"),
                NormalizeReference(subject.SubjectReference, "Subject reference"),
                NormalizeOptionalKey(subject.SourceProduct),
                string.IsNullOrWhiteSpace(subject.DisplayLabel) ? null : subject.DisplayLabel.Trim()))
            .ToList();
    }

    private static IReadOnlyList<ProductGateFactSnapshotReference> NormalizeFactSnapshots(
        IReadOnlyList<ProductGateFactSnapshotReference>? snapshots)
    {
        if (snapshots is null || snapshots.Count == 0)
        {
            return [];
        }

        return snapshots
            .Select(snapshot => new ProductGateFactSnapshotReference(
                ProductFactMirrorRules.NormalizeFactKey(snapshot.FactKey),
                NormalizeReference(snapshot.SnapshotReference, "Snapshot reference"),
                snapshot.CapturedAt,
                snapshot.ExpiresAt))
            .ToList();
    }

    private static string NormalizeReference(string value, string label)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new StlApiException(
                "product_gates.validation",
                $"{label} is required.",
                400);
        }

        return normalized;
    }

    private static string NormalizeKey(string value, string label)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException(
                "product_gates.validation",
                $"{label} is required.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeContextKeySegment(string value) =>
        new(value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
            .ToArray());
}
