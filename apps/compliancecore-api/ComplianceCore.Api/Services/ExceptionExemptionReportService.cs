using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class ExceptionExemptionReportService(ComplianceCoreDbContext db)
{
    private const int RecentLimit = 100;
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(14);

    public async Task<ExceptionExemptionReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? type,
        string? effectType,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceExceptionExemptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        var normalizedType = NormalizeFilterValue(type);
        var normalizedEffectType = NormalizeFilterValue(effectType);

        if (normalizedType is not null)
        {
            query = query.Where(x => x.Type == normalizedType);
        }

        if (normalizedEffectType is not null)
        {
            query = query.Where(x => x.EffectType == normalizedEffectType);
        }

        if (activeOnly == true)
        {
            query = query.Where(x => x.Active);
        }

        var exemptions = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var recent = exemptions
            .Take(RecentLimit)
            .Select(x => new ExceptionExemptionReportSummaryItem(
                x.ExceptionExemptionId,
                x.Key,
                x.Label,
                x.Type,
                x.EffectType,
                x.PackKey,
                string.IsNullOrWhiteSpace(x.CitationKey) ? null : x.CitationKey,
                x.Active ? "active" : "inactive",
                x.EffectiveAt,
                x.ExpiresAt,
                x.UpdatedAt))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        return new ExceptionExemptionReportSummaryResponse(
            exemptions.Count,
            exemptions.Count(x => x.Active),
            exemptions.Count(x => !x.Active),
            exemptions.Count(x => string.Equals(x.Type, ComplianceExceptionExemptionTypes.Waiver, StringComparison.OrdinalIgnoreCase)),
            exemptions.Count(x => string.Equals(x.Type, ComplianceExceptionExemptionTypes.Variance, StringComparison.OrdinalIgnoreCase)),
            exemptions.Count(x => string.Equals(x.Type, ComplianceExceptionExemptionTypes.SpecialPermit, StringComparison.OrdinalIgnoreCase)),
            exemptions.Count(x =>
                x.Active
                && x.ExpiresAt.HasValue
                && x.ExpiresAt.Value > now
                && x.ExpiresAt.Value <= now.Add(ExpiringSoonWindow)),
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? type,
        string? effectType,
        bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, type, effectType, activeOnly, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine("exceptionExemptionId,key,label,type,effectType,packKey,citationKey,activeState,effectiveAt,expiresAt,updatedAt");

        foreach (var item in summary.RecentExceptionExemptions)
        {
            builder.Append(CsvEscape(item.ExceptionExemptionId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.Key));
            builder.Append(',');
            builder.Append(CsvEscape(item.Label));
            builder.Append(',');
            builder.Append(CsvEscape(item.Type));
            builder.Append(',');
            builder.Append(CsvEscape(item.EffectType));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.CitationKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.ActiveState));
            builder.Append(',');
            builder.Append(CsvEscape(item.EffectiveAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(item.ExpiresAt?.ToString("O") ?? string.Empty));
            builder.AppendLine(CsvEscape(item.UpdatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-exception-exemption-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
