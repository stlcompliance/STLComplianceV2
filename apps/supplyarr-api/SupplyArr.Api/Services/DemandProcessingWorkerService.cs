using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class DemandProcessingWorkerService(
    SupplyArrDbContext db,
    DemandProcessingSettingsService settingsService,
    MaintainArrDemandIntakeService demandIntake,
    ProcurementNotificationEnqueueService notificationEnqueue,
    ISupplyArrAuditService audit)
{
    public const string ProcessDemandProcessingActionScope = "supplyarr.demand.process";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fe");

    public async Task<PendingDemandProcessingResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = DemandProcessingRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = DemandProcessingRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingDemandProcessingItem(
                x.DemandRefId,
                x.MaintainarrWorkOrderNumber,
                x.Title,
                x.ReceivedAt,
                x.LastProcessedAt,
                x.LastProcessingOutcome))
            .ToList();

        return new PendingDemandProcessingResponse(asOf, normalizedStalenessHours, normalizedBatchSize, items);
    }

    public async Task<ProcessDemandProcessingResponse> ProcessBatchAsync(
        ProcessDemandProcessingRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = DemandProcessingRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = DemandProcessingRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var processed = new List<DemandProcessingResult>();
        var skipped = new List<DemandProcessingSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Processed, int PrDrafts, int Skipped)>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0, 0);
            }

            var stats = runStats[candidate.TenantId];
            stats.Candidates++;
            runStats[candidate.TenantId] = stats;

            try
            {
                var result = await ProcessDemandRefAsync(candidate, asOf, cancellationToken);
                processed.Add(result);

                stats = runStats[candidate.TenantId];
                stats.Processed++;
                if (result.PurchaseRequestId.HasValue)
                {
                    stats.PrDrafts++;
                }

                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new DemandProcessingSkip(candidate.DemandRefId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.DemandProcessingRuns.Add(new DemandProcessingRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                ProcessedCount = stats.Processed,
                PrDraftsCreatedCount = stats.PrDrafts,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && processed.Count > 0)
        {
            await audit.WriteAsync(
                "supplyarr.demand_processing.batch",
                tenantId,
                WorkerActorUserId,
                "demand_processing_run",
                $"{processed.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessDemandProcessingResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            processed.Count,
            processed.Count(x => x.PurchaseRequestId.HasValue),
            skipped.Count,
            processed,
            skipped);
    }

    public async Task<DemandProcessingRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = DemandProcessingRules.NormalizeRunListLimit(limit);
        var runs = await db.DemandProcessingRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new DemandProcessingRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.ProcessedCount,
                x.PrDraftsCreatedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DemandProcessingRunsResponse(runs);
    }

    private async Task<DemandProcessingResult> ProcessDemandRefAsync(
        PendingDemandProcessingCandidate candidate,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(candidate.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Demand processing settings are not configured for this tenant.");

        var demandRef = await db.MaintainArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.Id == candidate.DemandRefId,
                cancellationToken)
            ?? throw new InvalidOperationException("Demand reference was not found.");

        if (demandRef.PurchaseRequestId.HasValue)
        {
            throw new InvalidOperationException("Demand reference already has a purchase request.");
        }

        if (!string.Equals(demandRef.Status, MaintainArrDemandRefStatuses.Received, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Demand reference is not in received status.");
        }

        var stockTotals = await LoadStockTotalsAsync(candidate.TenantId, cancellationToken);
        var lineSummaries = BuildLineSummaries(demandRef.Lines, stockTotals);
        var linesTotalCount = demandRef.Lines.Count;
        var linesCatalogCount = lineSummaries.Count(x => x.PartId.HasValue);
        var linesShortCount = lineSummaries.Count(x => x.PartId.HasValue && x.IsShort);

        var (outcome, recommendedAction) = DemandProcessingRules.ResolveOutcome(
            linesTotalCount,
            linesCatalogCount,
            linesShortCount);

        Guid? purchaseRequestId = null;
        Guid? notificationDispatchId = null;
        var processingMessage = DemandProcessingRules.BuildProcessingMessage(
            outcome,
            linesShortCount,
            linesCatalogCount,
            linesTotalCount);

        if (settings.AutoCreatePrDraftWhenShort
            && string.Equals(outcome, DemandProcessingOutcomes.StockShort, StringComparison.OrdinalIgnoreCase)
            && linesCatalogCount > 0)
        {
            var draft = await demandIntake.CreatePurchaseRequestFromDemandRefAsync(
                candidate.TenantId,
                WorkerActorUserId,
                candidate.DemandRefId,
                new CreatePurchaseRequestFromDemandRefRequest(
                    $"auto-demand-{demandRef.MaintainarrWorkOrderNumber}-{demandRef.MaintainarrPublicationId:N}".ToLowerInvariant(),
                    $"MaintainArr WO {demandRef.MaintainarrWorkOrderNumber} (auto)",
                    demandRef.Notes),
                cancellationToken);

            purchaseRequestId = draft.PurchaseRequestId;
            outcome = DemandProcessingOutcomes.PrDrafted;
            recommendedAction = DemandProcessingRecommendedActions.PrAutoCreated;
            processingMessage = DemandProcessingRules.BuildProcessingMessage(
                outcome,
                linesShortCount,
                linesCatalogCount,
                linesTotalCount);

            if (settings.NotifyOnPrDraftCreated)
            {
                notificationDispatchId = await notificationEnqueue.TryEnqueueRepeatableAsync(
                    candidate.TenantId,
                    ProcurementNotificationEventKinds.MaintainArrDemandPrDrafted,
                    null,
                    "demand_ref",
                    candidate.DemandRefId,
                    cancellationToken);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var state = await db.DemandProcessingStates
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.DemandRefId == candidate.DemandRefId,
                cancellationToken);

        if (state is null)
        {
            state = new DemandProcessingState
            {
                Id = Guid.NewGuid(),
                TenantId = candidate.TenantId,
                DemandRefId = candidate.DemandRefId,
                CreatedAt = now,
            };
            db.DemandProcessingStates.Add(state);
        }

        state.MaintainarrWorkOrderNumber = demandRef.MaintainarrWorkOrderNumber;
        state.Title = demandRef.Title;
        state.ProcessingOutcome = outcome;
        state.RecommendedAction = recommendedAction;
        state.LinesTotalCount = linesTotalCount;
        state.LinesCatalogCount = linesCatalogCount;
        state.LinesShortCount = linesShortCount;
        state.PurchaseRequestId = purchaseRequestId ?? demandRef.PurchaseRequestId;
        state.LastProcessingMessage = processingMessage;
        state.DemandReceivedAt = demandRef.ReceivedAt;
        state.LastProcessedAt = asOfUtc;
        state.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return new DemandProcessingResult(
            candidate.DemandRefId,
            demandRef.MaintainarrWorkOrderNumber,
            outcome,
            recommendedAction,
            linesShortCount,
            purchaseRequestId,
            notificationDispatchId);
    }

    private async Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantDemandProcessingSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var settingsByTenant = await db.TenantDemandProcessingSettings
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        var stateLookup = await db.DemandProcessingStates
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.DemandRefId, x => x, cancellationToken);

        var receivedStatus = MaintainArrDemandRefStatuses.Received;
        var demandRefs = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId)
                && x.Status == receivedStatus
                && x.PurchaseRequestId == null)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.MaintainarrWorkOrderNumber,
                x.Title,
                x.ReceivedAt,
            })
            .ToListAsync(cancellationToken);

        var candidates = new List<PendingDemandProcessingCandidate>();

        foreach (var demandRef in demandRefs)
        {
            if (!settingsByTenant.TryGetValue(demandRef.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = DemandProcessingSettingsService.ToSnapshot(settingsEntity);
            stateLookup.TryGetValue(demandRef.Id, out var state);
            var lastProcessedAt = state?.LastProcessedAt;

            if (!DemandProcessingRules.IsDueForProcessing(
                    demandRef.ReceivedAt,
                    lastProcessedAt,
                    settings.MinHoursBeforeProcessing,
                    stalenessHours,
                    asOfUtc))
            {
                continue;
            }

            candidates.Add(new PendingDemandProcessingCandidate(
                demandRef.TenantId,
                demandRef.Id,
                demandRef.MaintainarrWorkOrderNumber,
                demandRef.Title,
                demandRef.ReceivedAt,
                lastProcessedAt,
                state?.ProcessingOutcome));
        }

        return candidates
            .OrderBy(x => x.LastProcessedAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.ReceivedAt)
            .Take(batchSize)
            .ToList();
    }

    private static IReadOnlyList<DemandProcessingLineSummary> BuildLineSummaries(
        IEnumerable<MaintainArrDemandRefLine> lines,
        IReadOnlyDictionary<Guid, (decimal OnHand, decimal Reserved)> stockTotals)
    {
        return lines
            .OrderBy(x => x.LineNumber)
            .Select(line =>
            {
                decimal quantityAvailable = 0;
                var isShort = false;
                if (line.PartId is Guid partId)
                {
                    stockTotals.TryGetValue(partId, out var stock);
                    quantityAvailable = stock.OnHand - stock.Reserved;
                    isShort = quantityAvailable < line.QuantityRequested;
                }

                return new DemandProcessingLineSummary(
                    line.Id,
                    line.LineNumber,
                    line.PartId,
                    line.PartNumber,
                    line.QuantityRequested,
                    quantityAvailable,
                    isShort);
            })
            .ToList();
    }

    private async Task<Dictionary<Guid, (decimal OnHand, decimal Reserved)>> LoadStockTotalsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .GroupBy(x => x.PartId)
            .Select(g => new
            {
                PartId = g.Key,
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved),
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.PartId, x => (x.OnHand, x.Reserved));
    }

    private sealed record PendingDemandProcessingCandidate(
        Guid TenantId,
        Guid DemandRefId,
        string MaintainarrWorkOrderNumber,
        string Title,
        DateTimeOffset ReceivedAt,
        DateTimeOffset? LastProcessedAt,
        string? LastProcessingOutcome);
}
