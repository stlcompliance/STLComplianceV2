using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class AssetStatusRollupWorkerService(
    MaintainArrDbContext db,
    AssetStatusRollupSettingsService settingsService,
    AssetReadinessService assetReadiness,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue,
    IMaintainArrAuditService audit)
{
    public const string ProcessAssetStatusRollupsActionScope = "maintainarr.asset_status.rollup";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f7");

    public async Task<PendingAssetStatusRollupsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AssetStatusRollupRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = await ResolveStalenessHoursAsync(tenantId, stalenessHours, cancellationToken);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            stalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingAssetStatusRollupItem(x.AssetId, x.AssetTag, x.AssetName, x.LastComputedAt))
            .ToList();

        return new PendingAssetStatusRollupsResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessAssetStatusRollupsResponse> ProcessBatchAsync(
        ProcessAssetStatusRollupsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AssetStatusRollupRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = await ResolveStalenessHoursAsync(request.TenantId, request.StalenessHours, cancellationToken);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            request.StalenessHours,
            batchSize,
            cancellationToken);

        var refreshed = new List<AssetStatusRollupSummaryResponse>();
        var skipped = new List<AssetStatusRollupRefreshSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Refreshed, int Skipped, int ScopeRollups)>();
        var scopesToRefresh = new Dictionary<Guid, HashSet<ScopeRefreshTarget>>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0, 0);
                scopesToRefresh[candidate.TenantId] = [];
            }

            var stats = runStats[candidate.TenantId];
            stats.Candidates++;
            runStats[candidate.TenantId] = stats;

            try
            {
                var summary = await RefreshAssetRollupAsync(
                    candidate.TenantId,
                    candidate.AssetId,
                    asOf,
                    cancellationToken);
                refreshed.Add(summary);

                stats = runStats[candidate.TenantId];
                stats.Refreshed++;
                runStats[candidate.TenantId] = stats;

                scopesToRefresh[candidate.TenantId].Add(new ScopeRefreshTarget(
                    AssetStatusRollupScopeTypes.Fleet,
                    candidate.TenantId,
                    null,
                    "Fleet"));
                scopesToRefresh[candidate.TenantId].Add(new ScopeRefreshTarget(
                    AssetStatusRollupScopeTypes.AssetType,
                    candidate.AssetTypeId,
                    null,
                    candidate.AssetTypeName));
                scopesToRefresh[candidate.TenantId].Add(new ScopeRefreshTarget(
                    AssetStatusRollupScopeTypes.AssetClass,
                    candidate.AssetClassId,
                    null,
                    candidate.AssetClassName));

                var siteKey = AssetStatusRollupRules.NormalizeSiteKey(candidate.SiteRef);
                if (!string.IsNullOrWhiteSpace(siteKey))
                {
                    scopesToRefresh[candidate.TenantId].Add(new ScopeRefreshTarget(
                        AssetStatusRollupScopeTypes.Site,
                        Guid.Empty,
                        siteKey,
                        siteKey));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new AssetStatusRollupRefreshSkip(candidate.AssetId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        var totalScopeRollupsRefreshed = 0;
        foreach (var (tenantIdKey, targets) in scopesToRefresh)
        {
            foreach (var target in targets)
            {
                try
                {
                    await RefreshScopeRollupAsync(tenantIdKey, target, asOf, cancellationToken);
                    totalScopeRollupsRefreshed++;

                    var stats = runStats[tenantIdKey];
                    stats.ScopeRollups++;
                    runStats[tenantIdKey] = stats;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Scope refresh failures are non-fatal for the batch.
                    _ = ex;
                }
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.AssetStatusRollupRuns.Add(new AssetStatusRollupRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                RefreshedCount = stats.Refreshed,
                SkippedCount = stats.Skipped,
                ScopeRollupsRefreshed = stats.ScopeRollups,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && refreshed.Count > 0)
        {
            await audit.WriteAsync(
                "maintainarr.asset_status_rollup.batch",
                tenantId,
                WorkerActorUserId,
                "asset_status_rollup_run",
                $"{refreshed.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessAssetStatusRollupsResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            refreshed.Count,
            skipped.Count,
            totalScopeRollupsRefreshed,
            refreshed,
            skipped);
    }

    public async Task<AssetStatusRollupRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssetStatusRollupRules.NormalizeRunListLimit(limit);
        var runs = await db.AssetStatusRollupRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new AssetStatusRollupRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.RefreshedCount,
                x.SkippedCount,
                x.ScopeRollupsRefreshed,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssetStatusRollupRunsResponse(runs);
    }

    private async Task<AssetStatusRollupSummaryResponse> RefreshAssetRollupAsync(
        Guid tenantId,
        Guid assetId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var readiness = await assetReadiness.GetAsync(tenantId, assetId, cancellationToken);

        var existing = await db.AssetStatusRollups.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.AssetId == assetId,
            cancellationToken);

        var previousReadinessStatus = existing?.ReadinessStatus;
        var previousLifecycleStatus = existing?.LifecycleStatus;

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new AssetStatusRollup
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                CreatedAt = now,
            };
            db.AssetStatusRollups.Add(existing);
        }

        existing.AssetTag = readiness.AssetTag;
        existing.AssetName = readiness.AssetName;
        existing.LifecycleStatus = readiness.LifecycleStatus;
        existing.ReadinessStatus = readiness.ReadinessStatus;
        existing.ReadinessBasis = readiness.ReadinessBasis;
        existing.BlockerCount = readiness.Blockers.Count;
        existing.PrimaryBlockerMessage = readiness.Blockers.Count > 0 ? readiness.Blockers[0].Message : null;
        existing.OpenCriticalDefectCount = readiness.Signals.OpenCriticalDefectCount;
        existing.OpenHighDefectCount = readiness.Signals.OpenHighDefectCount;
        existing.ActiveWorkOrderCount = readiness.Signals.ActiveWorkOrderCount;
        existing.PmDueCount = readiness.Signals.PmDueCount;
        existing.PmOverdueCount = readiness.Signals.PmOverdueCount;
        existing.FailedInspectionCount = readiness.Signals.FailedInspectionCount;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await platformOutboxEnqueue.TryEnqueueReadinessTransitionsAsync(
            tenantId,
            readiness,
            previousReadinessStatus,
            previousLifecycleStatus,
            asOfUtc,
            cancellationToken);

        return MapAssetSummary(existing, isMaterialized: true);
    }

    private async Task RefreshScopeRollupAsync(
        Guid tenantId,
        ScopeRefreshTarget target,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var assetQuery = from asset in db.Assets.AsNoTracking()
            join assetType in db.AssetTypes.AsNoTracking()
                on asset.AssetTypeId equals assetType.Id
            where asset.TenantId == tenantId && assetType.TenantId == tenantId
            select new AssetScopeProjection(
                asset.Id,
                asset.AssetTypeId,
                assetType.AssetClassId,
                asset.SiteRef);

        assetQuery = target.ScopeType switch
        {
            AssetStatusRollupScopeTypes.Fleet => assetQuery,
            AssetStatusRollupScopeTypes.AssetType => assetQuery.Where(x => x.AssetTypeId == target.ScopeEntityId),
            AssetStatusRollupScopeTypes.AssetClass => assetQuery.Where(x => x.AssetClassId == target.ScopeEntityId),
            AssetStatusRollupScopeTypes.Site => assetQuery.Where(x =>
                x.SiteRef != null
                && x.SiteRef == target.ScopeEntityKey),
            _ => throw new InvalidOperationException($"Unsupported scope type {target.ScopeType}."),
        };

        var assetIds = await assetQuery.Select(x => x.AssetId).ToListAsync(cancellationToken);
        var snapshots = await LoadAssetSnapshotsAsync(tenantId, assetIds, cancellationToken);
        var (readyCount, notReadyCount) = AssetStatusRollupRules.AggregateAssetCounts(snapshots);
        var totalAssets = snapshots.Count;
        var readyPercent = AssetStatusRollupRules.ComputeReadyPercent(totalAssets, readyCount);

        var existing = await db.AssetStatusScopeRollups.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.ScopeType == target.ScopeType
                && x.ScopeEntityId == target.ScopeEntityId
                && x.ScopeEntityKey == target.ScopeEntityKey,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new AssetStatusScopeRollup
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ScopeType = target.ScopeType,
                ScopeEntityId = target.ScopeEntityId,
                ScopeEntityKey = target.ScopeEntityKey,
                CreatedAt = now,
            };
            db.AssetStatusScopeRollups.Add(existing);
        }

        existing.ScopeLabel = target.ScopeLabel;
        existing.TotalAssets = totalAssets;
        existing.ReadyCount = readyCount;
        existing.NotReadyCount = notReadyCount;
        existing.ReadyPercent = readyPercent;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<AssetStatusRollupSnapshot>> LoadAssetSnapshotsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> assetIds,
        CancellationToken cancellationToken)
    {
        if (assetIds.Count == 0)
        {
            return [];
        }

        var rollups = await db.AssetStatusRollups.AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.AssetId))
            .ToListAsync(cancellationToken);

        var rollupByAsset = rollups.ToDictionary(x => x.AssetId);
        var snapshots = new List<AssetStatusRollupSnapshot>();

        foreach (var assetId in assetIds)
        {
            if (rollupByAsset.TryGetValue(assetId, out var rollup))
            {
                snapshots.Add(new AssetStatusRollupSnapshot(assetId, rollup.ReadinessStatus));
                continue;
            }

            var readiness = await assetReadiness.GetAsync(tenantId, assetId, cancellationToken);
            snapshots.Add(new AssetStatusRollupSnapshot(assetId, readiness.ReadinessStatus));
        }

        return snapshots;
    }

    private async Task<int> ResolveStalenessHoursAsync(
        Guid? tenantId,
        int? overrideStalenessHours,
        CancellationToken cancellationToken)
    {
        if (overrideStalenessHours is not null)
        {
            return AssetStatusRollupRules.NormalizeStalenessHours(overrideStalenessHours);
        }

        if (tenantId is Guid scopedTenantId)
        {
            var snapshot = await settingsService.LoadSnapshotAsync(scopedTenantId, cancellationToken);
            return AssetStatusRollupRules.NormalizeStalenessHours(snapshot?.StalenessHours);
        }

        return AssetStatusRollupDefaults.StalenessHours;
    }

    private async Task<IReadOnlyList<PendingAssetCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int? overrideStalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var tenantStaleness = await db.TenantAssetStatusRollupSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .ToDictionaryAsync(x => x.TenantId, x => x.StalenessHours, cancellationToken);

        if (tenantStaleness.Count == 0)
        {
            return [];
        }

        var enabledTenantIds = tenantStaleness.Keys.ToList();

        var assets = await (
            from asset in db.Assets.AsNoTracking()
            join assetType in db.AssetTypes.AsNoTracking()
                on asset.AssetTypeId equals assetType.Id
            join assetClass in db.AssetClasses.AsNoTracking()
                on assetType.AssetClassId equals assetClass.Id
            where enabledTenantIds.Contains(asset.TenantId)
                && assetType.TenantId == asset.TenantId
                && assetClass.TenantId == asset.TenantId
            orderby asset.TenantId, asset.AssetTag
            select new PendingAssetCandidate(
                asset.Id,
                asset.TenantId,
                asset.AssetTag,
                asset.Name,
                asset.SiteRef,
                asset.AssetTypeId,
                assetType.Name,
                assetType.AssetClassId,
                assetClass.Name,
                null))
            .ToListAsync(cancellationToken);

        var rollupLookup = await db.AssetStatusRollups.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => (x.TenantId, x.AssetId), x => x.ComputedAt, cancellationToken);

        var pending = new List<PendingAssetCandidate>();
        foreach (var asset in assets)
        {
            rollupLookup.TryGetValue((asset.TenantId, asset.AssetId), out var computedAt);
            var stalenessHours = overrideStalenessHours ?? tenantStaleness[asset.TenantId];
            if (!AssetStatusRollupRules.IsStale(computedAt, asOfUtc, stalenessHours))
            {
                continue;
            }

            pending.Add(asset with { LastComputedAt = computedAt });
            if (pending.Count >= batchSize)
            {
                break;
            }
        }

        return pending
            .OrderBy(x => x.LastComputedAt.HasValue ? 1 : 0)
            .ThenBy(x => x.LastComputedAt)
            .Take(batchSize)
            .ToList();
    }

    internal static AssetStatusRollupSummaryResponse MapAssetSummary(
        AssetStatusRollup rollup,
        bool isMaterialized) =>
        new(
            rollup.AssetId,
            rollup.AssetTag,
            rollup.AssetName,
            rollup.LifecycleStatus,
            rollup.ReadinessStatus,
            rollup.BlockerCount,
            rollup.PrimaryBlockerMessage,
            rollup.ComputedAt,
            isMaterialized);

    internal static AssetStatusScopeRollupSummaryResponse MapScopeSummary(AssetStatusScopeRollup rollup) =>
        new(
            rollup.ScopeType,
            rollup.ScopeEntityId,
            rollup.ScopeEntityKey,
            rollup.ScopeLabel,
            rollup.TotalAssets,
            rollup.ReadyCount,
            rollup.NotReadyCount,
            rollup.ReadyPercent,
            rollup.ComputedAt);

    private sealed record PendingAssetCandidate(
        Guid AssetId,
        Guid TenantId,
        string AssetTag,
        string AssetName,
        string? SiteRef,
        Guid AssetTypeId,
        string AssetTypeName,
        Guid AssetClassId,
        string AssetClassName,
        DateTimeOffset? LastComputedAt);

    private sealed record AssetScopeProjection(
        Guid AssetId,
        Guid AssetTypeId,
        Guid AssetClassId,
        string? SiteRef);

    private sealed record ScopeRefreshTarget(
        string ScopeType,
        Guid ScopeEntityId,
        string? ScopeEntityKey,
        string ScopeLabel);
}

