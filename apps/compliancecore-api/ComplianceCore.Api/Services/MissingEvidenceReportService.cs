using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class MissingEvidenceReportService(ComplianceCoreDbContext db)
{
    private const int RecentLimit = 100;

    public async Task<MissingEvidenceReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? severity,
        string? reasonCode,
        CancellationToken cancellationToken = default)
    {
        var query = db.MissingEvidenceWarnings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(severity)
            && !string.Equals(severity, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = severity.Trim();
            query = query.Where(x => x.Severity == normalized);
        }

        if (!string.IsNullOrWhiteSpace(reasonCode)
            && !string.Equals(reasonCode, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = reasonCode.Trim();
            query = query.Where(x => x.ReasonCode == normalized);
        }

        var warnings = await query
            .OrderByDescending(x => x.EvaluatedAt)
            .ToListAsync(cancellationToken);

        var recent = warnings
            .Take(RecentLimit)
            .Select(x => new MissingEvidenceReportSummaryItem(
                x.Id,
                x.RunId,
                x.RulePackId,
                x.PackKey,
                x.FactKey,
                x.WarningType,
                x.Severity,
                x.ReasonCode,
                x.HasMirrorAtScope,
                x.IsRequiredInRule,
                x.IsRequiredInCatalog,
                x.Summary,
                x.EvaluatedAt))
            .ToList();

        return new MissingEvidenceReportSummaryResponse(
            warnings.Count,
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Critical),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.High),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Medium),
            warnings.Count(x => x.Severity == MissingEvidenceWarningSeverities.Low),
            warnings.Count(x => x.ReasonCode == MissingEvidenceReasonCodes.MissingMirror),
            warnings.Count(x => x.ReasonCode == MissingEvidenceReasonCodes.UnresolvedFact),
            warnings.Count(x => x.ReasonCode == MissingEvidenceReasonCodes.NoFactDefinition),
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? severity,
        string? reasonCode,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, severity, reasonCode, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "warningId,runId,rulePackId,packKey,factKey,warningType,severity,reasonCode,hasMirrorAtScope,isRequiredInRule,isRequiredInCatalog,summary,evaluatedAt");

        foreach (var item in summary.RecentWarnings)
        {
            builder.Append(CsvEscape(item.WarningId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RunId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.FactKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.WarningType));
            builder.Append(',');
            builder.Append(CsvEscape(item.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(item.ReasonCode));
            builder.Append(',');
            builder.Append(CsvEscape(item.HasMirrorAtScope ? "true" : "false"));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsRequiredInRule ? "true" : "false"));
            builder.Append(',');
            builder.Append(CsvEscape(item.IsRequiredInCatalog ? "true" : "false"));
            builder.Append(',');
            builder.Append(CsvEscape(item.Summary));
            builder.AppendLine(CsvEscape(item.EvaluatedAt.ToString("O")));
        }

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-missing-evidence-report-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
