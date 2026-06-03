using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class WaiverReportService(ComplianceCoreDbContext db)
{
    private const int RecentLimit = 100;
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(14);

    public async Task<WaiverReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        string? packKey,
        string? scopeKey,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceWaivers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        var normalizedStatus = NormalizeFilterValue(status);
        var normalizedPackKey = NormalizeFilterValue(packKey);
        var normalizedScopeKey = NormalizeFilterValue(scopeKey);

        if (normalizedStatus is not null)
        {
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (normalizedPackKey is not null)
        {
            query = query.Where(x => x.PackKey == normalizedPackKey);
        }

        if (normalizedScopeKey is not null)
        {
            query = query.Where(x => x.SubjectScopeKey == normalizedScopeKey);
        }

        var waivers = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var recent = waivers
            .Take(RecentLimit)
            .Select(x => new WaiverReportSummaryItem(
                x.Id,
                x.WaiverKey,
                x.PackKey,
                x.SubjectScopeKey,
                x.Status,
                x.ReasonCode,
                x.EffectiveAt,
                x.ExpiresAt,
                x.UpdatedAt))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        return new WaiverReportSummaryResponse(
            waivers.Count,
            waivers.Count(x => x.Status == WaiverStatuses.Pending),
            waivers.Count(x => x.Status == WaiverStatuses.Approved),
            waivers.Count(x => x.Status == WaiverStatuses.Rejected),
            waivers.Count(x => x.Status == WaiverStatuses.Revoked),
            waivers.Count(x => x.Status == WaiverStatuses.Expired),
            waivers.Count(x =>
                x.Status == WaiverStatuses.Approved
                && x.ExpiresAt.HasValue
                && x.ExpiresAt.Value > now
                && x.ExpiresAt.Value <= now.Add(ExpiringSoonWindow)),
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? status,
        string? packKey,
        string? scopeKey,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, status, packKey, scopeKey, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("waiverId,waiverKey,packKey,subjectScopeKey,status,reasonCode,effectiveAt,expiresAt,updatedAt");

        foreach (var item in summary.RecentWaivers)
        {
            builder.Append(CsvEscape(item.WaiverId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.WaiverKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.SubjectScopeKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReasonCode));
            builder.Append(',');
            builder.Append(CsvEscape(item.EffectiveAt.ToString("O")));
            builder.Append(',');
            builder.Append(CsvEscape(item.ExpiresAt?.ToString("O") ?? string.Empty));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-waiver-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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

    private static string? NormalizeFilterValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }
}
