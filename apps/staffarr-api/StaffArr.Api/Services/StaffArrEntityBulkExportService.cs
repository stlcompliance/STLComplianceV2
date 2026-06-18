using System.Text;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class StaffArrEntityBulkExportService(
    StaffArrDbContext db,
    IStaffArrAuditService auditService,
    StaffArrTenantSettingsService tenantSettingsService)
{
    public const string PeopleCsvHeader =
        "personId,displayName,primaryEmail,employmentStatus,primaryOrgUnitName,jobTitle,createdAt,updatedAt";

    public const string IncidentsCsvHeader =
        "incidentId,personId,reasonCategoryKey,severity,status,title,occurredAt,reportedAt,sourceProduct,sourceIncidentId,sourceEventKind,sourceReferenceKey";

    public const string CertificationsCsvHeader =
        "personCertificationId,personId,certificationDefinitionKey,certificationName,status,issuedAt,expiresAt";

    private static readonly EntityExportFormatDescriptor CsvFormat = new(
        "csv",
        "text/csv",
        "staffarr-{entity}-export-{timestamp}.csv",
        "Comma-separated values for spreadsheets and operational analysis.");

    public EntityExportManifestResponse GetManifest(string exportBasePath = "/api/exports") =>
        new(
            PackageVersion: "2",
            Entities:
            [
                new(
                    "people",
                    $"{exportBasePath}/people",
                    "People directory",
                    PeopleCsvHeader,
                    "Tenant workforce directory with employment status and org linkage.",
                    [CsvFormat]),
                new(
                    "personnel_incidents",
                    $"{exportBasePath}/personnel-incidents",
                    "Personnel incidents",
                    IncidentsCsvHeader,
                    "Incident registry with severity, status, and occurrence timestamps.",
                    [CsvFormat]),
                new(
                    "person_certifications",
                    $"{exportBasePath}/person-certifications",
                    "Person certifications",
                    CertificationsCsvHeader,
                    "Granted certifications with lifecycle status and expiry.",
                    [CsvFormat]),
            ]);

    public async Task<CsvExportResult> ExportPeopleCsvAsync(
        Guid tenantId,
        Guid actorUserId,
        string? employmentStatus,
        CancellationToken cancellationToken = default)
    {
        await EnsureExportEnabledAsync(tenantId, cancellationToken);

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
        await EnsureExportEnabledAsync(tenantId, cancellationToken);

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
            builder.Append(',');
            builder.Append(CsvEscape(incident.ReportedAt.ToString("O")));
            builder.Append(',');
            builder.Append(CsvEscape(incident.SourceProduct ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(incident.SourceIncidentId?.ToString() ?? string.Empty));
            builder.Append(',');
            builder.Append(CsvEscape(incident.SourceEventKind ?? string.Empty));
            builder.Append(',');
            builder.AppendLine(CsvEscape(incident.SourceReferenceKey ?? string.Empty));
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
        await EnsureExportEnabledAsync(tenantId, cancellationToken);

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

    private async Task EnsureExportEnabledAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var settings = await tenantSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!settings.ExportEnabled)
        {
            throw new StlApiException(
                "staffarr_exports.disabled",
                "StaffArr exports are disabled for this tenant.",
                409);
        }
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
