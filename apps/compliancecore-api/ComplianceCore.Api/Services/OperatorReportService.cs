using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class OperatorReportService(ComplianceCoreDbContext db)
{
    private const int RecentLimit = 25;

    public async Task<OperatorReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var dayAgo = now.AddHours(-24);

        var evaluationsQuery = db.RuleEvaluationRuns.AsNoTracking().Where(x => x.TenantId == tenantId);
        var evaluationTotal = await evaluationsQuery.CountAsync(cancellationToken);
        var passCount = await evaluationsQuery.CountAsync(
            x => x.OverallResult == RuleEvaluationResults.Pass,
            cancellationToken);
        var failCount = await evaluationsQuery.CountAsync(
            x => x.OverallResult == RuleEvaluationResults.Fail,
            cancellationToken);
        var evaluationsLast24Hours = await evaluationsQuery.CountAsync(
            x => x.CreatedAt >= dayAgo,
            cancellationToken);

        var gateDefinitionCount = await db.WorkflowGateDefinitions
            .AsNoTracking()
            .CountAsync(x => x.TenantId == tenantId, cancellationToken);
        var gateBlockCount = await db.WorkflowGateCheckResults
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId && x.Outcome == ComplianceEvaluationOutcomes.Block,
                cancellationToken);
        var gateWarnCount = await db.WorkflowGateCheckResults
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId && x.Outcome == ComplianceEvaluationOutcomes.Warn,
                cancellationToken);

        var rulePacksQuery = db.RulePacks.AsNoTracking().Where(x => x.TenantId == tenantId);
        var publishedCount = await rulePacksQuery.CountAsync(
            x => x.Status == RulePackStatuses.Published,
            cancellationToken);
        var draftCount = await rulePacksQuery.CountAsync(
            x => x.Status == RulePackStatuses.Draft,
            cancellationToken);

        var recentEvaluations = await (
            from run in db.RuleEvaluationRuns.AsNoTracking()
            join pack in db.RulePacks.AsNoTracking() on run.RulePackId equals pack.Id
            where run.TenantId == tenantId
            orderby run.CreatedAt descending
            select new OperatorReportSummaryItem(
                run.Id,
                pack.Label,
                pack.PackKey,
                run.OverallResult,
                run.CreatedAt))
            .Take(RecentLimit)
            .ToListAsync(cancellationToken);

        var filtered = attentionOnly
            ? recentEvaluations
                .Where(x => string.Equals(x.OverallResult, RuleEvaluationResults.Fail, StringComparison.OrdinalIgnoreCase))
                .ToList()
            : recentEvaluations;

        var attentionCount = failCount + gateBlockCount + gateWarnCount + (publishedCount == 0 ? 1 : 0);

        return new OperatorReportSummaryResponse(
            evaluationTotal,
            passCount,
            failCount,
            evaluationsLast24Hours,
            gateDefinitionCount,
            gateBlockCount,
            gateWarnCount,
            publishedCount,
            draftCount,
            attentionCount,
            filtered);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, attentionOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("evaluationRunId,rulePackLabel,packKey,overallResult,createdAt");

        foreach (var item in summary.RecentEvaluations)
        {
            builder.Append(CsvEscape(item.EvaluationRunId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.OverallResult));
            builder.AppendLine(CsvEscape(item.CreatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-operator-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
