using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class PriceSnapshotWorkerService(
    SupplyArrDbContext db,
    PricingSnapshotService pricingSnapshots,
    ISupplyArrAuditService audit)
{
    public const string ProcessPriceSnapshotCapturesActionScope = "supplyarr.pricing.snapshots.capture";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f9");

    public async Task<PendingPriceSnapshotCapturesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = PriceSnapshotCaptureRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = PriceSnapshotCaptureRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingPriceSnapshotCaptureItem(
                x.PartSupplierLinkId,
                x.PartId,
                x.PartKey,
                x.PartDisplayName,
                x.SupplierId,
                x.SupplierKey,
                x.SupplierDisplayName,
                x.SupplierPartNumber,
                x.CatalogUnitPrice,
                x.CatalogCurrencyCode,
                x.CatalogMinimumOrderQuantity,
                x.CurrentUnitPrice,
                x.CurrentCurrencyCode,
                x.LastCapturedAt))
            .ToList();

        return new PendingPriceSnapshotCapturesResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessPriceSnapshotCapturesResponse> ProcessBatchAsync(
        ProcessPriceSnapshotCapturesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = PriceSnapshotCaptureRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = PriceSnapshotCaptureRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var captured = new List<PricingSnapshotResponse>();
        var skipped = new List<PriceSnapshotCaptureSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Captured, int Skipped)>();

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
                var snapshot = await pricingSnapshots.CreateWorkerCaptureAsync(
                    candidate.TenantId,
                    WorkerActorUserId,
                    candidate.PartSupplierLinkId,
                    candidate.CatalogUnitPrice,
                    candidate.CatalogCurrencyCode,
                    candidate.CatalogMinimumOrderQuantity,
                    asOf,
                    cancellationToken);

                await UpsertCaptureStateAsync(
                    candidate.TenantId,
                    candidate.PartSupplierLinkId,
                    candidate.CatalogUnitPrice,
                    candidate.CatalogCurrencyCode,
                    candidate.CatalogMinimumOrderQuantity,
                    snapshot.PricingSnapshotId,
                    asOf,
                    cancellationToken);

                captured.Add(snapshot);

                stats = runStats[candidate.TenantId];
                stats.Captured++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PriceSnapshotCaptureSkip(candidate.PartSupplierLinkId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.PriceSnapshotRuns.Add(new PriceSnapshotRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                CapturedCount = stats.Captured,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && captured.Count > 0)
        {
            await audit.WriteAsync(
                "supplyarr.price_snapshot_capture.batch",
                tenantId,
                WorkerActorUserId,
                "price_snapshot_run",
                $"{captured.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessPriceSnapshotCapturesResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            captured.Count,
            skipped.Count,
            captured,
            skipped);
    }

    public async Task<PriceSnapshotRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = PriceSnapshotCaptureRules.NormalizeRunListLimit(limit);
        var runs = await db.PriceSnapshotRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new PriceSnapshotRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.CapturedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PriceSnapshotRunsResponse(runs);
    }

    private async Task UpsertCaptureStateAsync(
        Guid tenantId,
        Guid partSupplierLinkId,
        decimal catalogUnitPrice,
        string catalogCurrencyCode,
        decimal? catalogMinimumOrderQuantity,
        Guid pricingSnapshotId,
        DateTimeOffset capturedAt,
        CancellationToken cancellationToken)
    {
        var state = await db.PartSupplierPriceCaptureStates
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartSupplierLinkId == partSupplierLinkId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (state is null)
        {
            state = new PartSupplierPriceCaptureState
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartSupplierLinkId = partSupplierLinkId,
                CreatedAt = now,
            };
            db.PartSupplierPriceCaptureStates.Add(state);
        }

        state.LastCapturedUnitPrice = catalogUnitPrice;
        state.LastCapturedCurrencyCode = catalogCurrencyCode;
        state.LastCapturedMinimumOrderQuantity = catalogMinimumOrderQuantity;
        state.LastPricingSnapshotId = pricingSnapshotId;
        state.LastCapturedAt = capturedAt;
        state.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<PendingLinkCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantPriceSnapshotSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var links = await db.PartSupplierLinks
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.Supplier)
            .Where(x =>
                enabledTenantIds.Contains(x.TenantId)
                && x.CatalogUnitPrice != null
                && x.CatalogUnitPrice > 0)
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.Part.PartKey)
            .ToListAsync(cancellationToken);

        var linkIds = links.Select(x => x.Id).ToList();
        var captureStates = await db.PartSupplierPriceCaptureStates
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && linkIds.Contains(x.PartSupplierLinkId))
            .ToDictionaryAsync(x => (x.TenantId, x.PartSupplierLinkId), cancellationToken);

        var currentSnapshots = await LoadCurrentSnapshotsAsync(enabledTenantIds, linkIds, asOfUtc, cancellationToken);

        var pending = new List<PendingLinkCandidate>();
        foreach (var link in links)
        {
            captureStates.TryGetValue((link.TenantId, link.Id), out var captureState);
            currentSnapshots.TryGetValue((link.TenantId, link.Id), out var currentSnapshot);

            var needsCapture = PriceSnapshotCaptureRules.NeedsCapture(
                link.CatalogUnitPrice,
                link.CatalogCurrencyCode,
                link.CatalogMinimumOrderQuantity,
                currentSnapshot?.UnitPrice,
                currentSnapshot?.CurrencyCode,
                currentSnapshot?.MinimumOrderQuantity);

            if (!needsCapture)
            {
                continue;
            }

            pending.Add(new PendingLinkCandidate(
                link.Id,
                link.TenantId,
                link.PartId,
                link.Part.PartKey,
                link.Part.DisplayName,
                link.SupplierId,
                link.Supplier.SupplierKey,
                link.Supplier.DisplayName,
                link.SupplierPartNumber,
                link.CatalogUnitPrice!.Value,
                PriceSnapshotCaptureRules.NormalizeCurrencyCode(link.CatalogCurrencyCode),
                link.CatalogMinimumOrderQuantity,
                currentSnapshot?.UnitPrice,
                currentSnapshot?.CurrencyCode,
                captureState?.LastCapturedAt));

            if (pending.Count >= batchSize)
            {
                break;
            }
        }

        return pending
            .OrderBy(x => x.LastCapturedAt.HasValue ? 1 : 0)
            .ThenBy(x => x.LastCapturedAt)
            .Take(batchSize)
            .ToList();
    }

    private async Task<Dictionary<(Guid TenantId, Guid PartSupplierLinkId), CurrentSnapshotValues>> LoadCurrentSnapshotsAsync(
        IReadOnlyList<Guid> tenantIds,
        IReadOnlyList<Guid> linkIds,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        if (linkIds.Count == 0)
        {
            return [];
        }

        var snapshots = await db.PartSupplierPricingSnapshots
            .AsNoTracking()
            .Where(x =>
                tenantIds.Contains(x.TenantId)
                && linkIds.Contains(x.PartSupplierLinkId)
                && x.EffectiveFrom <= asOfUtc
                && (x.EffectiveTo == null || x.EffectiveTo > asOfUtc))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var lookup = new Dictionary<(Guid TenantId, Guid PartSupplierLinkId), CurrentSnapshotValues>();
        foreach (var snapshot in snapshots)
        {
            var key = (snapshot.TenantId, snapshot.PartSupplierLinkId);
            if (!lookup.ContainsKey(key))
            {
                lookup[key] = new CurrentSnapshotValues(
                    snapshot.UnitPrice,
                    snapshot.CurrencyCode,
                    snapshot.MinimumOrderQuantity);
            }
        }

        return lookup;
    }

    private sealed record CurrentSnapshotValues(
        decimal UnitPrice,
        string CurrencyCode,
        decimal? MinimumOrderQuantity);

    private sealed record PendingLinkCandidate(
        Guid PartSupplierLinkId,
        Guid TenantId,
        Guid PartId,
        string PartKey,
        string PartDisplayName,
        Guid SupplierId,
        string SupplierKey,
        string SupplierDisplayName,
        string SupplierPartNumber,
        decimal CatalogUnitPrice,
        string CatalogCurrencyCode,
        decimal? CatalogMinimumOrderQuantity,
        decimal? CurrentUnitPrice,
        string? CurrentCurrencyCode,
        DateTimeOffset? LastCapturedAt);
}

