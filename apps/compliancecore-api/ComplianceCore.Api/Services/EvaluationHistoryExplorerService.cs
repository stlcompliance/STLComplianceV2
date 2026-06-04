using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class EvaluationHistoryExplorerService(ComplianceCoreDbContext db)
{
    private const int DefaultLimit = 25;
    private const int MaxLimit = 100;

    public async Task<EvaluationHistoryExplorerResponse> GetSummaryAsync(
        Guid tenantId,
        Guid? rulePackId = null,
        string? overallResult = null,
        string? status = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.RuleEvaluationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (rulePackId.HasValue)
        {
            query = query.Where(x => x.RulePackId == rulePackId.Value);
        }

        if (!string.IsNullOrWhiteSpace(overallResult))
        {
            query = query.Where(x => x.OverallResult == overallResult.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        var totalRuns = await query.CountAsync(cancellationToken);
        var passedCount = await query.CountAsync(x => x.OverallResult == RuleEvaluationResults.Pass, cancellationToken);
        var failedCount = await query.CountAsync(x => x.OverallResult == RuleEvaluationResults.Fail, cancellationToken);
        var cappedLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
        var cappedOffset = Math.Max(0, offset ?? 0);

        var runs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(cappedOffset)
            .Take(cappedLimit)
            .Join(
                db.RulePacks.AsNoTracking(),
                run => run.RulePackId,
                pack => pack.Id,
                (run, pack) => new EvaluationHistoryExplorerItemResponse(
                    run.Id,
                    run.RulePackId,
                    pack.PackKey,
                    pack.Label,
                    run.OverallResult,
                    run.Status,
                    run.ActorUserId,
                    run.CreatedAt))
            .ToListAsync(cancellationToken);

        return new EvaluationHistoryExplorerResponse(
            tenantId,
            rulePackId,
            await ResolvePackKeyAsync(rulePackId, cancellationToken),
            totalRuns,
            passedCount,
            failedCount,
            cappedLimit,
            cappedOffset,
            cappedOffset + runs.Count < totalRuns,
            DateTimeOffset.UtcNow,
            runs);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        Guid? rulePackId = null,
        string? overallResult = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(
            tenantId,
            rulePackId,
            overallResult,
            status,
            limit: MaxLimit,
            offset: 0,
            cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("evaluationRunId,rulePackId,packKey,packLabel,overallResult,status,actorUserId,createdAt");

        foreach (var item in summary.Runs)
        {
            builder.Append(CsvEscape(item.EvaluationRunId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackLabel));
            builder.Append(',');
            builder.Append(CsvEscape(item.OverallResult));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.ActorUserId?.ToString() ?? string.Empty));
            builder.AppendLine(CsvEscape(item.CreatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-evaluation-history-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private async Task<string?> ResolvePackKeyAsync(Guid? rulePackId, CancellationToken cancellationToken)
    {
        if (!rulePackId.HasValue)
        {
            return null;
        }

        return await db.RulePacks.AsNoTracking()
            .Where(x => x.Id == rulePackId.Value)
            .Select(x => x.PackKey)
            .FirstOrDefaultAsync(cancellationToken);
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
