using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class AssetDowntimeSyncWorkerService(
    MaintainArrDbContext db,
    DowntimeTrackingSettingsService settingsService,
    AssetDowntimeService downtimeService,
    AssetReadinessService assetReadiness,
    IMaintainArrAuditService audit)
{
    public const string ProcessAssetDowntimeSyncActionScope = "maintainarr.downtime.sync";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    public async Task<PendingAssetDowntimeSyncResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AssetDowntimeRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadSyncCandidatesAsync(tenantId, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingAssetDowntimeSyncItem(
                x.AssetId,
                x.AssetTag,
                x.AssetName,
                x.LifecycleStatus,
                x.ReadinessStatus,
                x.HasOpenAutomaticEvent))
            .ToList();

        return new PendingAssetDowntimeSyncResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessAssetDowntimeSyncResponse> ProcessBatchAsync(
        ProcessAssetDowntimeSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AssetDowntimeRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadSyncCandidatesAsync(request.TenantId, batchSize, cancellationToken);

        var eventsOpened = 0;
        var eventsClosed = 0;
        var snapshotsRefreshed = 0;
        var refreshedAssets = new List<AssetAvailabilityResponse>();
        var runStats = new Dictionary<Guid, (int Scanned, int Opened, int Closed, int Snapshots)>();
        var tenantsToRefreshFleet = new HashSet<Guid>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0, 0);
            }

            var stats = runStats[candidate.TenantId];
            stats.Scanned++;
            runStats[candidate.TenantId] = stats;

            var syncResult = await SyncAssetAutomaticDowntimeAsync(candidate, asOf, cancellationToken);
            if (syncResult.Opened)
            {
                eventsOpened++;
                stats = runStats[candidate.TenantId];
                stats.Opened++;
                runStats[candidate.TenantId] = stats;
            }

            if (syncResult.Closed)
            {
                eventsClosed++;
                stats = runStats[candidate.TenantId];
                stats.Closed++;
                runStats[candidate.TenantId] = stats;
            }

            try
            {
                var settings = await settingsService.LoadSnapshotAsync(candidate.TenantId, cancellationToken);
                var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
                var periodEnd = asOf;
                var periodStart = periodEnd.AddDays(-periodDays);

                await downtimeService.RefreshAssetAvailabilitySnapshotAsync(
                    candidate.TenantId,
                    candidate.AssetId,
                    periodStart,
                    periodEnd,
                    asOf,
                    cancellationToken);

                snapshotsRefreshed++;
                stats = runStats[candidate.TenantId];
                stats.Snapshots++;
                runStats[candidate.TenantId] = stats;
                tenantsToRefreshFleet.Add(candidate.TenantId);

                var snapshot = await db.AssetAvailabilitySnapshots
                    .AsNoTracking()
                    .FirstAsync(
                        x => x.TenantId == candidate.TenantId && x.AssetId == candidate.AssetId,
                        cancellationToken);
                refreshedAssets.Add(AssetDowntimeService.MapAssetAvailability(snapshot, isMaterialized: true));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _ = ex;
            }
        }

        foreach (var tenantId in tenantsToRefreshFleet)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
                var periodDays = AssetDowntimeRules.NormalizeAvailabilityPeriodDays(settings?.AvailabilityPeriodDays);
                var periodEnd = asOf;
                var periodStart = periodEnd.AddDays(-periodDays);
                await downtimeService.RefreshFleetAvailabilitySnapshotAsync(
                    tenantId,
                    periodStart,
                    periodEnd,
                    asOf,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _ = ex;
            }
        }

        foreach (var (tenantId, stats) in runStats)
        {
            db.AssetDowntimeSyncRuns.Add(new AssetDowntimeSyncRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AsOfUtc = asOf,
                AssetsScanned = stats.Scanned,
                EventsOpened = stats.Opened,
                EventsClosed = stats.Closed,
                SnapshotsRefreshed = stats.Snapshots,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid scopedTenantId && (eventsOpened > 0 || eventsClosed > 0))
        {
            await audit.WriteAsync(
                "maintainarr.downtime_sync.batch",
                scopedTenantId,
                WorkerActorUserId,
                "asset_downtime_sync_run",
                $"{eventsOpened + eventsClosed}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessAssetDowntimeSyncResponse(
            asOf,
            batchSize,
            candidates.Count,
            eventsOpened,
            eventsClosed,
            snapshotsRefreshed,
            refreshedAssets);
    }

    public async Task<AssetDowntimeSyncRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssetDowntimeRules.NormalizeRunListLimit(limit);
        var runs = await db.AssetDowntimeSyncRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new AssetDowntimeSyncRunItem(
                x.Id,
                x.AsOfUtc,
                x.AssetsScanned,
                x.EventsOpened,
                x.EventsClosed,
                x.SnapshotsRefreshed,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssetDowntimeSyncRunsResponse(runs);
    }

    private async Task<(bool Opened, bool Closed)> SyncAssetAutomaticDowntimeAsync(
        AssetDowntimeSyncCandidate candidate,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var shouldBeDown = AssetDowntimeRules.IsAutomaticDowntimeState(
            candidate.LifecycleStatus,
            candidate.ReadinessStatus,
            candidate.AutoTrackOutOfService,
            candidate.AutoTrackNotReady);

        var openEvent = await db.AssetDowntimeEvents
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId
                    && x.AssetId == candidate.AssetId
                    && x.Source == AssetDowntimeSources.AutomaticStatus
                    && x.EndedAt == null,
                cancellationToken);

        if (shouldBeDown && openEvent is null)
        {
            var now = DateTimeOffset.UtcNow;
            db.AssetDowntimeEvents.Add(new AssetDowntimeEvent
            {
                Id = Guid.NewGuid(),
                TenantId = candidate.TenantId,
                AssetId = candidate.AssetId,
                AssetTag = candidate.AssetTag,
                AssetName = candidate.AssetName,
                Source = AssetDowntimeSources.AutomaticStatus,
                Reason = AssetDowntimeRules.ResolveAutomaticReason(
                    candidate.LifecycleStatus,
                    candidate.ReadinessStatus),
                IsPlanned = false,
                StartedAt = asOfUtc,
                StatusTrigger = AssetDowntimeRules.ResolveAutomaticStatusTrigger(
                    candidate.LifecycleStatus,
                    candidate.ReadinessStatus),
                CreatedAt = now,
                UpdatedAt = now,
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false);
        }

        if (!shouldBeDown && openEvent is not null)
        {
            openEvent.EndedAt = asOfUtc;
            openEvent.ClosedByUserId = WorkerActorUserId;
            openEvent.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return (false, true);
        }

        return (false, false);
    }

    private async Task<IReadOnlyList<AssetDowntimeSyncCandidate>> LoadSyncCandidatesAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledSettings = await db.TenantDowntimeTrackingSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .ToDictionaryAsync(x => x.TenantId, x => x, cancellationToken);

        if (enabledSettings.Count == 0)
        {
            return [];
        }

        var enabledTenantIds = enabledSettings.Keys.ToList();
        var assets = await (
            from asset in db.Assets.AsNoTracking()
            where enabledTenantIds.Contains(asset.TenantId)
            orderby asset.TenantId, asset.AssetTag
            select asset)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var assetIds = assets.Select(x => x.Id).ToList();
        var rollups = await db.AssetStatusRollups
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && assetIds.Contains(x.AssetId))
            .ToDictionaryAsync(x => (x.TenantId, x.AssetId), cancellationToken);

        var openAutomaticEvents = await db.AssetDowntimeEvents
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId)
                && assetIds.Contains(x.AssetId)
                && x.Source == AssetDowntimeSources.AutomaticStatus
                && x.EndedAt == null)
            .Select(x => new { x.TenantId, x.AssetId })
            .ToListAsync(cancellationToken);
        var openEventLookup = openAutomaticEvents
            .Select(x => (x.TenantId, x.AssetId))
            .ToHashSet();

        var candidates = new List<AssetDowntimeSyncCandidate>();
        foreach (var asset in assets)
        {
            var settings = enabledSettings[asset.TenantId];
            string lifecycleStatus;
            string readinessStatus;

            if (rollups.TryGetValue((asset.TenantId, asset.Id), out var rollup))
            {
                lifecycleStatus = rollup.LifecycleStatus;
                readinessStatus = rollup.ReadinessStatus;
            }
            else
            {
                var readiness = await assetReadiness.GetAsync(asset.TenantId, asset.Id, cancellationToken);
                lifecycleStatus = readiness.LifecycleStatus;
                readinessStatus = readiness.ReadinessStatus;
            }

            candidates.Add(new AssetDowntimeSyncCandidate(
                asset.Id,
                asset.TenantId,
                asset.AssetTag,
                asset.Name,
                lifecycleStatus,
                readinessStatus,
                settings.AutoTrackOutOfService,
                settings.AutoTrackNotReady,
                openEventLookup.Contains((asset.TenantId, asset.Id))));
        }

        return candidates;
    }

    private sealed record AssetDowntimeSyncCandidate(
        Guid AssetId,
        Guid TenantId,
        string AssetTag,
        string AssetName,
        string LifecycleStatus,
        string ReadinessStatus,
        bool AutoTrackOutOfService,
        bool AutoTrackNotReady,
        bool HasOpenAutomaticEvent);
}
