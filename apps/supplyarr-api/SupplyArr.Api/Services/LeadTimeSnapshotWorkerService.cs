using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class LeadTimeSnapshotWorkerService(
    SupplyArrDbContext db,
    LeadTimeSnapshotService leadTimeSnapshots,
    ISupplyArrAuditService audit)
{
    public const string ProcessLeadTimeSnapshotCapturesActionScope = "supplyarr.leadtime.snapshots.capture";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fa");

    public async Task<PendingLeadTimeSnapshotCapturesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = LeadTimeSnapshotCaptureRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = LeadTimeSnapshotCaptureRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingLeadTimeSnapshotCaptureItem(
                x.PartVendorLinkId,
                x.PartId,
                x.PartKey,
                x.PartDisplayName,
                x.SupplierId,
                x.SupplierKey,
                x.SupplierDisplayName,
                x.VendorPartyId,
                x.VendorPartyKey,
                x.VendorDisplayName,
                x.VendorPartNumber,
                x.CatalogLeadTimeDays,
                x.CurrentLeadTimeDays,
                x.LastCapturedAt))
            .ToList();

        return new PendingLeadTimeSnapshotCapturesResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessLeadTimeSnapshotCapturesResponse> ProcessBatchAsync(
        ProcessLeadTimeSnapshotCapturesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = LeadTimeSnapshotCaptureRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = LeadTimeSnapshotCaptureRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var captured = new List<LeadTimeSnapshotResponse>();
        var skipped = new List<LeadTimeSnapshotCaptureSkip>();
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
                var snapshot = await leadTimeSnapshots.CreateWorkerCaptureAsync(
                    candidate.TenantId,
                    WorkerActorUserId,
                    candidate.PartVendorLinkId,
                    candidate.CatalogLeadTimeDays,
                    asOf,
                    cancellationToken);

                await UpsertCaptureStateAsync(
                    candidate.TenantId,
                    candidate.PartVendorLinkId,
                    candidate.CatalogLeadTimeDays,
                    snapshot.LeadTimeSnapshotId,
                    asOf,
                    cancellationToken);

                captured.Add(snapshot);

                stats = runStats[candidate.TenantId];
                stats.Captured++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new LeadTimeSnapshotCaptureSkip(candidate.PartVendorLinkId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.LeadTimeSnapshotRuns.Add(new LeadTimeSnapshotRun
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
                "supplyarr.lead_time_snapshot_capture.batch",
                tenantId,
                WorkerActorUserId,
                "lead_time_snapshot_run",
                $"{captured.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessLeadTimeSnapshotCapturesResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            captured.Count,
            skipped.Count,
            captured,
            skipped);
    }

    public async Task<LeadTimeSnapshotRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = LeadTimeSnapshotCaptureRules.NormalizeRunListLimit(limit);
        var runs = await db.LeadTimeSnapshotRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new LeadTimeSnapshotRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.CapturedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new LeadTimeSnapshotRunsResponse(runs);
    }

    private async Task UpsertCaptureStateAsync(
        Guid tenantId,
        Guid partVendorLinkId,
        int catalogLeadTimeDays,
        Guid leadTimeSnapshotId,
        DateTimeOffset capturedAt,
        CancellationToken cancellationToken)
    {
        var state = await db.PartVendorLeadTimeCaptureStates
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartVendorLinkId == partVendorLinkId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (state is null)
        {
            state = new PartVendorLeadTimeCaptureState
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartVendorLinkId = partVendorLinkId,
                CreatedAt = now,
            };
            db.PartVendorLeadTimeCaptureStates.Add(state);
        }

        state.LastCapturedLeadTimeDays = catalogLeadTimeDays;
        state.LastLeadTimeSnapshotId = leadTimeSnapshotId;
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
        var enabledTenantIds = await db.TenantLeadTimeSnapshotSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var links = await db.PartVendorLinks
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.ExternalParty)
            .Where(x =>
                enabledTenantIds.Contains(x.TenantId)
                && x.CatalogLeadTimeDays != null
                && x.CatalogLeadTimeDays >= 0)
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.Part.PartKey)
            .ToListAsync(cancellationToken);

        var linkIds = links.Select(x => x.Id).ToList();
        var captureStates = await db.PartVendorLeadTimeCaptureStates
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && linkIds.Contains(x.PartVendorLinkId))
            .ToDictionaryAsync(x => (x.TenantId, x.PartVendorLinkId), cancellationToken);

        var currentSnapshots = await LoadCurrentSnapshotsAsync(enabledTenantIds, linkIds, asOfUtc, cancellationToken);

        var pending = new List<PendingLinkCandidate>();
        foreach (var link in links)
        {
            captureStates.TryGetValue((link.TenantId, link.Id), out var captureState);
            currentSnapshots.TryGetValue((link.TenantId, link.Id), out var currentSnapshot);

            var needsCapture = LeadTimeSnapshotCaptureRules.NeedsCapture(
                link.CatalogLeadTimeDays,
                currentSnapshot?.LeadTimeDays);

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
                link.ExternalPartyId,
                link.ExternalParty.PartyKey,
                link.ExternalParty.DisplayName,
                link.ExternalPartyId,
                link.ExternalParty.PartyKey,
                link.ExternalParty.DisplayName,
                link.VendorPartNumber,
                link.CatalogLeadTimeDays!.Value,
                currentSnapshot?.LeadTimeDays,
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

    private async Task<Dictionary<(Guid TenantId, Guid PartVendorLinkId), CurrentSnapshotValues>> LoadCurrentSnapshotsAsync(
        IReadOnlyList<Guid> tenantIds,
        IReadOnlyList<Guid> linkIds,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        if (linkIds.Count == 0)
        {
            return [];
        }

        var snapshots = await db.PartVendorLeadTimeSnapshots
            .AsNoTracking()
            .Where(x =>
                tenantIds.Contains(x.TenantId)
                && linkIds.Contains(x.PartVendorLinkId)
                && x.EffectiveFrom <= asOfUtc
                && (x.EffectiveTo == null || x.EffectiveTo > asOfUtc))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var lookup = new Dictionary<(Guid TenantId, Guid PartVendorLinkId), CurrentSnapshotValues>();
        foreach (var snapshot in snapshots)
        {
            var key = (snapshot.TenantId, snapshot.PartVendorLinkId);
            if (!lookup.ContainsKey(key))
            {
                lookup[key] = new CurrentSnapshotValues(snapshot.LeadTimeDays);
            }
        }

        return lookup;
    }

    private sealed record CurrentSnapshotValues(int LeadTimeDays);

    private sealed record PendingLinkCandidate(
        Guid PartVendorLinkId,
        Guid TenantId,
        Guid PartId,
        string PartKey,
        string PartDisplayName,
        Guid SupplierId,
        string SupplierKey,
        string SupplierDisplayName,
        Guid VendorPartyId,
        string VendorPartyKey,
        string VendorDisplayName,
        string VendorPartNumber,
        int CatalogLeadTimeDays,
        int? CurrentLeadTimeDays,
        DateTimeOffset? LastCapturedAt);
}
