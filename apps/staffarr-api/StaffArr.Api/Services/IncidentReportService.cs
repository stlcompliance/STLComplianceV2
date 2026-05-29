using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class IncidentReportService(StaffArrDbContext db)
{
    private const int RecentLimit = 25;

    private static readonly HashSet<string> OpenStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "open",
        "in_review",
    };

    private static readonly HashSet<string> HighSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "high",
        "critical",
    };

    public async Task<IncidentReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        string? severity,
        bool openOnly,
        CancellationToken cancellationToken = default)
    {
        var incidents = await db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = incidents
            .Where(x => MatchesStatusFilter(x, status))
            .Where(x => MatchesSeverityFilter(x, severity))
            .Where(x => !openOnly || OpenStatuses.Contains(x.Status))
            .ToList();

        var openCount = incidents.Count(x => OpenStatuses.Contains(x.Status));
        var closedCount = incidents.Count(x =>
            string.Equals(x.Status, "closed", StringComparison.OrdinalIgnoreCase));
        var highSeverityOpenCount = incidents.Count(x =>
            OpenStatuses.Contains(x.Status) && HighSeverities.Contains(x.Severity));

        var recent = filtered
            .OrderByDescending(x => x.ReportedAt)
            .Take(RecentLimit)
            .Select(x => new IncidentReportSummaryItem(
                x.Id,
                x.PersonId,
                x.ReasonCategoryKey,
                x.Severity,
                x.Status,
                x.Title,
                x.OccurredAt,
                x.ReportedAt))
            .ToList();

        return new IncidentReportSummaryResponse(
            filtered.Count,
            openCount,
            closedCount,
            highSeverityOpenCount,
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
        builder.AppendLine(
            "incidentId,personId,reasonCategoryKey,severity,status,title,occurredAt,reportedAt");

        foreach (var item in summary.RecentIncidents)
        {
            builder.Append(CsvEscape(item.IncidentId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReasonCategoryKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.Title));
            builder.Append(',');
            builder.Append(CsvEscape(item.OccurredAt.ToString("O")));
            builder.AppendLine(CsvEscape(item.ReportedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"staffarr-incident-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    private static bool MatchesStatusFilter(PersonnelIncident incident, string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(incident.Status, status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesSeverityFilter(PersonnelIncident incident, string? severity)
    {
        if (string.IsNullOrWhiteSpace(severity) || string.Equals(severity, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(incident.Severity, severity.Trim(), StringComparison.OrdinalIgnoreCase);
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
