using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class QualificationReportService(TrainArrDbContext db)
{
    private const int RecentLimit = 25;
    private const int ExpiringSoonDays = 30;

    public async Task<QualificationReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.AddDays(ExpiringSoonDays);

        var qualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = qualifications
            .Where(x => MatchesStatusFilter(x, status))
            .ToList();

        var issuedCount = qualifications.Count(x => string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase));
        var expiredCount = qualifications.Count(x => string.Equals(x.Status, "expired", StringComparison.OrdinalIgnoreCase));
        var suspendedCount = qualifications.Count(x => string.Equals(x.Status, "suspended", StringComparison.OrdinalIgnoreCase));
        var revokedCount = qualifications.Count(x => string.Equals(x.Status, "revoked", StringComparison.OrdinalIgnoreCase));
        var expiringSoon = qualifications.Count(x =>
            string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase)
            && x.ExpiresAt is not null
            && x.ExpiresAt <= expiringThreshold
            && x.ExpiresAt >= now);

        var recent = filtered
            .OrderByDescending(x => x.IssuedAt)
            .Take(RecentLimit)
            .Select(x => new QualificationReportSummaryItem(
                x.Id,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase)
                && x.ExpiresAt is not null
                && x.ExpiresAt <= expiringThreshold
                && x.ExpiresAt >= now))
            .ToList();

        return new QualificationReportSummaryResponse(
            filtered.Count,
            issuedCount,
            expiredCount,
            suspendedCount,
            revokedCount,
            expiringSoon,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, status, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "qualificationIssueId,staffarrPersonId,qualificationKey,qualificationName,status,issuedAt,expiresAt,expiringSoon");

        foreach (var item in summary.RecentQualifications)
        {
            builder.Append(CsvEscape(item.QualificationIssueId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.QualificationKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.QualificationName));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.IssuedAt.ToString("O")));
            builder.Append(',');
            builder.Append(CsvEscape(item.ExpiresAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.AppendLine(item.ExpiringSoon ? "true" : "false");
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        return new CsvExportResult(
            "text/csv",
            $"trainarr-qualification-report-{timestamp}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static bool MatchesStatusFilter(QualificationIssue issue, string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(issue.Status, status.Trim(), StringComparison.OrdinalIgnoreCase);
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
