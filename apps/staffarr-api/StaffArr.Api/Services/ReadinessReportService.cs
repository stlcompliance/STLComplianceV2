using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class ReadinessReportService(StaffArrDbContext db)
{
    private const int RecentLimit = 25;

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
