using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public sealed class StaffArrEntityBulkExportService(
    StaffArrDbContext db,
    IStaffArrAuditService auditService)
{
    public const string PeopleCsvHeader =
        "personId,displayName,primaryEmail,employmentStatus,primaryOrgUnitName,jobTitle,createdAt,updatedAt";

    public const string IncidentsCsvHeader =
        "incidentId,personId,reasonCategoryKey,severity,status,title,occurredAt,reportedAt";

    public const string CertificationsCsvHeader =
        "personCertificationId,personId,certificationDefinitionKey,certificationName,status,issuedAt,expiresAt";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "staffarr-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and operational analysis.");

    public EntityExportManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Entities:
            [
                new(
                    "people",
                    "/api/exports/people",
                    "People directory",
                    PeopleCsvHeader,
                    "Tenant workforce directory with employment status and org linkage.",
                    [CsvFormat]),
                new(
                    "personnel_incidents",
                    "/api/exports/personnel-incidents",
                    "Personnel incidents",
                    IncidentsCsvHeader,
                    "Incident registry with severity, status, and occurrence timestamps.",
                    [CsvFormat]),
                new(
                    "person_certifications",
                    "/api/exports/person-certifications",
                    "Person certifications",
                    CertificationsCsvHeader,
                    "Granted certifications with lifecycle status and expiry.",
                    [CsvFormat]),
            ],
            ReportExports:
            [
                new(
                    "personnel",
                    "/api/reports/personnel/summary/export",
                    "Personnel report CSV",
                    "Scoped workforce rollups by employment status."),
                new(
                    "readiness",
                    "/api/reports/readiness/summary/export",
                    "Readiness report CSV",
                    "Org-unit readiness rollups with attention filters."),
                new(
                    "incidents",
                    "/api/reports/incidents/summary/export",
                    "Incident report CSV",
                    "Scoped incident metrics with open and severity filters."),
            ],
            AuditPackageFormats: ["json", "zip"]);

    public async Task<CsvExportResult> ExportPeopleCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? employmentStatus,
        CancellationToken cancellationToken = default)
    {
        var query = db.People
            .AsNoTracking()
            .Include(x => x.PrimaryOrgUnit)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(employmentStatus)
            && !string.Equals(employmentStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = employmentStatus.Trim();
            query = query.Where(x => x.EmploymentStatus == normalized);
        }

        var people = await query
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(PeopleCsvHeader);
        foreach (var person in people)
        {
            builder.Append(CsvEscape(person.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(person.DisplayName));
            builder.Append(',');
            builder.Append(CsvEscape(person.PrimaryEmail));
            builder.Append(',');
            builder.Append(CsvEscape(person.EmploymentStatus));
            builder.Append(',');
            builder.Append(CsvEscape(person.PrimaryOrgUnit?.Name ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(person.JobTitle ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(person.CreatedAt.ToString("O")));
            builder.AppendLine(CsvEscape(person.UpdatedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "staffarr.exports.people",
            tenantId,
            actorUserId,
            "people_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"staffarr-people-export-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<CsvExportResult> ExportPersonnelIncidentsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status)
            && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim();
            query = query.Where(x => x.Status == normalized);
        }

        var incidents = await query
            .OrderByDescending(x => x.ReportedAt)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(IncidentsCsvHeader);
        foreach (var incident in incidents)
        {
            builder.Append(CsvEscape(incident.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(incident.PersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(incident.ReasonCategoryKey));
            builder.Append(',');
            builder.Append(CsvEscape(incident.Severity));
            builder.Append(',');
            builder.Append(CsvEscape(incident.Status));
            builder.Append(',');
            builder.Append(CsvEscape(incident.Title));
            builder.Append(',');
            builder.Append(CsvEscape(incident.OccurredAt.ToString("O")));
            builder.AppendLine(CsvEscape(incident.ReportedAt.ToString("O")));
        }

        await auditService.WriteAsync(
            "staffarr.exports.personnel_incidents",
            tenantId,
            actorUserId,
            "personnel_incidents_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"staffarr-personnel-incidents-export-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<CsvExportResult> ExportPersonCertificationsCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var certificationQuery = db.PersonCertifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status)
            && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = status.Trim();
            certificationQuery = certificationQuery.Where(x => x.Status == normalized);
        }

        var certifications = await certificationQuery
            .OrderByDescending(x => x.GrantedAt)
            .ToListAsync(cancellationToken);

        var definitionIds = certifications.Select(x => x.CertificationDefinitionId).Distinct().ToList();
        var definitions = await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && definitionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(CertificationsCsvHeader);
        foreach (var certification in certifications)
        {
            definitions.TryGetValue(certification.CertificationDefinitionId, out var definition);
            builder.Append(CsvEscape(certification.Id.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(certification.PersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(definition?.CertificationKey ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(definition?.Name ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(certification.Status));
            builder.Append(',');
            builder.Append(CsvEscape(certification.GrantedAt.ToString("O")));
            builder.AppendLine(CsvEscape(certification.ExpiresAt?.ToString("O") ?? string.Empty));
        }

        await auditService.WriteAsync(
            "staffarr.exports.person_certifications",
            tenantId,
            actorUserId,
            "person_certifications_export",
            null,
            "success",
            cancellationToken: cancellationToken);

        return new CsvExportResult(
            "text/csv",
            $"staffarr-person-certifications-export-{DateTime.UtcNow:yyyy-MM-dd}.csv",
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
