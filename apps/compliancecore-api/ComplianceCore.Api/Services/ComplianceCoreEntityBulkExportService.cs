using System.Text;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceCoreEntityBulkExportService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public const string FindingsCsvHeader =
        "findingId,findingKey,severity,status,title,packKey,ruleKey,factKey,createdAt";

    public const string EvaluationsCsvHeader =
        "evaluationRunId,rulePackId,packKey,overallResult,createdAt";

    public const string RulePacksCsvHeader =
        "rulePackId,packKey,label,status,version,createdAt,updatedAt";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "compliancecore-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and operational analysis.");

    public EntityExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Entities:
            [
                new(
                    "findings",
                    "/api/exports/findings",
                    "Compliance findings",
                    FindingsCsvHeader,
                    "Tenant findings registry with severity, status, and rule pack linkage.",
                    [CsvFormat]),
                new(
                    "evaluations",
                    "/api/exports/evaluations",
                    "Rule evaluation runs",
                    EvaluationsCsvHeader,
                    "Historical rule evaluation outcomes with pack references.",
                    [CsvFormat]),
                new(
                    "rule_packs",
                    "/api/exports/rule-packs",
                    "Rule packs",
                    RulePacksCsvHeader,
                    "Rule pack catalog with lifecycle status and version metadata.",
                    [CsvFormat]),
            ],
            ReportExports:
            [
                new(
                    "findings",
                    "/api/reports/findings/summary/export",
                    "Findings report CSV",
                    "Scoped findings rollups with status and severity filters."),
                new(
                    "operator",
                    "/api/reports/operator/summary/export",
                    "Operator report CSV",
                    "Evaluation and workflow gate rollups with attention filters."),
            ],
            AuditPackageFormats: ["json", "zip"]);

    public async Task<CsvExportResult> ExportFindingsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        bool? openOnly,
        CancellationToken cancellationToken = default)
    {
        var query = db.ComplianceFindings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (openOnly == true)
        {
            query = query.Where(x => x.Status == FindingStatuses.Open);
        }
        else if (!string.IsNullOrWhiteSpace(status)
            && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.Status == normalized);
        }

        var findings = await (
            from finding in query
            join pack in db.RulePacks.AsNoTracking() on finding.RulePackId equals pack.Id
            orderby finding.CreatedAt descending
            select new
            {
                finding.Id,
                finding.FindingKey,
                finding.Severity,
                finding.Status,
                finding.Title,
                finding.RuleKey,
                finding.FactKey,
                finding.CreatedAt,
                pack.PackKey,
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(FindingsCsvHeader);
        foreach (var finding in findings)
        {
            builder.Append(CsvEscape(finding.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(finding.FindingKey));
            builder.Append(',');
            builder.Append(CsvEscape(finding.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(finding.Status));
            builder.Append(',');
            builder.Append(CsvEscape(finding.Title));
            builder.Append(',');
            builder.Append(CsvEscape(finding.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(finding.RuleKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(finding.FactKey ?? string.Empty));
            builder.AppendLine(CsvEscape(finding.CreatedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "compliancecore.exports.findings",
            tenantId,
            actorUserId,
            "findings_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-findings-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<CsvExportResult> ExportEvaluationsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var evaluations = await (
            from run in db.RuleEvaluationRuns.AsNoTracking()
            join pack in db.RulePacks.AsNoTracking() on run.RulePackId equals pack.Id
            where run.TenantId == tenantId
            orderby run.CreatedAt descending
            select new
            {
                run.Id,
                run.RulePackId,
                pack.PackKey,
                run.OverallResult,
                run.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(EvaluationsCsvHeader);
        foreach (var evaluation in evaluations)
        {
            builder.Append(CsvEscape(evaluation.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(evaluation.RulePackId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(evaluation.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(evaluation.OverallResult));
            builder.AppendLine(CsvEscape(evaluation.CreatedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "compliancecore.exports.evaluations",
            tenantId,
            actorUserId,
            "evaluations_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-evaluations-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<CsvExportResult> ExportRulePacksCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.RulePacks.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status)
            && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.Status == normalized);
        }

        var rulePacks = await query
            .OrderBy(x => x.PackKey)
            .Select(x => new
            {
                x.Id,
                x.PackKey,
                x.Label,
                x.Status,
                x.VersionNumber,
                x.CreatedAt,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(RulePacksCsvHeader);
        foreach (var pack in rulePacks)
        {
            builder.Append(CsvEscape(pack.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(pack.PackKey));
            builder.Append(',');
            builder.Append(CsvEscape(pack.Label));
            builder.Append(',');
            builder.Append(CsvEscape(pack.Status));
            builder.Append(',');
            builder.Append(CsvEscape(pack.VersionNumber.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(pack.CreatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(pack.UpdatedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "compliancecore.exports.rule_packs",
            tenantId,
            actorUserId,
            "rule_packs_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"compliancecore-rule-packs-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
