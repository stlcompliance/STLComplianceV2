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

    public async Task<AuditPackageFilterOptionsResponse> GetFilterOptionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);

        var actions = await query.Select(x => x.Action).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        var results = await query.Select(x => x.Result).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        var targetTypes = await query.Select(x => x.TargetType).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        var actorUserIds = await query
            .Where(x => x.ActorUserId != null)
            .Select(x => x.ActorUserId!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new AuditPackageFilterOptionsResponse(actions, results, targetTypes, actorUserIds);
    }

    public async Task<AuditPackageExportSummaryResponse> GetExportSummaryAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);

        var byResult = package.AuditEvents
            .GroupBy(x => x.Result, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var byAction = package.AuditEvents
            .GroupBy(x => x.Action, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AuditPackageBreakdownItem(group.Key, group.Count()))
            .OrderByDescending(x => x.Count)
            .Take(15)
            .ToList();

        return new AuditPackageExportSummaryResponse(
            MapAppliedFilters(filter),
            package.Counts,
            byResult,
            byAction,
            DateTimeOffset.UtcNow);
    }

    public async Task<PagedResult<StaffArrAuditEventExportItem>> ListAuditTimelineAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => pageSize,
        };

        var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        query = ApplyAuditEventFilters(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResult<StaffArrAuditEventExportItem>(
            items,
            page,
            pageSize,
            totalCount,
            page * pageSize < totalCount);
    }

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "2",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped StaffArr audit trail (JSON)."),
                new("audit_events_csv", "audit_events.csv", "Audit events (CSV)", "Same audit events in CSV for spreadsheets."),
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
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(tenantId, filter, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await MaterializeExportAsync(tenantId, filter, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<byte[]> ExportAuditEventsCsvAsync(
        Guid tenantId,
        Guid? actorUserId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);
        var csvBytes = AuditPackageCsvWriter.WriteAuditEvents(package.AuditEvents);

        await auditService.WriteAsync(
            "audit_package.export_csv",
            tenantId,
            actorUserId,
            "audit_package",
            Guid.NewGuid().ToString(),
            "success",
            reasonCode: BuildFilterReasonCode(filter),
            cancellationToken: cancellationToken);

        return csvBytes;
    }

    public async Task<AuditPackageExportResponse> MaterializeExportAsync(
        Guid tenantId,
        AuditPackageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(filter.From, filter.To);
        var package = await LoadPackageDataAsync(tenantId, filter, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

    public static AuditPackageFilter FromJob(AuditPackageGenerationJob job)
    {
        if (string.IsNullOrWhiteSpace(job.FilterJson))
        {
            return new AuditPackageFilter(job.FromUtc, job.ToUtc);
        }

        return JsonSerializer.Deserialize<AuditPackageFilter>(job.FilterJson, JsonOptions)
            ?? new AuditPackageFilter(job.FromUtc, job.ToUtc);
    }

    public static string SerializeFilter(AuditPackageFilter filter) =>
        JsonSerializer.Serialize(filter, JsonOptions);

    public async Task<byte[]> CreateZipBytesAsync(
        AuditPackageExportResponse package,
        CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteJsonEntryAsync(archive, "manifest.json", new
            {
                package.PackageId,
                package.TenantId,
                package.GeneratedAt,
                package.DateRange,
                package.AppliedFilters,
                package.Counts,
                PackageVersion = "2",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
            await WriteCsvEntryAsync(archive, "audit_events.csv", package.AuditEvents, cancellationToken);
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
        AuditPackageFilter filter,
        CancellationToken cancellationToken)
    {
        var from = filter.From;
        var to = filter.To;

        var auditEventsQuery = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyAuditEventFilters(auditEventsQuery, filter);

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
                x.UpdatedAt,
                x.SourceProduct,
                x.SourceIncidentId,
                x.SourceEventKind,
                x.SourceReferenceKey))
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
            AppliedFilters: MapAppliedFilters(filter),
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

    private static IQueryable<StaffArrAuditEvent> ApplyAuditEventFilters(
        IQueryable<StaffArrAuditEvent> query,
        AuditPackageFilter filter)
    {
        query = ApplyOccurredAtFilter(query, filter.From, filter.To);

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            var action = filter.Action.Trim().ToLowerInvariant();
            query = query.Where(x => x.Action.ToLower() == action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            var result = filter.Result.Trim().ToLowerInvariant();
            query = query.Where(x => x.Result.ToLower() == result);
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetType))
        {
            var targetType = filter.TargetType.Trim().ToLowerInvariant();
            query = query.Where(x => x.TargetType.ToLower() == targetType);
        }

        if (filter.ActorUserId is Guid actorUserId)
        {
            query = query.Where(x => x.ActorUserId == actorUserId);
        }

        return query;
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

    private static AuditPackageAppliedFiltersResponse MapAppliedFilters(AuditPackageFilter filter) =>
        new(filter.From, filter.To, filter.Action, filter.Result, filter.TargetType, filter.ActorUserId);

    private static string BuildFilterReasonCode(AuditPackageFilter filter)
    {
        var parts = new List<string>();
        if (filter.From is not null)
        {
            parts.Add($"from={filter.From:O}");
        }

        if (filter.To is not null)
        {
            parts.Add($"to={filter.To:O}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            parts.Add($"action={filter.Action}");
        }

        return parts.Count == 0 ? "all_time" : string.Join(";", parts);
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

    private static async Task WriteCsvEntryAsync(
        ZipArchive archive,
        string entryName,
        IReadOnlyList<StaffArrAuditEventExportItem> events,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        var bytes = AuditPackageCsvWriter.WriteAuditEvents(events);
        await entryStream.WriteAsync(bytes, cancellationToken);
    }

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
