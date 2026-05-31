using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class ReadinessReportService(StaffArrDbContext db)
{
    private const int RecentLimit = 25;
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(30);

    public async Task<ReadinessReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? scopeType,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.ReadinessRollups
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = rollups
            .Where(x => MatchesScopeTypeFilter(x.ScopeType, scopeType))
            .Where(x => !attentionOnly || x.NotReadyCount > 0)
            .ToList();

        var totalMembers = rollups.Sum(x => x.TotalMembers);
        var readyCount = rollups.Sum(x => x.ReadyCount);
        var notReadyCount = rollups.Sum(x => x.NotReadyCount);
        var overrideCount = rollups.Sum(x => x.OverrideCount);
        var readyPercent = totalMembers == 0 ? 0m : Math.Round(readyCount * 100m / totalMembers, 1);

        var recent = filtered
            .OrderByDescending(x => x.ComputedAt)
            .Take(RecentLimit)
            .Select(x => new ReadinessReportSummaryItem(
                x.Id,
                x.ScopeType,
                x.OrgUnitId,
                x.OrgUnitName,
                x.TotalMembers,
                x.ReadyCount,
                x.NotReadyCount,
                x.ReadyPercent,
                x.ComputedAt))
            .ToList();

        return new ReadinessReportSummaryResponse(
            filtered.Count,
            totalMembers,
            readyCount,
            notReadyCount,
            overrideCount,
            readyPercent,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? scopeType,
        bool attentionOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, scopeType, attentionOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "rollupId,scopeType,orgUnitId,orgUnitName,totalMembers,readyCount,notReadyCount,readyPercent,computedAt");

        foreach (var item in summary.RecentRollups)
        {
            builder.Append(CsvEscape(item.RollupId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.ScopeType));
            builder.Append(',');
            builder.Append(CsvEscape(item.OrgUnitId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.OrgUnitName));
            builder.Append(',');
            builder.Append(item.TotalMembers);
            builder.Append(',');
            builder.Append(item.ReadyCount);
            builder.Append(',');
            builder.Append(item.NotReadyCount);
            builder.Append(',');
            builder.Append(item.ReadyPercent.ToString("F1"));
            builder.Append(',');
            builder.AppendLine(CsvEscape(item.ComputedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"staffarr-readiness-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<IReadOnlyList<ReadinessReportAlertResponse>> ListAlertsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var alerts = new List<ReadinessReportAlertResponse>();

        var people = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var expiringCertifications = await (
            from certification in db.PersonCertifications.AsNoTracking()
            join definition in db.CertificationDefinitions.AsNoTracking()
                on certification.CertificationDefinitionId equals definition.Id
            where certification.TenantId == tenantId
                && definition.TenantId == tenantId
                && certification.Status == "active"
                && certification.ExpiresAt != null
                && certification.ExpiresAt >= now
                && certification.ExpiresAt <= now.Add(ExpiringSoonWindow)
            orderby certification.ExpiresAt
            select new
            {
                certification.PersonId,
                definition.Name,
                definition.CertificationKey,
                ExpiresAt = certification.ExpiresAt!.Value,
                certification.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var certification in expiringCertifications)
        {
            if (!people.TryGetValue(certification.PersonId, out var person))
            {
                continue;
            }

            alerts.Add(new ReadinessReportAlertResponse(
                "certification_expiring",
                "medium",
                person.Id,
                person.DisplayName,
                $"Certification '{certification.Name}' ({certification.CertificationKey}) expires on {certification.ExpiresAt:O}.",
                certification.UpdatedAt));
        }

        var expiringOverrides = await db.PersonReadinessOverrides
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == "active"
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= now.Add(ExpiringSoonWindow))
            .OrderBy(x => x.ExpiresAt)
            .ToListAsync(cancellationToken);

        foreach (var overrideItem in expiringOverrides)
        {
            if (!people.TryGetValue(overrideItem.PersonId, out var person))
            {
                continue;
            }

            alerts.Add(new ReadinessReportAlertResponse(
                "override_expiring",
                "high",
                person.Id,
                person.DisplayName,
                $"Readiness override expires on {overrideItem.ExpiresAt:O}.",
                overrideItem.UpdatedAt));
        }

        var openIncidents = await db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && (x.Status == "open" || x.Status == "in_review"))
            .OrderByDescending(x => x.ReportedAt)
            .ToListAsync(cancellationToken);

        foreach (var incident in openIncidents)
        {
            if (!people.TryGetValue(incident.PersonId, out var person))
            {
                continue;
            }

            alerts.Add(new ReadinessReportAlertResponse(
                "open_incident",
                incident.Severity is "critical" or "high" ? "high" : "medium",
                person.Id,
                person.DisplayName,
                $"Open personnel incident: {incident.Title}.",
                incident.UpdatedAt));
        }

        return alerts
            .OrderByDescending(x => x.DetectedAt)
            .Take(200)
            .ToList();
    }

    private static bool MatchesScopeTypeFilter(string scopeType, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || string.Equals(filter, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(scopeType, filter.Trim(), StringComparison.OrdinalIgnoreCase);
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
