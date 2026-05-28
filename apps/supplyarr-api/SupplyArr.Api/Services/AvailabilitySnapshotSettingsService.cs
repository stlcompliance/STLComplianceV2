using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class AvailabilitySnapshotSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<AvailabilitySnapshotSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAvailabilitySnapshotSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<AvailabilitySnapshotSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertAvailabilitySnapshotSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantAvailabilitySnapshotSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantAvailabilitySnapshotSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantAvailabilitySnapshotSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = AvailabilitySnapshotCaptureRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.availability_snapshot_settings.update",
            tenantId,
            actorUserId,
            "tenant_availability_snapshot_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static AvailabilitySnapshotSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: AvailabilitySnapshotWorkerDefaults.StalenessHours,
            UpdatedAt: null);

    private static AvailabilitySnapshotSettingsResponse MapResponse(TenantAvailabilitySnapshotSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
