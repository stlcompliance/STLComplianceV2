using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenancePlatformEventSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<MaintenancePlatformEventSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantMaintenancePlatformEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<MaintenancePlatformEventSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertMaintenancePlatformEventSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantMaintenancePlatformEventSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantMaintenancePlatformEventSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantMaintenancePlatformEventSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.MaxAttempts = MaintenancePlatformEventRules.NormalizeMaxAttempts(request.MaxAttempts);
        entity.RetryIntervalMinutes = MaintenancePlatformEventRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.platform_event_settings.update",
            tenantId,
            actorUserId,
            "tenant_maintenance_platform_event_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantMaintenancePlatformEventSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantMaintenancePlatformEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantMaintenancePlatformEventSettingsSnapshot ToSnapshot(
        TenantMaintenancePlatformEventSettings settings) =>
        new(
            settings.IsEnabled,
            settings.MaxAttempts,
            settings.RetryIntervalMinutes);

    private static MaintenancePlatformEventSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: true,
            MaxAttempts: MaintenancePlatformEventRules.DefaultMaxAttempts,
            RetryIntervalMinutes: MaintenancePlatformEventRules.DefaultRetryIntervalMinutes,
            UpdatedAt: null);

    private static MaintenancePlatformEventSettingsResponse MapResponse(
        TenantMaintenancePlatformEventSettings settings) =>
        new(
            settings.IsEnabled,
            settings.MaxAttempts,
            settings.RetryIntervalMinutes,
            settings.UpdatedAt);
}
