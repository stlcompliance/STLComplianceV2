using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Services;

public sealed class OperatorDashboardService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string ReadAction = "operator_dashboard.read";

    public async Task<OperatorDashboardResponse> GetOperatorDashboardAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var dayAgo = now.AddHours(-24);

        var findingsQuery = db.ComplianceFindings.Where(f => f.TenantId == tenantId);
        var rulePacksQuery = db.RulePacks.Where(p => p.TenantId == tenantId);
        var evaluationsQuery = db.RuleEvaluationRuns.Where(r => r.TenantId == tenantId);
        var gateDefinitionsQuery = db.WorkflowGateDefinitions.Where(g => g.TenantId == tenantId);
        var gateChecksQuery = db.WorkflowGateCheckResults.Where(r => r.TenantId == tenantId);
        var auditQuery = db.AuditEvents.Where(e => e.TenantId == tenantId);

        var openCount = await findingsQuery.CountAsync(
            f => f.Status == FindingStatuses.Open,
            cancellationToken);
        var openBlockCount = await findingsQuery.CountAsync(
            f => f.Status == FindingStatuses.Open && f.Severity == FindingSeverities.Block,
            cancellationToken);
        var openWarnCount = await findingsQuery.CountAsync(
            f => f.Status == FindingStatuses.Open && f.Severity == FindingSeverities.Warn,
            cancellationToken);
        var acknowledgedCount = await findingsQuery.CountAsync(
            f => f.Status == FindingStatuses.Acknowledged,
            cancellationToken);
        var resolvedCount = await findingsQuery.CountAsync(
            f => f.Status == FindingStatuses.Resolved,
            cancellationToken);
        var findingsTotal = await findingsQuery.CountAsync(cancellationToken);

        var draftCount = await rulePacksQuery.CountAsync(
            p => p.Status == RulePackStatuses.Draft,
            cancellationToken);
        var reviewCount = await rulePacksQuery.CountAsync(
            p => p.Status == RulePackStatuses.Review,
            cancellationToken);
        var publishedCount = await rulePacksQuery.CountAsync(
            p => p.Status == RulePackStatuses.Published,
            cancellationToken);
        var archivedCount = await rulePacksQuery.CountAsync(
            p => p.Status == RulePackStatuses.Archived,
            cancellationToken);
        var rulePackTotal = await rulePacksQuery.CountAsync(cancellationToken);

        var evaluationTotal = await evaluationsQuery.CountAsync(cancellationToken);
        var evaluationsLast24Hours = await evaluationsQuery.CountAsync(
            r => r.CreatedAt >= dayAgo,
            cancellationToken);
        var passCount = await evaluationsQuery.CountAsync(
            r => r.OverallResult == RuleEvaluationResults.Pass,
            cancellationToken);
        var failCount = await evaluationsQuery.CountAsync(
            r => r.OverallResult == RuleEvaluationResults.Fail,
            cancellationToken);

        var gateDefinitionCount = await gateDefinitionsQuery.CountAsync(cancellationToken);
        var gateCheckTotal = await gateChecksQuery.CountAsync(cancellationToken);
        var gateChecksLast24Hours = await gateChecksQuery.CountAsync(
            r => r.CreatedAt >= dayAgo,
            cancellationToken);
        var gateBlockCount = await gateChecksQuery.CountAsync(
            r => r.Outcome == ComplianceEvaluationOutcomes.Block,
            cancellationToken);
        var gateWarnCount = await gateChecksQuery.CountAsync(
            r => r.Outcome == ComplianceEvaluationOutcomes.Warn,
            cancellationToken);
        var gateAllowCount = await gateChecksQuery.CountAsync(
            r => r.Outcome == ComplianceEvaluationOutcomes.Allow,
            cancellationToken);

        var auditTotal = await auditQuery.CountAsync(cancellationToken);
        var auditLast24Hours = await auditQuery.CountAsync(
            e => e.OccurredAt >= dayAgo,
            cancellationToken);
        var auditSuccess = await auditQuery.CountAsync(
            e => e.Result == "success",
            cancellationToken);
        var auditFailure = await auditQuery.CountAsync(
            e => e.Result != "success",
            cancellationToken);

        var recentEvaluations = await (
            from run in db.RuleEvaluationRuns.AsNoTracking()
            join pack in db.RulePacks.AsNoTracking() on run.RulePackId equals pack.Id
            where run.TenantId == tenantId
            orderby run.CreatedAt descending
            select new OperatorDashboardRecentEvaluation(
                run.Id,
                run.RulePackId,
                pack.Label,
                pack.PackKey,
                run.OverallResult,
                run.CreatedAt))
            .Take(8)
            .ToListAsync(cancellationToken);

        await auditService.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "operator_dashboard",
            "operator",
            "success",
            cancellationToken: cancellationToken);

        return new OperatorDashboardResponse(
            new OperatorDashboardFindingsSummary(
                openCount,
                openBlockCount,
                openWarnCount,
                acknowledgedCount,
                resolvedCount,
                findingsTotal),
            new OperatorDashboardRulePackSummary(
                draftCount,
                reviewCount,
                publishedCount,
                archivedCount,
                rulePackTotal),
            new OperatorDashboardEvaluationsSummary(
                evaluationTotal,
                evaluationsLast24Hours,
                passCount,
                failCount),
            new OperatorDashboardWorkflowGateSummary(
                gateDefinitionCount,
                gateCheckTotal,
                gateChecksLast24Hours,
                gateBlockCount,
                gateWarnCount,
                gateAllowCount),
            new OperatorDashboardAuditSummary(
                auditTotal,
                auditLast24Hours,
                auditSuccess,
                auditFailure),
            recentEvaluations,
            now);
    }
}
