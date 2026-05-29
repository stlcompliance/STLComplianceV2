using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class DowntimeTrackingSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<DowntimeTrackingSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDowntimeTrackingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<DowntimeTrackingSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertDowntimeTrackingSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantDowntimeTrackingSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantDowntimeTrackingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantDowntimeTrackingSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.AutoTrackOutOfService = request.AutoTrackOutOfService;
        entity.AutoTrackNotReady = request.AutoTrackNotReady;
        entity.AvailabilityPeriodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(request.AvailabilityPeriodDays);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.downtime_tracking_settings.update",
            tenantId,
            actorUserId,
            "tenant_downtime_tracking_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantDowntimeTrackingSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDowntimeTrackingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantDowntimeTrackingSettingsSnapshot ToSnapshot(TenantDowntimeTrackingSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoTrackOutOfService,
            settings.AutoTrackNotReady,
            settings.AvailabilityPeriodDays);

    private static DowntimeTrackingSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            AutoTrackOutOfService: true,
            AutoTrackNotReady: true,
            AvailabilityPeriodDays: AssetDowntimeDefaults.AvailabilityPeriodDays,
            UpdatedAt: null);

    private static DowntimeTrackingSettingsResponse MapResponse(TenantDowntimeTrackingSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoTrackOutOfService,
            settings.AutoTrackNotReady,
            settings.AvailabilityPeriodDays,
            settings.UpdatedAt);
}

public sealed record TenantDowntimeTrackingSettingsSnapshot(
    bool IsEnabled,
    bool AutoTrackOutOfService,
    bool AutoTrackNotReady,
    int AvailabilityPeriodDays);
