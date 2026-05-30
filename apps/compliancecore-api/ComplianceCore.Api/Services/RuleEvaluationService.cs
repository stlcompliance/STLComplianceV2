using System.Text.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class RuleEvaluationService(
    ComplianceCoreDbContext db,
    ComplianceFindingService findingService,
    IComplianceCoreAuditService auditService)
{
    public async Task<RuleEvaluationRunResponse> EvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid rulePackId,
        EvaluateRulePackRequest request,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);

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
        var facts = request.Facts ?? new Dictionary<string, bool>();
        var (overallResult, ruleResults) = RuleEvaluator.Evaluate(content, facts);

        var now = DateTimeOffset.UtcNow;
        var factInputsJson = JsonSerializer.Serialize(facts, RuleEvaluationJson.Options);
        var ruleResultsJson = JsonSerializer.Serialize(ruleResults, RuleEvaluationJson.Options);

        var run = new RuleEvaluationRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = rulePackId,
            ActorUserId = actorUserId,
            Status = RuleEvaluationRunStatuses.Completed,
            OverallResult = overallResult,
            FactInputsJson = factInputsJson,
            RuleResultsJson = ruleResultsJson,
            CreatedAt = now,
        };

        db.RuleEvaluationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rule_evaluation.run",
            tenantId,
            actorUserId,
            "rule_evaluation_run",
            run.Id.ToString(),
            "success",
            reasonCode: overallResult,
            cancellationToken: cancellationToken);

        IReadOnlyList<ComplianceFindingResponse> findingsEmitted = [];
        if (request.EmitFindings &&
            !string.Equals(overallResult, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
        {
            findingsEmitted = await findingService.EmitFromEvaluationAsync(
                tenantId,
                rulePackId,
                rulePack.PackKey,
                run.Id,
                overallResult,
                [],
                ruleResults,
                cancellationToken);
        }

        return MapResponse(run, rulePack, facts, ruleResults, findingsEmitted);
    }

    public async Task<RuleEvaluationRunResponse> PersistInternalEvaluationSnapshotAsync(
        Guid tenantId,
        Guid rulePackId,
        IReadOnlyDictionary<string, bool> facts,
        IReadOnlyList<RuleEvaluationItemResponse> ruleResults,
        string overallResult,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks.AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == rulePackId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var factInputsJson = JsonSerializer.Serialize(facts, RuleEvaluationJson.Options);
        var ruleResultsJson = JsonSerializer.Serialize(ruleResults, RuleEvaluationJson.Options);

        var run = new RuleEvaluationRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RulePackId = rulePackId,
            ActorUserId = null,
            Status = RuleEvaluationRunStatuses.Completed,
            OverallResult = overallResult,
            FactInputsJson = factInputsJson,
            RuleResultsJson = ruleResultsJson,
            CreatedAt = now,
        };

        db.RuleEvaluationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        return MapResponse(run, rulePack, facts, ruleResults, []);
    }

    public async Task<IReadOnlyList<RuleEvaluationRunResponse>> ListAsync(
        Guid tenantId,
        Guid? rulePackId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (rulePackId.HasValue)
        {
            query = query.Where(x => x.RulePackId == rulePackId.Value);
        }

        var runs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Join(
                db.RulePacks.AsNoTracking(),
                run => run.RulePackId,
                pack => pack.Id,
                (run, pack) => new { run, pack })
            .ToListAsync(cancellationToken);

        return runs
            .Select(x => MapResponse(
                x.run,
                x.pack,
                DeserializeFacts(x.run.FactInputsJson),
                DeserializeRuleResults(x.run.RuleResultsJson),
                []))
            .ToList();
    }

    public async Task<RuleEvaluationRunResponse> GetAsync(
        Guid tenantId,
        Guid evaluationRunId,
        CancellationToken cancellationToken = default)
    {
        var result = await db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == evaluationRunId)
            .Join(
                db.RulePacks.AsNoTracking(),
                run => run.RulePackId,
                pack => pack.Id,
                (run, pack) => new { run, pack })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new StlApiException("rule_evaluation.not_found", "Rule evaluation run was not found.", 404);
        }

        return MapResponse(
            result.run,
            result.pack,
            DeserializeFacts(result.run.FactInputsJson),
            DeserializeRuleResults(result.run.RuleResultsJson),
            []);
    }

    public async Task<RuleEvaluationAuditExportResponse> BuildAuditExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid evaluationRunId,
        CancellationToken cancellationToken = default)
    {
        var evaluation = await db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == evaluationRunId)
            .Join(
                db.RulePacks.AsNoTracking(),
                run => run.RulePackId,
                pack => pack.Id,
                (run, pack) => new AuditPackageEvaluationRunItem(
                    run.Id,
                    run.RulePackId,
                    pack.PackKey,
                    run.ActorUserId,
                    run.Status,
                    run.OverallResult,
                    run.FactInputsJson,
                    run.RuleResultsJson,
                    run.AppliedWaiverId,
                    run.AppliedWaiverKey,
                    run.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (evaluation is null)
        {
            throw new StlApiException("rule_evaluation.not_found", "Rule evaluation run was not found.", 404);
        }

        var findings = await db.ComplianceFindings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RuleEvaluationRunId == evaluationRunId)
            .Join(
                db.RulePacks.AsNoTracking(),
                finding => finding.RulePackId,
                pack => pack.Id,
                (finding, pack) => new AuditPackageFindingItem(
                    finding.Id,
                    finding.RulePackId,
                    pack.PackKey,
                    finding.RuleEvaluationRunId,
                    finding.FindingKey,
                    finding.Severity,
                    finding.Status,
                    finding.RuleKey,
                    finding.FactKey,
                    finding.Title,
                    finding.Message,
                    finding.ReasonCode,
                    finding.CreatedAt))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var workflowGateChecks = await db.WorkflowGateCheckResults
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.RuleEvaluationRunId == evaluationRunId)
            .Join(
                db.WorkflowGateDefinitions.AsNoTracking(),
                check => check.WorkflowGateDefinitionId,
                gate => gate.Id,
                (check, gate) => new { check, gate })
            .Join(
                db.RulePacks.AsNoTracking(),
                item => item.gate.RulePackId,
                pack => pack.Id,
                (item, pack) => new AuditPackageWorkflowGateCheckItem(
                    item.check.Id,
                    item.check.GateKey,
                    pack.Id,
                    pack.PackKey,
                    item.check.RuleEvaluationRunId,
                    item.check.Outcome,
                    item.check.ReasonCode,
                    item.check.Message,
                    item.check.AppliedWaiverId,
                    item.check.AppliedWaiverKey,
                    item.check.CreatedAt))
            .OrderBy(x => x.CheckedAt)
            .ToListAsync(cancellationToken);

        var waiverIds = workflowGateChecks
            .Select(x => x.AppliedWaiverId)
            .Append(evaluation.AppliedWaiverId)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        IReadOnlyList<AuditPackageWaiverItem> waivers = waiverIds.Count == 0
            ? []
            : await db.ComplianceWaivers
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && waiverIds.Contains(x.Id))
                .Select(x => new AuditPackageWaiverItem(
                    x.Id,
                    x.WaiverKey,
                    x.RulePackId,
                    x.PackKey,
                    x.RuleKey,
                    x.GateKey,
                    x.SubjectScopeKey,
                    x.ReasonCode,
                    x.Explanation,
                    x.Status,
                    x.EffectiveAt,
                    x.ExpiresAt,
                    x.ApprovedByUserId,
                    x.ApprovedAt,
                    x.CreatedAt))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

        var export = new RuleEvaluationAuditExportResponse(
            Guid.NewGuid(),
            tenantId,
            DateTimeOffset.UtcNow,
            evaluation,
            workflowGateChecks,
            findings,
            waivers);

        await auditService.WriteAsync(
            "rule_evaluation.audit_export",
            tenantId,
            actorUserId,
            "rule_evaluation_run",
            evaluationRunId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return export;
    }

    public async Task<RuleEvaluationSimulationResponse> SimulateAsync(
        Guid tenantId,
        Guid rulePackId,
        IReadOnlyDictionary<string, bool> facts,
        CancellationToken cancellationToken = default)
    {
        var rulePack = await db.RulePacks.AsNoTracking().FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == rulePackId && x.IsActive,
            cancellationToken);

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
        var normalizedFacts = facts ?? new Dictionary<string, bool>();
        var (overallResult, ruleResults) = RuleEvaluator.Evaluate(content, normalizedFacts);
        return new RuleEvaluationSimulationResponse(
            rulePack.Id,
            rulePack.PackKey,
            rulePack.Label,
            rulePack.VersionNumber,
            overallResult,
            normalizedFacts,
            ruleResults,
            DateTimeOffset.UtcNow);
    }

    public async Task<RuleEvaluationRunResponse> ReEvaluateAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid evaluationRunId,
        bool emitFindings = false,
        CancellationToken cancellationToken = default)
    {
        var priorRun = await db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == evaluationRunId)
            .Select(x => new
            {
                x.RulePackId,
                x.FactInputsJson,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (priorRun is null)
        {
            throw new StlApiException("rule_evaluation.not_found", "Rule evaluation run was not found.", 404);
        }

        return await EvaluateAsync(
            tenantId,
            actorUserId,
            priorRun.RulePackId,
            new EvaluateRulePackRequest(DeserializeFacts(priorRun.FactInputsJson), emitFindings),
            cancellationToken);
    }

    public async Task<RuleEvaluationExplanationResponse> BuildExplanationAsync(
        Guid tenantId,
        Guid evaluationRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await GetAsync(tenantId, evaluationRunId, cancellationToken);
        var failedRuleKeys = run.RuleResults
            .Where(item => !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.RuleKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();
        var missingFactKeys = run.RuleResults
            .Where(item => item.Message.Contains("was not provided", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.RuleKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        var summary = $"{run.OverallResult}: {failedRuleKeys.Count} failed rule(s), {missingFactKeys.Count} missing fact-derived failure(s).";
        return new RuleEvaluationExplanationResponse(
            run.EvaluationRunId,
            run.RulePackId,
            run.PackKey,
            run.OverallResult,
            summary,
            failedRuleKeys,
            missingFactKeys,
            run.RuleResults,
            DateTimeOffset.UtcNow);
    }

    private static RuleEvaluationRunResponse MapResponse(
        RuleEvaluationRun run,
        RulePack pack,
        IReadOnlyDictionary<string, bool> facts,
        IReadOnlyList<RuleEvaluationItemResponse> ruleResults,
        IReadOnlyList<ComplianceFindingResponse> findingsEmitted) =>
        new(
            run.Id,
            run.RulePackId,
            pack.PackKey,
            pack.Label,
            pack.VersionNumber,
            run.Status,
            run.OverallResult,
            facts,
            ruleResults,
            run.CreatedAt,
            findingsEmitted,
            run.AppliedWaiverId,
            run.AppliedWaiverKey);

    private static IReadOnlyDictionary<string, bool> DeserializeFacts(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, bool>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, bool>>(json, RuleEvaluationJson.Options)
            ?? new Dictionary<string, bool>();
    }

    private static IReadOnlyList<RuleEvaluationItemResponse> DeserializeRuleResults(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<RuleEvaluationItemResponse>();
        }

        return JsonSerializer.Deserialize<List<RuleEvaluationItemResponse>>(json, RuleEvaluationJson.Options)
            ?? [];
    }
}
