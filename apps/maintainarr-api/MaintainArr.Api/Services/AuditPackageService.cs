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

    public AuditPackageManifestResponse GetManifest() =>
        new(
            PackageVersion: "1",
            Sections:
            [
                new("audit_events", "audit_events.json", "Audit events", "Tenant-scoped MaintainArr audit trail."),
                new("assets", "assets.json", "Assets", "Asset registry snapshot for the tenant."),
                new("work_orders", "work_orders.json", "Work orders", "Maintenance work order records."),
                new("defects", "defects.json", "Defects", "Asset defect capture and resolution records."),
                new("inspection_runs", "inspection_runs.json", "Inspection runs", "Completed and in-progress inspection runs."),
                new("pm_schedules", "pm_schedules.json", "PM schedules", "Preventive maintenance schedule due state."),
            ]);

    public async Task<AuditPackageExportResponse> BuildExportAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return package;
    }

    public async Task<byte[]> ExportZipAsync(
        Guid tenantId,
        Guid? actorUserId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await MaterializeExportAsync(tenantId, from, to, cancellationToken);
        var zipBytes = await CreateZipBytesAsync(package, cancellationToken);

        await auditService.WriteAsync(
            "audit_package.export",
            tenantId,
            actorUserId,
            "audit_package",
            package.PackageId.ToString(),
            "success",
            reasonCode: BuildDateRangeReasonCode(from, to),
            cancellationToken: cancellationToken);

        return zipBytes;
    }

    public async Task<AuditPackageExportResponse> MaterializeExportAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var package = await LoadPackageDataAsync(tenantId, from, to, cancellationToken);
        return package with { PackageId = Guid.NewGuid() };
    }

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
                package.Counts,
                PackageVersion = "1",
            }, cancellationToken);

            await WriteJsonEntryAsync(archive, "audit_events.json", package.AuditEvents, cancellationToken);
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
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken)
    {
        var auditEventsQuery = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
        auditEventsQuery = ApplyOccurredAtFilter(auditEventsQuery, from, to);

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
