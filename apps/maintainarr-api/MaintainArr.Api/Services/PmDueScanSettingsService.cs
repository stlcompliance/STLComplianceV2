using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class PmDueScanSettingsService(
    MaintainArrDbContext db,
    PmDueScanService pmDueScanService,
    IMaintainArrAuditService audit)
{
    public async Task<PmDueScanSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantPmDueScanSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var snapshot = settings is null ? null : ToSnapshot(settings);
        var pending = await pmDueScanService.ListPendingAsync(
            tenantId,
            DateTimeOffset.UtcNow,
            snapshot?.BatchSize ?? PmDueScanSettingsDefaults.BatchSize,
            cancellationToken);

        return settings is null
            ? DefaultResponse(pending.Items.Count)
            : MapResponse(settings, pending.Items.Count);
    }

    public async Task<PmDueScanSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPmDueScanSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantPmDueScanSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantPmDueScanSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantPmDueScanSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.ScanIntervalMinutes = PmDueScanSettingsRules.NormalizeScanIntervalMinutes(request.ScanIntervalMinutes);
        entity.BatchSize = PmDueScanSettingsRules.NormalizeBatchSize(request.BatchSize);
        entity.OverdueGraceDays = PmDueScanSettingsRules.NormalizeOverdueGraceDays(request.OverdueGraceDays);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.pm_due_scan_settings.update",
            tenantId,
            actorUserId,
            "tenant_pm_due_scan_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var pending = await pmDueScanService.ListPendingAsync(
            tenantId,
            DateTimeOffset.UtcNow,
            entity.BatchSize,
            cancellationToken);

        return MapResponse(entity, pending.Items.Count);
    }

    public async Task<TriggerPmDueScanResponse> TriggerManualScanAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantPmDueScanSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var snapshot = settings is null ? DefaultSnapshot() : ToSnapshot(settings);
        var result = await pmDueScanService.ProcessBatchAsync(
            new ProcessPmDueScanRequest(
                tenantId,
                DateTimeOffset.UtcNow,
                snapshot.BatchSize,
                snapshot.OverdueGraceDays),
            recordRun: true,
            cancellationToken: cancellationToken);

        await audit.WriteAsync(
            "maintainarr.pm_due_scan.trigger",
            tenantId,
            actorUserId,
            "tenant_pm_due_scan_settings",
            settings?.Id.ToString() ?? tenantId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new TriggerPmDueScanResponse(result);
    }

    internal async Task<TenantPmDueScanSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantPmDueScanSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantPmDueScanSettingsSnapshot ToSnapshot(TenantPmDueScanSettings settings) =>
        new(
            settings.IsEnabled,
            settings.ScanIntervalMinutes,
            settings.BatchSize,
            settings.OverdueGraceDays,
            settings.LastRunAt);

    private static TenantPmDueScanSettingsSnapshot DefaultSnapshot() =>
        new(
            IsEnabled: false,
            ScanIntervalMinutes: PmDueScanSettingsDefaults.ScanIntervalMinutes,
            BatchSize: PmDueScanSettingsDefaults.BatchSize,
            OverdueGraceDays: PmDueScanSettingsDefaults.OverdueGraceDays,
            LastRunAt: null);

    private static PmDueScanSettingsResponse DefaultResponse(int pendingPmCount) =>
        new(
            IsEnabled: false,
            ScanIntervalMinutes: PmDueScanSettingsDefaults.ScanIntervalMinutes,
            BatchSize: PmDueScanSettingsDefaults.BatchSize,
            OverdueGraceDays: PmDueScanSettingsDefaults.OverdueGraceDays,
            LastRunAt: null,
            PendingPmCount: pendingPmCount,
            UpdatedAt: null);

    private static PmDueScanSettingsResponse MapResponse(TenantPmDueScanSettings settings, int pendingPmCount) =>
        new(
            settings.IsEnabled,
            settings.ScanIntervalMinutes,
            settings.BatchSize,
            settings.OverdueGraceDays,
            settings.LastRunAt,
            pendingPmCount,
            settings.UpdatedAt);
}

public sealed record TenantPmDueScanSettingsSnapshot(
    bool IsEnabled,
    int ScanIntervalMinutes,
    int BatchSize,
    int OverdueGraceDays,
    DateTimeOffset? LastRunAt);
