using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class DemandProcessingWorkerService(
    SupplyArrDbContext db,
    DemandProcessingSettingsService settingsService,
    MaintainArrDemandIntakeService maintainArrDemandIntake,
    RoutArrDemandIntakeService routArrDemandIntake,
    TrainArrDemandIntakeService trainArrDemandIntake,
    StaffArrDemandIntakeService staffArrDemandIntake,
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
                x.DemandRefSource,
                x.SourceRefKey,
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

        if (!settings.IsSourceEnabled(candidate.DemandRefSource))
        {
            throw new InvalidOperationException($"Demand processing is disabled for source {candidate.DemandRefSource}.");
        }

        var snapshot = await LoadDemandRefSnapshotAsync(candidate, cancellationToken)
            ?? throw new InvalidOperationException("Demand reference was not found.");

        if (snapshot.PurchaseRequestId.HasValue)
        {
            throw new InvalidOperationException("Demand reference already has a purchase request.");
        }

        if (!string.Equals(snapshot.Status, snapshot.ReceivedStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Demand reference is not in received status.");
        }

        var stockTotals = await LoadStockTotalsAsync(candidate.TenantId, cancellationToken);
        var lineSummaries = BuildLineSummaries(snapshot.Lines, stockTotals);
        var linesTotalCount = snapshot.Lines.Count;
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
            var draft = await CreateAutoPurchaseRequestDraftAsync(snapshot, cancellationToken);
            purchaseRequestId = draft.PurchaseRequestId;
            outcome = DemandProcessingOutcomes.PrDrafted;
            recommendedAction = DemandProcessingRecommendedActions.PrAutoCreated;
            processingMessage = DemandProcessingRules.BuildProcessingMessage(
                outcome,
                linesShortCount,
                linesCatalogCount,
                linesTotalCount);

            if (settings.NotifyOnPrDraftCreated
                && string.Equals(candidate.DemandRefSource, DemandRefSources.MaintainArr, StringComparison.OrdinalIgnoreCase))
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
                DemandRefSource = candidate.DemandRefSource,
                CreatedAt = now,
            };
            db.DemandProcessingStates.Add(state);
        }

        state.DemandRefSource = candidate.DemandRefSource;
        state.MaintainarrWorkOrderNumber = snapshot.SourceRefKey;
        state.Title = snapshot.Title;
        state.ProcessingOutcome = outcome;
        state.RecommendedAction = recommendedAction;
        state.LinesTotalCount = linesTotalCount;
        state.LinesCatalogCount = linesCatalogCount;
        state.LinesShortCount = linesShortCount;
        state.PurchaseRequestId = purchaseRequestId ?? snapshot.PurchaseRequestId;
        state.LastProcessingMessage = processingMessage;
        state.DemandReceivedAt = snapshot.ReceivedAt;
        state.LastProcessedAt = asOfUtc;
        state.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return new DemandProcessingResult(
            candidate.DemandRefId,
            candidate.DemandRefSource,
            snapshot.SourceRefKey,
            outcome,
            recommendedAction,
            linesShortCount,
            purchaseRequestId,
            notificationDispatchId);
    }

    private async Task<PurchaseRequestResponse> CreateAutoPurchaseRequestDraftAsync(
        DemandRefProcessingSnapshot snapshot,
        CancellationToken cancellationToken) =>
        snapshot.Source switch
        {
            DemandRefSources.MaintainArr => await maintainArrDemandIntake.CreatePurchaseRequestFromDemandRefAsync(
                snapshot.TenantId,
                WorkerActorUserId,
                snapshot.DemandRefId,
                new CreatePurchaseRequestFromDemandRefRequest(
                    snapshot.AutoRequestKey,
                    snapshot.AutoTitle,
                    snapshot.Notes),
                cancellationToken),
            DemandRefSources.RoutArr => await routArrDemandIntake.CreatePurchaseRequestFromDemandRefAsync(
                snapshot.TenantId,
                WorkerActorUserId,
                snapshot.DemandRefId,
                new CreatePurchaseRequestFromRoutarrDemandRefRequest(
                    snapshot.AutoRequestKey,
                    snapshot.AutoTitle,
                    snapshot.Notes),
                cancellationToken),
            DemandRefSources.TrainArr => await trainArrDemandIntake.CreatePurchaseRequestFromDemandRefAsync(
                snapshot.TenantId,
                WorkerActorUserId,
                snapshot.DemandRefId,
                new CreatePurchaseRequestFromTrainarrDemandRefRequest(
                    snapshot.AutoRequestKey,
                    snapshot.AutoTitle,
                    snapshot.Notes),
                cancellationToken),
            DemandRefSources.StaffArr => await staffArrDemandIntake.CreatePurchaseRequestFromDemandRefAsync(
                snapshot.TenantId,
                WorkerActorUserId,
                snapshot.DemandRefId,
                new CreatePurchaseRequestFromStaffarrDemandRefRequest(
                    snapshot.AutoRequestKey,
                    snapshot.AutoTitle,
                    snapshot.Notes),
                cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported demand reference source: {snapshot.Source}"),
        };

    private async Task<DemandRefProcessingSnapshot?> LoadDemandRefSnapshotAsync(
        PendingDemandProcessingCandidate candidate,
        CancellationToken cancellationToken)
    {
        return candidate.DemandRefSource switch
        {
            DemandRefSources.MaintainArr => await LoadMaintainarrSnapshotAsync(candidate, cancellationToken),
            DemandRefSources.RoutArr => await LoadRoutarrSnapshotAsync(candidate, cancellationToken),
            DemandRefSources.TrainArr => await LoadTrainarrSnapshotAsync(candidate, cancellationToken),
            DemandRefSources.StaffArr => await LoadStaffarrSnapshotAsync(candidate, cancellationToken),
            _ => null,
        };
    }

    private async Task<DemandRefProcessingSnapshot?> LoadMaintainarrSnapshotAsync(
        PendingDemandProcessingCandidate candidate,
        CancellationToken cancellationToken)
    {
        var entity = await db.MaintainArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.Id == candidate.DemandRefId,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new DemandRefProcessingSnapshot(
            DemandRefSources.MaintainArr,
            entity.TenantId,
            entity.Id,
            entity.MaintainarrWorkOrderNumber,
            entity.Title,
            entity.Notes,
            entity.Status,
            MaintainArrDemandRefStatuses.Received,
            entity.PurchaseRequestId,
            entity.ReceivedAt,
            $"auto-demand-{entity.MaintainarrWorkOrderNumber}-{entity.MaintainarrPublicationId:N}".ToLowerInvariant(),
            $"MaintainArr WO {entity.MaintainarrWorkOrderNumber} (auto)",
            MapLines(entity.Lines.Select(x => (x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested))));
    }

    private async Task<DemandRefProcessingSnapshot?> LoadRoutarrSnapshotAsync(
        PendingDemandProcessingCandidate candidate,
        CancellationToken cancellationToken)
    {
        var entity = await db.RoutArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.Id == candidate.DemandRefId,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new DemandRefProcessingSnapshot(
            DemandRefSources.RoutArr,
            entity.TenantId,
            entity.Id,
            entity.RoutarrTripNumber,
            entity.Title,
            entity.Notes,
            entity.Status,
            RoutArrDemandRefStatuses.Received,
            entity.PurchaseRequestId,
            entity.ReceivedAt,
            $"routarr-{entity.RoutarrTripNumber}-{entity.RoutarrPublicationId:N}".ToLowerInvariant(),
            $"RoutArr trip {entity.RoutarrTripNumber} (auto)",
            MapLines(entity.Lines.Select(x => (x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested))));
    }

    private async Task<DemandRefProcessingSnapshot?> LoadTrainarrSnapshotAsync(
        PendingDemandProcessingCandidate candidate,
        CancellationToken cancellationToken)
    {
        var entity = await db.TrainArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.Id == candidate.DemandRefId,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new DemandRefProcessingSnapshot(
            DemandRefSources.TrainArr,
            entity.TenantId,
            entity.Id,
            entity.TrainarrAssignmentRefKey,
            entity.Title,
            entity.Notes,
            entity.Status,
            TrainArrDemandRefStatuses.Received,
            entity.PurchaseRequestId,
            entity.ReceivedAt,
            $"trainarr-{entity.TrainarrAssignmentRefKey}-{entity.TrainarrPublicationId:N}".ToLowerInvariant(),
            $"TrainArr assignment {entity.TrainarrAssignmentRefKey} (auto)",
            MapLines(entity.Lines.Select(x => (x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested))));
    }

    private async Task<DemandRefProcessingSnapshot?> LoadStaffarrSnapshotAsync(
        PendingDemandProcessingCandidate candidate,
        CancellationToken cancellationToken)
    {
        var entity = await db.StaffArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId && x.Id == candidate.DemandRefId,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new DemandRefProcessingSnapshot(
            DemandRefSources.StaffArr,
            entity.TenantId,
            entity.Id,
            entity.StaffarrIncidentTitle,
            entity.Title,
            entity.Notes,
            entity.Status,
            StaffArrDemandRefStatuses.Received,
            entity.PurchaseRequestId,
            entity.ReceivedAt,
            $"staffarr-{entity.StaffarrIncidentId:N}-{entity.StaffarrPublicationId:N}".ToLowerInvariant(),
            $"StaffArr incident {entity.StaffarrIncidentTitle} (auto)",
            MapLines(entity.Lines.Select(x => (x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested))));
    }

    private static IReadOnlyList<DemandRefProcessingLine> MapLines(
        IEnumerable<(Guid Id, int LineNumber, Guid? PartId, string PartNumber, decimal QuantityRequested)> lines) =>
        lines
            .OrderBy(x => x.LineNumber)
            .Select(x => new DemandRefProcessingLine(x.Id, x.LineNumber, x.PartId, x.PartNumber, x.QuantityRequested))
            .ToList();

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

        var candidates = new List<PendingDemandProcessingCandidate>();

        if (enabledTenantIds.Any(t => settingsByTenant[t].ProcessMaintainarrDemandRefs))
        {
            candidates.AddRange(await LoadMaintainarrPendingAsync(
                enabledTenantIds,
                settingsByTenant,
                stateLookup,
                asOfUtc,
                stalenessHours,
                cancellationToken));
        }

        if (enabledTenantIds.Any(t => settingsByTenant[t].ProcessRoutarrDemandRefs))
        {
            candidates.AddRange(await LoadRoutarrPendingAsync(
                enabledTenantIds,
                settingsByTenant,
                stateLookup,
                asOfUtc,
                stalenessHours,
                cancellationToken));
        }

        if (enabledTenantIds.Any(t => settingsByTenant[t].ProcessTrainarrDemandRefs))
        {
            candidates.AddRange(await LoadTrainarrPendingAsync(
                enabledTenantIds,
                settingsByTenant,
                stateLookup,
                asOfUtc,
                stalenessHours,
                cancellationToken));
        }

        if (enabledTenantIds.Any(t => settingsByTenant[t].ProcessStaffarrDemandRefs))
        {
            candidates.AddRange(await LoadStaffarrPendingAsync(
                enabledTenantIds,
                settingsByTenant,
                stateLookup,
                asOfUtc,
                stalenessHours,
                cancellationToken));
        }

        return candidates
            .OrderBy(x => x.LastProcessedAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.ReceivedAt)
            .Take(batchSize)
            .ToList();
    }

    private Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadMaintainarrPendingAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken) =>
        LoadMaintainarrPendingCoreAsync(enabledTenantIds, settingsByTenant, stateLookup, asOfUtc, stalenessHours, cancellationToken);

    private async Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadMaintainarrPendingCoreAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var tenantFilter = enabledTenantIds
            .Where(t => settingsByTenant[t].ProcessMaintainarrDemandRefs)
            .ToList();

        var demandRefs = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Where(x => tenantFilter.Contains(x.TenantId)
                && x.Status == MaintainArrDemandRefStatuses.Received
                && x.PurchaseRequestId == null)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                SourceRefKey = x.MaintainarrWorkOrderNumber,
                x.Title,
                x.ReceivedAt,
            })
            .ToListAsync(cancellationToken);

        return CollectDueCandidates(
            demandRefs.Select(x => (x.TenantId, x.Id, DemandRefSources.MaintainArr, x.SourceRefKey, x.Title, x.ReceivedAt)),
            settingsByTenant,
            stateLookup,
            asOfUtc,
            stalenessHours);
    }

    private Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadRoutarrPendingAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken) =>
        LoadRoutarrPendingCoreAsync(enabledTenantIds, settingsByTenant, stateLookup, asOfUtc, stalenessHours, cancellationToken);

    private async Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadRoutarrPendingCoreAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var tenantFilter = enabledTenantIds
            .Where(t => settingsByTenant[t].ProcessRoutarrDemandRefs)
            .ToList();

        var demandRefs = await db.RoutArrDemandRefs
            .AsNoTracking()
            .Where(x => tenantFilter.Contains(x.TenantId)
                && x.Status == RoutArrDemandRefStatuses.Received
                && x.PurchaseRequestId == null)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                SourceRefKey = x.RoutarrTripNumber,
                x.Title,
                x.ReceivedAt,
            })
            .ToListAsync(cancellationToken);

        return CollectDueCandidates(
            demandRefs.Select(x => (x.TenantId, x.Id, DemandRefSources.RoutArr, x.SourceRefKey, x.Title, x.ReceivedAt)),
            settingsByTenant,
            stateLookup,
            asOfUtc,
            stalenessHours);
    }

    private Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadTrainarrPendingAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken) =>
        LoadTrainarrPendingCoreAsync(enabledTenantIds, settingsByTenant, stateLookup, asOfUtc, stalenessHours, cancellationToken);

    private async Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadTrainarrPendingCoreAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var tenantFilter = enabledTenantIds
            .Where(t => settingsByTenant[t].ProcessTrainarrDemandRefs)
            .ToList();

        var demandRefs = await db.TrainArrDemandRefs
            .AsNoTracking()
            .Where(x => tenantFilter.Contains(x.TenantId)
                && x.Status == TrainArrDemandRefStatuses.Received
                && x.PurchaseRequestId == null)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                SourceRefKey = x.TrainarrAssignmentRefKey,
                x.Title,
                x.ReceivedAt,
            })
            .ToListAsync(cancellationToken);

        return CollectDueCandidates(
            demandRefs.Select(x => (x.TenantId, x.Id, DemandRefSources.TrainArr, x.SourceRefKey, x.Title, x.ReceivedAt)),
            settingsByTenant,
            stateLookup,
            asOfUtc,
            stalenessHours);
    }

    private Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadStaffarrPendingAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken) =>
        LoadStaffarrPendingCoreAsync(enabledTenantIds, settingsByTenant, stateLookup, asOfUtc, stalenessHours, cancellationToken);

    private async Task<IReadOnlyList<PendingDemandProcessingCandidate>> LoadStaffarrPendingCoreAsync(
        IReadOnlyList<Guid> enabledTenantIds,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var tenantFilter = enabledTenantIds
            .Where(t => settingsByTenant[t].ProcessStaffarrDemandRefs)
            .ToList();

        var demandRefs = await db.StaffArrDemandRefs
            .AsNoTracking()
            .Where(x => tenantFilter.Contains(x.TenantId)
                && x.Status == StaffArrDemandRefStatuses.Received
                && x.PurchaseRequestId == null)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                SourceRefKey = x.StaffarrIncidentTitle,
                x.Title,
                x.ReceivedAt,
            })
            .ToListAsync(cancellationToken);

        return CollectDueCandidates(
            demandRefs.Select(x => (x.TenantId, x.Id, DemandRefSources.StaffArr, x.SourceRefKey, x.Title, x.ReceivedAt)),
            settingsByTenant,
            stateLookup,
            asOfUtc,
            stalenessHours);
    }

    private static IReadOnlyList<PendingDemandProcessingCandidate> CollectDueCandidates(
        IEnumerable<(Guid TenantId, Guid Id, string Source, string SourceRefKey, string Title, DateTimeOffset ReceivedAt)> demandRefs,
        IReadOnlyDictionary<Guid, TenantDemandProcessingSettings> settingsByTenant,
        IReadOnlyDictionary<Guid, DemandProcessingState> stateLookup,
        DateTimeOffset asOfUtc,
        int stalenessHours)
    {
        var candidates = new List<PendingDemandProcessingCandidate>();

        foreach (var demandRef in demandRefs)
        {
            if (!settingsByTenant.TryGetValue(demandRef.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = DemandProcessingSettingsService.ToSnapshot(settingsEntity);
            if (!settings.IsSourceEnabled(demandRef.Source))
            {
                continue;
            }

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
                demandRef.Source,
                demandRef.SourceRefKey,
                demandRef.Title,
                demandRef.ReceivedAt,
                lastProcessedAt,
                state?.ProcessingOutcome));
        }

        return candidates;
    }

    private static IReadOnlyList<DemandProcessingLineSummary> BuildLineSummaries(
        IEnumerable<DemandRefProcessingLine> lines,
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
                    line.LineId,
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
        string DemandRefSource,
        string SourceRefKey,
        string Title,
        DateTimeOffset ReceivedAt,
        DateTimeOffset? LastProcessedAt,
        string? LastProcessingOutcome);

    private sealed record DemandRefProcessingLine(
        Guid LineId,
        int LineNumber,
        Guid? PartId,
        string PartNumber,
        decimal QuantityRequested);

    private sealed record DemandRefProcessingSnapshot(
        string Source,
        Guid TenantId,
        Guid DemandRefId,
        string SourceRefKey,
        string Title,
        string Notes,
        string Status,
        string ReceivedStatus,
        Guid? PurchaseRequestId,
        DateTimeOffset ReceivedAt,
        string AutoRequestKey,
        string AutoTitle,
        IReadOnlyList<DemandRefProcessingLine> Lines);
}
