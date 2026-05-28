using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class AssetStatusRollupSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<AssetStatusRollupSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssetStatusRollupSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<AssetStatusRollupSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertAssetStatusRollupSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantAssetStatusRollupSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantAssetStatusRollupSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantAssetStatusRollupSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = AssetStatusRollupRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.asset_status_rollup_settings.update",
            tenantId,
            actorUserId,
            "tenant_asset_status_rollup_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantAssetStatusRollupSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssetStatusRollupSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantAssetStatusRollupSettingsSnapshot ToSnapshot(TenantAssetStatusRollupSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours);

    private static AssetStatusRollupSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: AssetStatusRollupDefaults.StalenessHours,
            UpdatedAt: null);

    private static AssetStatusRollupSettingsResponse MapResponse(TenantAssetStatusRollupSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}

public sealed record TenantAssetStatusRollupSettingsSnapshot(
    bool IsEnabled,
    int StalenessHours);
