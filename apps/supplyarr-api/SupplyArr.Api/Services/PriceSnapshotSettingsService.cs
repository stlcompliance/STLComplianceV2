using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class PriceSnapshotSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<PriceSnapshotSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantPriceSnapshotSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<PriceSnapshotSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPriceSnapshotSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantPriceSnapshotSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantPriceSnapshotSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantPriceSnapshotSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = PriceSnapshotCaptureRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.price_snapshot_settings.update",
            tenantId,
            actorUserId,
            "tenant_price_snapshot_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static PriceSnapshotSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: PriceSnapshotWorkerDefaults.StalenessHours,
            UpdatedAt: null);

    private static PriceSnapshotSettingsResponse MapResponse(TenantPriceSnapshotSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
