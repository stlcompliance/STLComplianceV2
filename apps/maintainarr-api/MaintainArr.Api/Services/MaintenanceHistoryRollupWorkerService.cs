using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceHistoryRollupWorkerService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public const string ProcessMaintenanceHistoryRollupsActionScope = "maintainarr.maintenance_history.rollup";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    public async Task<PendingMaintenanceHistoryRollupsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = MaintenanceHistoryRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = MaintenanceHistoryRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingMaintenanceHistoryRollupItem(
                x.AssetId,
                x.AssetTag,
                x.AssetName,
                x.LastComputedAt))
            .ToList();

        return new PendingMaintenanceHistoryRollupsResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessMaintenanceHistoryRollupsResponse> ProcessBatchAsync(
        ProcessMaintenanceHistoryRollupsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = MaintenanceHistoryRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = MaintenanceHistoryRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var refreshed = new List<MaintenanceHistorySummaryResponse>();
        var skipped = new List<MaintenanceHistoryRollupRefreshSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Refreshed, int Skipped)>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0);
            }

            var stats = runStats[candidate.TenantId];
            stats.Candidates++;
            runStats[candidate.TenantId] = stats;

            try
            {
                var summary = await RefreshRollupAsync(
                    candidate.TenantId,
                    candidate.AssetId,
                    candidate.AssetTag,
                    candidate.AssetName,
                    asOf,
                    cancellationToken);
                refreshed.Add(summary);

                stats = runStats[candidate.TenantId];
                stats.Refreshed++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new MaintenanceHistoryRollupRefreshSkip(candidate.AssetId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.MaintenanceHistoryRollupRuns.Add(new MaintenanceHistoryRollupRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                RefreshedCount = stats.Refreshed,
                SkippedCount = stats.Skipped,
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
                "maintainarr.maintenance_history_rollup.batch",
                tenantId,
                WorkerActorUserId,
                "maintenance_history_rollup_run",
                $"{refreshed.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessMaintenanceHistoryRollupsResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            refreshed.Count,
            skipped.Count,
            refreshed,
            skipped);
    }

    public async Task<MaintenanceHistoryRollupRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = MaintenanceHistoryRules.NormalizeRunListLimit(limit);
        var runs = await db.MaintenanceHistoryRollupRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new MaintenanceHistoryRollupRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.RefreshedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new MaintenanceHistoryRollupRunsResponse(runs);
    }

    private async Task<MaintenanceHistorySummaryResponse> RefreshRollupAsync(
        Guid tenantId,
        Guid assetId,
        string assetTag,
        string assetName,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var entries = await MaintenanceHistoryTimelineBuilder.BuildTimelineEntriesAsync(
            db,
            tenantId,
            assetId,
            cancellationToken);
        var counts = MaintenanceHistoryRules.AggregateCategoryCounts(entries);
        var lastEventAt = entries.Count == 0
            ? (DateTimeOffset?)null
            : entries.Max(x => x.OccurredAt);

        var existing = await db.MaintenanceHistoryRollups
            .Include(x => x.Events)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.AssetId == assetId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new MaintenanceHistoryRollup
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                CreatedAt = now,
            };
            db.MaintenanceHistoryRollups.Add(existing);
        }
        else if (existing.Events.Count > 0)
        {
            db.MaintenanceHistoryEvents.RemoveRange(existing.Events);
            existing.Events.Clear();
        }

        existing.AssetTag = assetTag;
        existing.AssetName = assetName;
        existing.EventCount = entries.Count;
        existing.InspectionCount = counts.InspectionCount;
        existing.DefectCount = counts.DefectCount;
        existing.WorkOrderCount = counts.WorkOrderCount;
        existing.PmCount = counts.PmCount;
        existing.LastEventAt = lastEventAt;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        foreach (var entry in entries)
        {
            existing.Events.Add(new MaintenanceHistoryEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = assetId,
                RollupId = existing.Id,
                EntryId = entry.EntryId,
                Category = entry.Category,
                EventType = entry.EventType,
                Title = entry.Title,
                Detail = entry.Detail,
                OccurredAt = entry.OccurredAt,
                ActorUserId = entry.ActorUserId,
                SourceEntityType = entry.SourceEntityType,
                SourceEntityId = entry.SourceEntityId,
                RelatedEntityId = entry.RelatedEntityId,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return MapSummary(existing, isMaterialized: true);
    }

    private async Task<IReadOnlyList<PendingAssetCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantMaintenanceHistoryRollupSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var assets = await db.Assets.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.AssetTag)
            .Select(x => new PendingAssetCandidate(
                x.Id,
                x.TenantId,
                x.AssetTag,
                x.Name,
                null))
            .ToListAsync(cancellationToken);

        var rollupLookup = await db.MaintenanceHistoryRollups.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => (x.TenantId, x.AssetId), x => x.ComputedAt, cancellationToken);

        var pending = new List<PendingAssetCandidate>();
        foreach (var asset in assets)
        {
            rollupLookup.TryGetValue((asset.TenantId, asset.AssetId), out var computedAt);
            if (!MaintenanceHistoryRules.IsStale(computedAt, asOfUtc, stalenessHours))
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

    internal static MaintenanceHistorySummaryResponse MapSummary(
        MaintenanceHistoryRollup rollup,
        bool isMaterialized) =>
        new(
            rollup.AssetId,
            rollup.AssetTag,
            rollup.AssetName,
            rollup.EventCount,
            rollup.InspectionCount,
            rollup.DefectCount,
            rollup.WorkOrderCount,
            rollup.PmCount,
            rollup.LastEventAt,
            rollup.ComputedAt,
            isMaterialized);

    private sealed record PendingAssetCandidate(
        Guid AssetId,
        Guid TenantId,
        string AssetTag,
        string AssetName,
        DateTimeOffset? LastComputedAt);
}
