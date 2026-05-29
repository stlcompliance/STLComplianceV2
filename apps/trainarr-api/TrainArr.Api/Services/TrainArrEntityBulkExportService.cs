using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;

namespace TrainArr.Api.Services;

public sealed class TrainArrEntityBulkExportService(
    TrainArrDbContext db,
    ITrainArrAuditService auditService)
{
    public const string AssignmentsCsvHeader =
        "assignmentId,staffarrPersonId,definitionKey,definitionName,status,dueAt,createdAt,updatedAt,completedAt";

    public const string QualificationsCsvHeader =
        "qualificationIssueId,staffarrPersonId,qualificationKey,qualificationName,status,issuedAt,expiresAt";

    public const string TrainingDefinitionsCsvHeader =
        "definitionId,definitionKey,name,qualificationKey,qualificationName,status,createdAt,updatedAt";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "trainarr-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and operational analysis.");

    public EntityExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Entities:
            [
                new(
                    "training_assignments",
                    "/api/exports/training-assignments",
                    "Training assignments",
                    AssignmentsCsvHeader,
                    "Tenant training assignment registry with status and due timestamps.",
                    [CsvFormat]),
                new(
                    "qualification_issues",
                    "/api/exports/qualification-issues",
                    "Qualification issues",
                    QualificationsCsvHeader,
                    "Issued qualifications with lifecycle status and expiry.",
                    [CsvFormat]),
                new(
                    "training_definitions",
                    "/api/exports/training-definitions",
                    "Training definitions",
                    TrainingDefinitionsCsvHeader,
                    "Training definition catalog with qualification mapping.",
                    [CsvFormat]),
            ],
            ReportExports:
            [
                new(
                    "assignments",
                    "/api/reports/assignments/summary/export",
                    "Assignment report CSV",
                    "Scoped assignment rollups with overdue flags."),
                new(
                    "qualifications",
                    "/api/reports/qualifications/summary/export",
                    "Qualification report CSV",
                    "Scoped qualification lifecycle metrics."),
                new(
                    "compliance",
                    "/api/reports/compliance/summary/export",
                    "Compliance report CSV",
                    "Citation, rule-pack, and remediation attention rows."),
            ],
            AuditPackageFormats: ["json", "zip"]);

    public async Task<CsvExportResult> ExportTrainingAssignmentsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalized);
        }

        var assignments = await query.OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(AssignmentsCsvHeader);

        foreach (var assignment in assignments)
        {
            builder.Append(CsvEscape(assignment.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.TrainingDefinition.DefinitionKey));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.TrainingDefinition.Name));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.Status));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.DueAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.CreatedAt.ToString("O")));
            builder.Append(',');
            builder.Append(CsvEscape(assignment.UpdatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(assignment.CompletedAt?.ToString("O") ?? string.Empty));
        }

        await auditService.WriteAsync(
            "trainarr.exports.training_assignments",
            tenantId,
            actorUserId,
            "entity_export",
            "training_assignments",
            "success",
            cancellationToken: cancellationToken);

        return BuildCsv("training-assignments", builder);
    }

    public async Task<CsvExportResult> ExportQualificationIssuesCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.QualificationIssues.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalized);
        }

        var qualifications = await query.OrderBy(x => x.IssuedAt).ToListAsync(cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(QualificationsCsvHeader);

        foreach (var issue in qualifications)
        {
            builder.Append(CsvEscape(issue.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(issue.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(issue.QualificationKey));
            builder.Append(',');
            builder.Append(CsvEscape(issue.QualificationName));
            builder.Append(',');
            builder.Append(CsvEscape(issue.Status));
            builder.Append(',');
            builder.Append(CsvEscape(issue.IssuedAt.ToString("O")));
            builder.AppendLine(CsvEscape(issue.ExpiresAt?.ToString("O") ?? string.Empty));
        }

        await auditService.WriteAsync(
            "trainarr.exports.qualification_issues",
            tenantId,
            actorUserId,
            "entity_export",
            "qualification_issues",
            "success",
            cancellationToken: cancellationToken);

        return BuildCsv("qualification-issues", builder);
    }

    public async Task<CsvExportResult> ExportTrainingDefinitionsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var definitions = await db.TrainingDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DefinitionKey)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(TrainingDefinitionsCsvHeader);

        foreach (var definition in definitions)
        {
            builder.Append(CsvEscape(definition.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(definition.DefinitionKey));
            builder.Append(',');
            builder.Append(CsvEscape(definition.Name));
            builder.Append(',');
            builder.Append(CsvEscape(definition.QualificationKey));
            builder.Append(',');
            builder.Append(CsvEscape(definition.QualificationName));
            builder.Append(',');
            builder.Append(CsvEscape(definition.Status));
            builder.Append(',');
            builder.Append(CsvEscape(definition.CreatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(definition.UpdatedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "trainarr.exports.training_definitions",
            tenantId,
            actorUserId,
            "entity_export",
            "training_definitions",
            "success",
            cancellationToken: cancellationToken);

        return BuildCsv("training-definitions", builder);
    }

    private static CsvExportResult BuildCsv(string entityKey, StringBuilder builder)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        return new CsvExportResult(
            "text/csv",
            $"trainarr-{entityKey}-export-{timestamp}.csv",
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
