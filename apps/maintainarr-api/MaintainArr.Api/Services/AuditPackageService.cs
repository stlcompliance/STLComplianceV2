using System.IO.Compression;
using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AuditPackageService(
    MaintainArrDbContext db,
    IMaintainArrAuditService auditService)
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

        return new AuditPackageFilterOptionsResponse(actions, results, targetTypes);
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

    public async Task<PagedResult<AuditEventExportItem>> ListAuditTimelineAsync(
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
            .Select(x => new AuditEventExportItem(
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

        return new PagedResult<AuditEventExportItem>(
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
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped MaintainArr audit trail (JSON)."),
                new("audit_events_csv", "audit_events.csv", "Audit events (CSV)", "Same audit events in CSV for spreadsheets."),
                new("assets", "assets.json", "Assets", "Asset registry snapshot for the tenant."),
                new("work_orders", "work_orders.json", "Work orders", "Maintenance work order records."),
                new("defects", "defects.json", "Defects", "Asset defect capture and resolution records."),
                new("inspection_runs", "inspection_runs.json", "Inspection runs", "Completed and in-progress inspection runs."),
                new("pm_schedules", "pm_schedules.json", "PM schedules", "Preventive maintenance schedule due state."),
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
            await WriteJsonEntryAsync(archive, "assets.json", package.Assets, cancellationToken);
            await WriteJsonEntryAsync(archive, "work_orders.json", package.WorkOrders, cancellationToken);
            await WriteJsonEntryAsync(archive, "defects.json", package.Defects, cancellationToken);
            await WriteJsonEntryAsync(archive, "inspection_runs.json", package.InspectionRuns, cancellationToken);
            await WriteJsonEntryAsync(archive, "pm_schedules.json", package.PmSchedules, cancellationToken);
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

        var assetsQuery = db.Assets.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            assetsQuery = assetsQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            assetsQuery = assetsQuery.Where(x => x.CreatedAt <= to);
        }

        var workOrdersQuery = db.WorkOrders.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            workOrdersQuery = workOrdersQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            workOrdersQuery = workOrdersQuery.Where(x => x.CreatedAt <= to);
        }

        var defectsQuery = db.Defects.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            defectsQuery = defectsQuery.Where(x => x.CreatedAt >= from);
        }

        if (to is not null)
        {
            defectsQuery = defectsQuery.Where(x => x.CreatedAt <= to);
        }

        var inspectionRunsQuery = db.InspectionRuns.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            inspectionRunsQuery = inspectionRunsQuery.Where(x => x.StartedAt >= from);
        }

        if (to is not null)
        {
            inspectionRunsQuery = inspectionRunsQuery.Where(x => x.StartedAt <= to);
        }

        var pmSchedulesQuery = db.PmSchedules.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (from is not null)
        {
            pmSchedulesQuery = pmSchedulesQuery.Where(x => x.NextDueAt >= from);
        }

        if (to is not null)
        {
            pmSchedulesQuery = pmSchedulesQuery.Where(x => x.NextDueAt <= to);
        }

        var auditEvents = await auditEventsQuery
            .OrderBy(x => x.OccurredAt)
            .Select(x => new AuditEventExportItem(
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

        var assets = await assetsQuery
            .OrderBy(x => x.AssetTag)
            .Join(db.AssetTypes.AsNoTracking(), a => a.AssetTypeId, t => t.Id, (asset, type) => new { asset, type })
            .Join(db.AssetClasses.AsNoTracking(), x => x.type.AssetClassId, c => c.Id, (x, assetClass) =>
                new AuditPackageAssetItem(
                    x.asset.Id,
                    x.asset.AssetTag,
                    x.asset.Name,
                    x.asset.LifecycleStatus,
                    x.asset.SiteRef,
                    x.type.TypeKey,
                    assetClass.ClassKey,
                    x.asset.CreatedAt,
                    x.asset.UpdatedAt))
            .ToListAsync(cancellationToken);

        var workOrders = await workOrdersQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackageWorkOrderItem(
                x.Id,
                x.WorkOrderNumber,
                x.AssetId,
                x.Title,
                x.Status,
                x.Priority,
                x.Source,
                x.CreatedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);

        var defects = await defectsQuery
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AuditPackageDefectItem(
                x.Id,
                x.AssetId,
                x.Title,
                x.Severity,
                x.Status,
                x.CreatedAt,
                x.ResolvedAt))
            .ToListAsync(cancellationToken);

        var inspectionRuns = await inspectionRunsQuery
            .OrderBy(x => x.StartedAt)
            .Join(
                db.InspectionTemplates.AsNoTracking(),
                run => run.InspectionTemplateId,
                template => template.Id,
                (run, template) => new AuditPackageInspectionRunItem(
                    run.Id,
                    run.AssetId,
                    template.TemplateKey,
                    run.Status,
                    run.Result,
                    run.StartedAt,
                    run.CompletedAt))
            .ToListAsync(cancellationToken);

        var pmSchedules = await pmSchedulesQuery
            .OrderBy(x => x.NextDueAt)
            .Select(x => new AuditPackagePmScheduleItem(
                x.Id,
                x.AssetId,
                x.ScheduleKey,
                x.Name,
                x.DueStatus,
                x.NextDueAt,
                x.LastCompletedAt))
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
                assets.Count,
                workOrders.Count,
                defects.Count,
                inspectionRuns.Count,
                pmSchedules.Count),
            AuditEvents: auditEvents,
            Assets: assets,
            WorkOrders: workOrders,
            Defects: defects,
            InspectionRuns: inspectionRuns,
            PmSchedules: pmSchedules);
    }

    private static IQueryable<MaintainArrAuditEvent> ApplyAuditEventFilters(
        IQueryable<MaintainArrAuditEvent> query,
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

    private static IQueryable<MaintainArrAuditEvent> ApplyOccurredAtFilter(
        IQueryable<MaintainArrAuditEvent> query,
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

        if (!string.IsNullOrWhiteSpace(filter.Result))
        {
            parts.Add($"result={filter.Result}");
        }

        if (!string.IsNullOrWhiteSpace(filter.TargetType))
        {
            parts.Add($"targetType={filter.TargetType}");
        }

        if (filter.ActorUserId is Guid actorUserId)
        {
            parts.Add($"actorUserId={actorUserId}");
        }

        return parts.Count == 0 ? "all_time" : string.Join(";", parts);
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

    private static async Task WriteCsvEntryAsync(
        ZipArchive archive,
        string entryName,
        IReadOnlyList<AuditEventExportItem> events,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        var bytes = AuditPackageCsvWriter.WriteAuditEvents(events);
        await entryStream.WriteAsync(bytes, cancellationToken);
    }
}
