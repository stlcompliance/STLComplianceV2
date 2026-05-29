using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class FindingsReportService(ComplianceCoreDbContext db)
{
    private const int RecentLimit = 25;

    public async Task<FindingsReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        string? severity,
        bool openOnly,
        CancellationToken cancellationToken = default)
    {
        var findings = await (
            from finding in db.ComplianceFindings.AsNoTracking()
            join pack in db.RulePacks.AsNoTracking() on finding.RulePackId equals pack.Id
            where finding.TenantId == tenantId
            orderby finding.CreatedAt descending
            select new
            {
                finding.Id,
                finding.FindingKey,
                finding.Severity,
                finding.Status,
                finding.Title,
                finding.CreatedAt,
                pack.PackKey,
            })
            .ToListAsync(cancellationToken);

        var filtered = findings
            .Where(x => MatchesStatusFilter(x.Status, status, openOnly))
            .Where(x => MatchesSeverityFilter(x.Severity, severity))
            .ToList();

        var openCount = findings.Count(x => x.Status == FindingStatuses.Open);
        var acknowledgedCount = findings.Count(x => x.Status == FindingStatuses.Acknowledged);
        var resolvedCount = findings.Count(x => x.Status == FindingStatuses.Resolved);
        var openBlockCount = findings.Count(x =>
            x.Status == FindingStatuses.Open && x.Severity == FindingSeverities.Block);
        var openWarnCount = findings.Count(x =>
            x.Status == FindingStatuses.Open && x.Severity == FindingSeverities.Warn);

        var recent = filtered
            .Take(RecentLimit)
            .Select(x => new FindingsReportSummaryItem(
                x.Id,
                x.FindingKey,
                x.Severity,
                x.Status,
                x.Title,
                x.PackKey,
                x.CreatedAt))
            .ToList();

        return new FindingsReportSummaryResponse(
            filtered.Count,
            openCount,
            acknowledgedCount,
            resolvedCount,
            openBlockCount,
            openWarnCount,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? status,
        string? severity,
        bool openOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, status, severity, openOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("findingId,findingKey,severity,status,title,packKey,createdAt");

        foreach (var item in summary.RecentFindings)
        {
            builder.Append(CsvEscape(item.FindingId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.FindingKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.Title));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.AppendLine(CsvEscape(item.CreatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-findings-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static bool MatchesStatusFilter(string findingStatus, string? status, bool openOnly)
    {
        if (openOnly && !string.Equals(findingStatus, FindingStatuses.Open, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(status)
            || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(findingStatus, status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesSeverityFilter(string findingSeverity, string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity)
            || string.Equals(severity, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(findingSeverity, severity.Trim(), StringComparison.OrdinalIgnoreCase);
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
