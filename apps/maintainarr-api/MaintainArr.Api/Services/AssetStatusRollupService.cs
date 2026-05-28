using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetStatusRollupService(
    MaintainArrDbContext db,
    AssetReadinessService assetReadiness)
{
    public async Task<AssetStatusScopeRollupSummaryResponse> GetFleetRollupAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.AssetStatusScopeRollups.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ScopeType == AssetStatusRollupScopeTypes.Fleet
                    && x.ScopeEntityId == tenantId,
                cancellationToken);

        if (rollup is null)
        {
            throw new StlApiException(
                "asset_status_rollup.not_found",
                "Fleet asset status rollup has not been computed yet.",
                404);
        }

        return AssetStatusRollupWorkerService.MapScopeSummary(rollup);
    }

    public async Task<IReadOnlyList<AssetStatusScopeRollupSummaryResponse>> ListAssetTypeRollupsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.AssetStatusScopeRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == AssetStatusRollupScopeTypes.AssetType)
            .OrderBy(x => x.ScopeLabel)
            .ToListAsync(cancellationToken);

        return rollups.Select(AssetStatusRollupWorkerService.MapScopeSummary).ToList();
    }

    public async Task<AssetStatusScopeRollupSummaryResponse> GetAssetTypeRollupAsync(
        Guid tenantId,
        Guid assetTypeId,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.AssetStatusScopeRollups.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.ScopeType == AssetStatusRollupScopeTypes.AssetType
                    && x.ScopeEntityId == assetTypeId,
                cancellationToken);

        if (rollup is null)
        {
            throw new StlApiException(
                "asset_status_rollup.not_found",
                "Asset type status rollup has not been computed yet.",
                404);
        }

        return AssetStatusRollupWorkerService.MapScopeSummary(rollup);
    }

    public async Task<IReadOnlyList<AssetStatusScopeRollupSummaryResponse>> ListAssetClassRollupsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.AssetStatusScopeRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == AssetStatusRollupScopeTypes.AssetClass)
            .OrderBy(x => x.ScopeLabel)
            .ToListAsync(cancellationToken);

        return rollups.Select(AssetStatusRollupWorkerService.MapScopeSummary).ToList();
    }

    public async Task<IReadOnlyList<AssetStatusScopeRollupSummaryResponse>> ListSiteRollupsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.AssetStatusScopeRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ScopeType == AssetStatusRollupScopeTypes.Site)
            .OrderBy(x => x.ScopeLabel)
            .ToListAsync(cancellationToken);

        return rollups.Select(AssetStatusRollupWorkerService.MapScopeSummary).ToList();
    }

    public async Task<AssetStatusRollupSummaryResponse> GetAssetRollupAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.AssetStatusRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        if (rollup is not null)
        {
            return AssetStatusRollupWorkerService.MapAssetSummary(rollup, isMaterialized: true);
        }

        var readiness = await assetReadiness.GetAsync(tenantId, assetId, cancellationToken);
        return new AssetStatusRollupSummaryResponse(
            readiness.AssetId,
            readiness.AssetTag,
            readiness.AssetName,
            readiness.LifecycleStatus,
            readiness.ReadinessStatus,
            readiness.Blockers.Count,
            readiness.Blockers.Count > 0 ? readiness.Blockers[0].Message : null,
            readiness.CalculatedAt,
            IsMaterialized: false);
    }

    public async Task<IReadOnlyList<AssetStatusRollupSummaryResponse>> ListAssetRollupsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.AssetStatusRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.AssetName)
            .ToListAsync(cancellationToken);

        if (rollups.Count > 0)
        {
            return rollups
                .Select(x => AssetStatusRollupWorkerService.MapAssetSummary(x, isMaterialized: true))
                .ToList();
        }

        var fleet = await assetReadiness.ListFleetAsync(tenantId, cancellationToken);
        return fleet
            .Select(x => new AssetStatusRollupSummaryResponse(
                x.AssetId,
                x.AssetTag,
                x.AssetName,
                x.LifecycleStatus,
                x.ReadinessStatus,
                x.BlockerCount,
                x.PrimaryBlockerMessage,
                DateTimeOffset.UtcNow,
                IsMaterialized: false))
            .ToList();
    }

    public async Task<AssetStatusRollupSummaryResponse?> TryGetMaterializedAssetRollupAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.AssetStatusRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        if (rollup is null || AssetStatusRollupRules.IsStale(rollup.ComputedAt, asOfUtc, stalenessHours))
        {
            return null;
        }

        return AssetStatusRollupWorkerService.MapAssetSummary(rollup, isMaterialized: true);
    }

    public async Task<IReadOnlyList<AssetStatusRollupSummaryResponse>> TryListMaterializedFleetAsync(
        Guid tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var rollups = await db.AssetStatusRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.AssetTag)
            .ThenBy(x => x.AssetName)
            .ToListAsync(cancellationToken);

        if (rollups.Count == 0)
        {
            return [];
        }

        if (rollups.Any(x => AssetStatusRollupRules.IsStale(x.ComputedAt, asOfUtc, stalenessHours)))
        {
            return [];
        }

        return rollups
            .Select(x => AssetStatusRollupWorkerService.MapAssetSummary(x, isMaterialized: true))
            .ToList();
    }
}
