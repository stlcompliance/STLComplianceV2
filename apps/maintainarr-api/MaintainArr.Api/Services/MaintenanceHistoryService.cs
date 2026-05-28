using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceHistoryService(MaintainArrDbContext db)
{
    private const int MaxPageSize = 100;

    public async Task<MaintenanceHistorySummaryResponse> GetSummaryAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssetExistsAsync(tenantId, assetId, cancellationToken);

        var rollup = await db.MaintenanceHistoryRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        if (rollup is null)
        {
            return new MaintenanceHistorySummaryResponse(
                assetId,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                0,
                null,
                DateTimeOffset.MinValue,
                IsMaterialized: false);
        }

        return MaintenanceHistoryRollupWorkerService.MapSummary(rollup, isMaterialized: true);
    }

    public async Task<PagedResult<MaintenanceHistoryEntryResponse>> ListAsync(
        Guid tenantId,
        Guid assetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssetExistsAsync(tenantId, assetId, cancellationToken);

        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 50,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

        var asOf = DateTimeOffset.UtcNow;
        var materialized = await TryListMaterializedEventsAsync(
            tenantId,
            assetId,
            page,
            pageSize,
            asOf,
            MaintenanceHistoryRules.DefaultReadStalenessHours,
            cancellationToken);

        if (materialized is not null)
        {
            return materialized;
        }

        var liveEntries = await MaintenanceHistoryTimelineBuilder.BuildTimelineEntriesAsync(
            db,
            tenantId,
            assetId,
            cancellationToken);
        return PageTimelineEntries(liveEntries, page, pageSize);
    }

    public async Task<PagedResult<MaintenanceHistoryEntryResponse>?> TryListMaterializedEventsAsync(
        Guid tenantId,
        Guid assetId,
        int page,
        int pageSize,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var rollup = await db.MaintenanceHistoryRollups.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        if (rollup is null || MaintenanceHistoryRules.IsStale(rollup.ComputedAt, asOfUtc, stalenessHours))
        {
            return null;
        }

        var query = db.MaintenanceHistoryEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetId == assetId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MaintenanceHistoryEntryResponse(
                x.EntryId,
                x.AssetId,
                x.Category,
                x.EventType,
                x.Title,
                x.Detail,
                x.OccurredAt,
                x.ActorUserId,
                x.SourceEntityType,
                x.SourceEntityId,
                x.RelatedEntityId))
            .ToListAsync(cancellationToken);

        return new PagedResult<MaintenanceHistoryEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private static PagedResult<MaintenanceHistoryEntryResponse> PageTimelineEntries(
        List<MaintenanceHistoryEntryResponse> entries,
        int page,
        int pageSize)
    {
        var total = entries.Count;
        var items = entries
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.EntryId, StringComparer.Ordinal)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<MaintenanceHistoryEntryResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private async Task EnsureAssetExistsAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var assetExists = await db.Assets.AnyAsync(
            x => x.TenantId == tenantId && x.Id == assetId,
            cancellationToken);
        if (!assetExists)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }
    }
}
