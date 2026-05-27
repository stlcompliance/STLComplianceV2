using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class AuditPackageService(
    StaffArrDbContext db,
    IStaffArrAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped StaffArr audit trail."),
                new("people", "people.json", "People", "Workforce directory snapshot for the tenant."),
                new("permission_history", "permission_history.json", "Permission history", "Role and permission assignment history events."),
                new("person_certifications", "person_certifications.json", "Certifications", "Person certification grants and lifecycle records."),
                new("personnel_incidents", "personnel_incidents.json", "Incidents", "Personnel incident intake records."),
                new("readiness_overrides", "readiness_overrides.json", "Readiness overrides", "Manual readiness override grants and clears."),
                new("training_blockers", "training_blockers.json", "Training blockers", "TrainArr training blocker mirror records."),
            ]);

    public async Task<AuditPackageExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);

        var package = await LoadPackageDataAsync(tenantId, from, to, cancellationToken);
        var packageId = Guid.NewGuid();

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            packageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return package with { PackageId = packageId };
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var package = await BuildExportAsync(tenantId, actorUserId, from, to, cancellationToken);

        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteJsonEntryAsync(archive, "manifest.json", new
            {
                package.PackageId,
                package.TenantId,
                package.GeneratedAt,
                package.DateRange,
                package.Counts,
                PackageVersion = "1",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
            await WriteJsonEntryAsync(archive, "people.json", package.People, cancellationToken);
            await WriteJsonEntryAsync(archive, "permission_history.json", package.PermissionHistory, cancellationToken);
            await WriteJsonEntryAsync(archive, "person_certifications.json", package.PersonCertifications, cancellationToken);
            await WriteJsonEntryAsync(archive, "personnel_incidents.json", package.PersonnelIncidents, cancellationToken);
            await WriteJsonEntryAsync(archive, "readiness_overrides.json", package.ReadinessOverrides, cancellationToken);
            await WriteJsonEntryAsync(archive, "training_blockers.json", package.TrainingBlockers, cancellationToken);
        }

        return memory.ToArray();
    }

    private async Task<AuditPackageExportResponse> LoadPackageDataAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyOccurredAtFilter(auditEventsQuery, from, to);

        var peopleQuery = db.People.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            peopleQuery = peopleQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            peopleQuery = peopleQuery.Where(x => x.UpdatedAt <= to);
        }

        var permissionHistoryQuery = db.PermissionHistoryEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        permissionHistoryQuery = ApplyPermissionHistoryFilter(permissionHistoryQuery, from, to);

        var certificationsQuery = db.PersonCertifications.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            certificationsQuery = certificationsQuery.Where(x => x.UpdatedAt >= from);
        }

        if (to is not null)
        {
            certificationsQuery = certificationsQuery.Where(x => x.UpdatedAt <= to);
        }

        var incidentsQuery = db.PersonnelIncidents.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            incidentsQuery = incidentsQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            incidentsQuery = incidentsQuery.Where(x => x.CreatedAt <= to);
        }

        var overridesQuery = db.PersonReadinessOverrides.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            overridesQuery = overridesQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            overridesQuery = overridesQuery.Where(x => x.CreatedAt <= to);
        }

        var blockersQuery = db.PersonTrainingBlockers.AsNoTracking().Where(x => x.TenantId == tenantId);
        blockersQuery = ApplyPublishedAtFilter(blockersQuery, from, to);

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new StaffArrAuditEventExportItem(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var people = await peopleQuery
            .OrderBy(x => x.DisplayName)
            .Select(x => new AuditPackagePersonItem(
                x.Id,
                x.ExternalUserId,
                x.DisplayName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.PrimaryOrgUnitId,
                x.ManagerPersonId,
                x.JobTitle,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var permissionHistory = await permissionHistoryQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new AuditPackagePermissionHistoryItem(
                x.Id,
                x.PersonId,
                x.AssignmentId,
                x.RoleTemplateId,
                x.PermissionTemplateId,
                x.ActorUserId,
                x.EventType,
                x.AssignmentStatus,
                x.RoleKey,
                x.RoleName,
                x.PermissionKey,
                x.PermissionName,
                x.ScopeType,
                x.ScopeValue,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var certifications = await certificationsQuery
            .OrderBy(x => x.GrantedAt)
            .Select(x => new AuditPackagePersonCertificationItem(
                x.Id,
                x.PersonId,
                x.CertificationDefinitionId,
                x.SourceType,
                x.Status,
                x.GrantedAt,
                x.ExpiresAt,
                x.Notes,
                x.GrantedByUserId,
                x.ExternalPublicationId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var incidents = await incidentsQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackagePersonnelIncidentItem(
                x.Id,
                x.PersonId,
                x.ReasonCategoryKey,
                x.Severity,
                x.Status,
                x.Title,
                x.Description,
                x.OccurredAt,
                x.ReportedAt,
                x.ReportedByUserId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var overrides = await overridesQuery
            .OrderBy(x => x.GrantedAt)
            .Select(x => new AuditPackageReadinessOverrideItem(
                x.Id,
                x.PersonId,
                x.Status,
                x.Reason,
                x.GrantedAt,
                x.ExpiresAt,
                x.GrantedByUserId,
                x.ClearedAt,
                x.ClearedByUserId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var blockers = await blockersQuery
            .OrderBy(x => x.PublishedAt)
            .Select(x => new AuditPackageTrainingBlockerItem(
                x.Id,
                x.PersonId,
                x.TrainarrPublicationId,
                x.QualificationKey,
                x.QualificationName,
                x.BlockerType,
                x.Message,
                x.Status,
                x.PublishedAt,
                x.ExpiresAt,
                x.ClearedAt,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new AuditPackageExportResponse(
            PackageId: Guid.Empty,
            TenantId: tenantId,
            GeneratedAt: DateTimeOffset.UtcNow,
            DateRange: from is null && to is null
                ? null
                : new AuditPackageDateRangeResponse(from, to),
            Counts: new AuditPackageCountsResponse(
                auditEvents.Count,
                people.Count,
                permissionHistory.Count,
                certifications.Count,
                incidents.Count,
                overrides.Count,
                blockers.Count),
            AuditEvents: auditEvents,
            People: people,
            PermissionHistory: permissionHistory,
            PersonCertifications: certifications,
            PersonnelIncidents: incidents,
            ReadinessOverrides: overrides,
            TrainingBlockers: blockers);
    }

    private static IQueryable<StaffArrAuditEvent> ApplyOccurredAtFilter(
        IQueryable<StaffArrAuditEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static IQueryable<PermissionHistoryEvent> ApplyPermissionHistoryFilter(
        IQueryable<PermissionHistoryEvent> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.OccurredAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.OccurredAt <= to);
        }

        return query;
    }

    private static IQueryable<PersonTrainingBlocker> ApplyPublishedAtFilter(
        IQueryable<PersonTrainingBlocker> query,
        DateTimeOffset? from,
        DateTimeOffset? to)
    {
        if (from is not null)
        {
            query = query.Where(x => x.PublishedAt >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.PublishedAt <= to);
        }

        return query;
    }

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }

    private static string? BuildDateRangeReasonCode(DateTimeOffset? from, DateTimeOffset? to) =>
        from is null && to is null ? "all_time" : "date_filtered";

    private static async Task WriteJsonEntryAsync<T>(
        ZipArchive archive,
        string entryName,
        T payload,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await JsonSerializer.SerializeAsync(entryStream, payload, JsonOptions, cancellationToken);
    }
}
