using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class StaffArrWorkerAdminService(
    StaffArrDbContext db,
    CertificationExpirationService certificationExpirationService,
    ReadinessRollupService readinessRollupService,
    PermissionProjectionService permissionProjectionService,
    PersonnelHistoryService personnelHistoryService,
    AuditPackageGenerationService auditPackageGenerationService,
    IStaffArrAuditService audit)
{
    public async Task<StaffArrWorkerSettingsResponse> GetSettingsAsync(
        string workerKey,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = StaffArrWorkerAdminRules.NormalizeWorkerKey(workerKey);
        var settings = await LoadOrDefaultAsync(normalizedKey, tenantId, cancellationToken);
        var pendingCount = await CountPendingAsync(normalizedKey, tenantId, settings, cancellationToken);

        return MapSettingsResponse(normalizedKey, settings, pendingCount);
    }

    public async Task<StaffArrWorkerSettingsResponse> UpsertSettingsAsync(
        string workerKey,
        Guid tenantId,
        Guid actorUserId,
        UpsertStaffArrWorkerSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = StaffArrWorkerAdminRules.NormalizeWorkerKey(workerKey);
        var entity = await db.TenantStaffArrWorkerSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkerKey == normalizedKey, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantStaffArrWorkerSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                WorkerKey = normalizedKey,
                CreatedAt = now,
            };
            db.TenantStaffArrWorkerSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.ScanIntervalMinutes = StaffArrWorkerAdminRules.NormalizeScanIntervalMinutes(request.ScanIntervalMinutes);
        entity.BatchSize = StaffArrWorkerAdminRules.NormalizeBatchSize(request.BatchSize, normalizedKey);
        entity.StalenessHours = StaffArrWorkerAdminRules.NormalizeStalenessHours(request.StalenessHours, normalizedKey);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "staffarr_worker_settings.upsert",
            tenantId,
            actorUserId,
            "staffarr_worker_settings",
            normalizedKey,
            "success",
            cancellationToken: cancellationToken);

        var pendingCount = await CountPendingAsync(normalizedKey, tenantId, entity, cancellationToken);
        return MapSettingsResponse(normalizedKey, entity, pendingCount);
    }

    public async Task<StaffArrWorkerPendingPreviewResponse> ListPendingPreviewAsync(
        string workerKey,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = StaffArrWorkerAdminRules.NormalizeWorkerKey(workerKey);
        var settings = await LoadOrDefaultAsync(normalizedKey, tenantId, cancellationToken);
        var batchSize = settings.BatchSize;
        var asOf = DateTimeOffset.UtcNow;

        var previewLines = normalizedKey switch
        {
            StaffArrWorkerKeys.CertificationExpiration => (await certificationExpirationService.ListPendingAsync(
                tenantId,
                asOf,
                batchSize,
                cancellationToken)).Items
                .Select(x => $"{x.PersonCertificationId} expires {x.ExpiresAt:yyyy-MM-dd}")
                .ToList(),
            StaffArrWorkerKeys.ReadinessRollup => (await readinessRollupService.ListPendingAsync(
                tenantId,
                asOf,
                batchSize,
                settings.StalenessHours,
                cancellationToken)).Items
                .Select(x => $"{x.ScopeType} {x.OrgUnitName}")
                .ToList(),
            StaffArrWorkerKeys.PermissionProjection => (await permissionProjectionService.ListPendingAsync(
                tenantId,
                asOf,
                batchSize,
                settings.StalenessHours,
                cancellationToken)).Items
                .Select(x => x.PersonId.ToString())
                .ToList(),
            StaffArrWorkerKeys.PersonnelHistoryRollup => (await personnelHistoryService.ListPendingAsync(
                tenantId,
                asOf,
                batchSize,
                settings.StalenessHours,
                cancellationToken)).Items
                .Select(x => x.PersonId.ToString())
                .ToList(),
            StaffArrWorkerKeys.AuditPackageGeneration => (await auditPackageGenerationService.ListPendingAsync(
                tenantId,
                asOf,
                batchSize,
                cancellationToken)).Items
                .Select(x => $"{x.JobId} ({x.Format})")
                .ToList(),
            _ => [],
        };

        return new StaffArrWorkerPendingPreviewResponse(
            normalizedKey,
            asOf,
            batchSize,
            previewLines.Count,
            previewLines);
    }

    public async Task<StaffArrWorkerRunsResponse> ListRecentRunsAsync(
        string workerKey,
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = StaffArrWorkerAdminRules.NormalizeWorkerKey(workerKey);
        var normalizedLimit = StaffArrWorkerAdminRules.NormalizeRunListLimit(limit);

        var runs = await db.StaffArrWorkerRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkerKey == normalizedKey)
            .OrderByDescending(x => x.StartedAt)
            .Take(normalizedLimit)
            .Select(x => new StaffArrWorkerRunItem(
                x.Id,
                x.Status,
                x.CandidatesFound,
                x.ProcessedCount,
                x.SkippedCount,
                x.Summary,
                x.StartedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);

        return new StaffArrWorkerRunsResponse(runs);
    }

    private async Task<TenantStaffArrWorkerSettings> LoadOrDefaultAsync(
        string workerKey,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entity = await db.TenantStaffArrWorkerSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.WorkerKey == workerKey, cancellationToken);

        if (entity is not null)
        {
            return entity;
        }

        return new TenantStaffArrWorkerSettings
        {
            TenantId = tenantId,
            WorkerKey = workerKey,
            IsEnabled = false,
            ScanIntervalMinutes = StaffArrWorkerAdminRules.DefaultScanIntervalMinutes(workerKey),
            BatchSize = StaffArrWorkerAdminRules.DefaultBatchSize(workerKey),
            StalenessHours = StaffArrWorkerAdminRules.SupportsStaleness(workerKey)
                ? StaffArrWorkerAdminRules.DefaultStalenessHours(workerKey)
                : null,
        };
    }

    private async Task<int> CountPendingAsync(
        string workerKey,
        Guid tenantId,
        TenantStaffArrWorkerSettings settings,
        CancellationToken cancellationToken)
    {
        var preview = await ListPendingPreviewAsync(workerKey, tenantId, cancellationToken);
        return preview.ItemCount;
    }

    private static StaffArrWorkerSettingsResponse MapSettingsResponse(
        string workerKey,
        TenantStaffArrWorkerSettings settings,
        int pendingCount) =>
        new(
            workerKey,
            settings.IsEnabled,
            settings.ScanIntervalMinutes,
            settings.BatchSize,
            settings.StalenessHours,
            settings.LastRunAt,
            pendingCount);
}
