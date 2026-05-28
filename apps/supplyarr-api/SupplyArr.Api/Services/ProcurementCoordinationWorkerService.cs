using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementCoordinationWorkerService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public const string ProcessProcurementCoordinationActionScope = "supplyarr.procurement.coordination";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fc");

    public async Task<PendingProcurementCoordinationResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ProcurementCoordinationRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = ProcurementCoordinationRules.NormalizeStalenessHours(stalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            cancellationToken);

        var items = candidates
            .Select(x => new PendingProcurementCoordinationItem(
                x.SubjectType,
                x.SubjectId,
                x.DocumentKey,
                x.Title,
                x.DocumentStatus,
                x.SourceUpdatedAt,
                x.LastComputedAt))
            .ToList();

        return new PendingProcurementCoordinationResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            items);
    }

    public async Task<ProcessProcurementCoordinationResponse> ProcessBatchAsync(
        ProcessProcurementCoordinationRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ProcurementCoordinationRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = ProcurementCoordinationRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            stalenessHours,
            batchSize,
            cancellationToken);

        var refreshed = new List<ProcurementCoordinationSummaryResponse>();
        var skipped = new List<ProcurementCoordinationRefreshSkip>();
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
                var summary = await RefreshRecordAsync(
                    candidate.TenantId,
                    candidate.SubjectType,
                    candidate.SubjectId,
                    asOf,
                    cancellationToken);
                refreshed.Add(summary);

                stats = runStats[candidate.TenantId];
                stats.Refreshed++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ProcurementCoordinationRefreshSkip(
                    candidate.SubjectType,
                    candidate.SubjectId,
                    ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.ProcurementCoordinationRuns.Add(new ProcurementCoordinationRun
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
                "supplyarr.procurement_coordination.batch",
                tenantId,
                WorkerActorUserId,
                "procurement_coordination_run",
                $"{refreshed.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessProcurementCoordinationResponse(
            asOf,
            batchSize,
            stalenessHours,
            candidates.Count,
            refreshed.Count,
            skipped.Count,
            refreshed,
            skipped);
    }

    public async Task<ProcurementCoordinationRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = ProcurementCoordinationRules.NormalizeRunListLimit(limit);
        var runs = await db.ProcurementCoordinationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new ProcurementCoordinationRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.RefreshedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ProcurementCoordinationRunsResponse(runs);
    }

    public async Task<ProcurementCoordinationSummaryResponse> RefreshRecordAsync(
        Guid tenantId,
        string subjectType,
        Guid subjectId,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        ProcurementCoordinationComputation computation;

        if (string.Equals(subjectType, ProcurementCoordinationSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase))
        {
            var purchaseRequest = await db.PurchaseRequests
                .Include(x => x.Lines)
                .Include(x => x.VendorParty)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new InvalidOperationException($"Purchase request {subjectId} was not found.");

            if (!ProcurementCoordinationRules.IsActivePurchaseRequestStatus(purchaseRequest.Status))
            {
                throw new InvalidOperationException($"Purchase request {subjectId} is not in an active coordination status.");
            }

            var hasOpenPurchaseOrder = await db.PurchaseOrders.AnyAsync(
                x => x.TenantId == tenantId
                    && x.PurchaseRequestId == subjectId
                    && (PurchaseOrderStatuses.Open.Contains(x.Status)
                        || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase)),
                cancellationToken);

            if (string.Equals(purchaseRequest.Status, PurchaseRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase)
                && hasOpenPurchaseOrder)
            {
                throw new InvalidOperationException(
                    $"Purchase request {subjectId} has an open purchase order and should be coordinated at PO level.");
            }

            computation = ProcurementCoordinationBuilder.BuildFromPurchaseRequest(
                purchaseRequest,
                hasOpenPurchaseOrder,
                asOfUtc);
        }
        else if (string.Equals(subjectType, ProcurementCoordinationSubjectTypes.PurchaseOrder, StringComparison.OrdinalIgnoreCase))
        {
            var purchaseOrder = await db.PurchaseOrders
                .Include(x => x.Lines)
                .Include(x => x.VendorParty)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new InvalidOperationException($"Purchase order {subjectId} was not found.");

            if (!ProcurementCoordinationRules.IsActivePurchaseOrderStatus(purchaseOrder.Status))
            {
                throw new InvalidOperationException($"Purchase order {subjectId} is not in an active coordination status.");
            }

            computation = ProcurementCoordinationBuilder.BuildFromPurchaseOrder(purchaseOrder, asOfUtc);
        }
        else
        {
            throw new InvalidOperationException($"Unknown coordination subject type '{subjectType}'.");
        }

        var existing = await db.ProcurementCoordinationRecords
            .Include(x => x.Events)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.SubjectType == subjectType && x.SubjectId == subjectId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new ProcurementCoordinationRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubjectType = subjectType,
                SubjectId = subjectId,
                CreatedAt = now,
            };
            db.ProcurementCoordinationRecords.Add(existing);
        }
        else if (existing.Events.Count > 0)
        {
            db.ProcurementCoordinationEvents.RemoveRange(existing.Events);
            existing.Events.Clear();
        }

        var summary = computation.Summary;
        existing.DocumentKey = summary.DocumentKey;
        existing.Title = summary.Title;
        existing.CoordinationStage = summary.CoordinationStage;
        existing.NextActionRequired = summary.NextActionRequired;
        existing.PurchaseRequestId = summary.PurchaseRequestId;
        existing.PurchaseOrderId = summary.PurchaseOrderId;
        existing.VendorPartyId = summary.VendorPartyId;
        existing.VendorDisplayName = summary.VendorDisplayName;
        existing.DocumentStatus = summary.DocumentStatus;
        existing.LineCount = summary.LineCount;
        existing.QuantityOrdered = summary.QuantityOrdered;
        existing.QuantityReceived = summary.QuantityReceived;
        existing.ReceiptProgressPercent = summary.ReceiptProgressPercent;
        existing.IsTerminal = summary.IsTerminal;
        existing.SourceUpdatedAt = summary.SourceUpdatedAt;
        existing.ComputedAt = asOfUtc;
        existing.UpdatedAt = now;

        foreach (var eventResponse in computation.Events)
        {
            existing.Events.Add(new ProcurementCoordinationEvent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CoordinationRecordId = existing.Id,
                SubjectType = subjectType,
                SubjectId = subjectId,
                EventKind = eventResponse.EventKind,
                Title = eventResponse.Title,
                Detail = eventResponse.Detail,
                OccurredAt = eventResponse.OccurredAt,
                SequenceNumber = eventResponse.SequenceNumber,
                SourceEntityType = eventResponse.SourceEntityType,
                SourceEntityId = eventResponse.SourceEntityId,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return MapSummary(existing, isMaterialized: true);
    }

    internal static ProcurementCoordinationSummaryResponse MapSummary(
        ProcurementCoordinationRecord record,
        bool isMaterialized) =>
        new(
            record.Id,
            record.SubjectType,
            record.SubjectId,
            record.DocumentKey,
            record.Title,
            record.CoordinationStage,
            record.NextActionRequired,
            record.PurchaseRequestId,
            record.PurchaseOrderId,
            record.VendorPartyId,
            record.VendorDisplayName,
            record.DocumentStatus,
            record.LineCount,
            record.QuantityOrdered,
            record.QuantityReceived,
            record.ReceiptProgressPercent,
            record.IsTerminal,
            record.SourceUpdatedAt,
            record.ComputedAt,
            isMaterialized);

    internal static ProcurementCoordinationEventResponse MapEvent(ProcurementCoordinationEvent entity) =>
        new(
            entity.EventKind,
            entity.Title,
            entity.Detail,
            entity.OccurredAt,
            entity.SequenceNumber,
            entity.SourceEntityType,
            entity.SourceEntityId);

    private async Task<IReadOnlyList<PendingCoordinationCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int stalenessHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantProcurementCoordinationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var candidates = new List<PendingCoordinationCandidate>();

        var submittedPrStatus = PurchaseRequestStatuses.Submitted;
        var approvedPrStatus = PurchaseRequestStatuses.Approved;

        var purchaseRequests = await db.PurchaseRequests.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId)
                && (x.Status == submittedPrStatus || x.Status == approvedPrStatus))
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        var purchaseRequestIdsWithOpenPo = await db.PurchaseOrders.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId)
                && (PurchaseOrderStatuses.Open.Contains(x.Status)
                    || x.Status == PurchaseOrderStatuses.Issued))
            .Select(x => x.PurchaseRequestId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var openPoRequestIdSet = purchaseRequestIdsWithOpenPo.ToHashSet();

        foreach (var pr in purchaseRequests)
        {
            if (string.Equals(pr.Status, approvedPrStatus, StringComparison.OrdinalIgnoreCase)
                && openPoRequestIdSet.Contains(pr.Id))
            {
                continue;
            }

            candidates.Add(new PendingCoordinationCandidate(
                pr.TenantId,
                ProcurementCoordinationSubjectTypes.PurchaseRequest,
                pr.Id,
                pr.RequestKey,
                pr.Title,
                pr.Status,
                pr.UpdatedAt,
                null));
        }

        var activePoStatuses = PurchaseOrderStatuses.Open
            .Append(PurchaseOrderStatuses.Issued)
            .ToList();

        var purchaseOrders = await db.PurchaseOrders.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && activePoStatuses.Contains(x.Status))
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        foreach (var po in purchaseOrders)
        {
            candidates.Add(new PendingCoordinationCandidate(
                po.TenantId,
                ProcurementCoordinationSubjectTypes.PurchaseOrder,
                po.Id,
                po.OrderKey,
                po.Title,
                po.Status,
                po.UpdatedAt,
                null));
        }

        var rollupLookup = await db.ProcurementCoordinationRecords.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(
                x => (x.TenantId, x.SubjectType, x.SubjectId),
                x => (x.ComputedAt, x.SourceUpdatedAt),
                cancellationToken);

        var pending = new List<PendingCoordinationCandidate>();
        foreach (var candidate in candidates.OrderByDescending(x => x.SourceUpdatedAt))
        {
            DateTimeOffset? computedAt = null;
            if (rollupLookup.TryGetValue((candidate.TenantId, candidate.SubjectType, candidate.SubjectId), out var rollupState))
            {
                computedAt = rollupState.ComputedAt;
            }

            if (!ProcurementCoordinationRules.IsPending(
                    candidate.SourceUpdatedAt,
                    computedAt,
                    asOfUtc,
                    stalenessHours))
            {
                continue;
            }

            pending.Add(candidate with { LastComputedAt = computedAt });
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

    private sealed record PendingCoordinationCandidate(
        Guid TenantId,
        string SubjectType,
        Guid SubjectId,
        string DocumentKey,
        string Title,
        string DocumentStatus,
        DateTimeOffset SourceUpdatedAt,
        DateTimeOffset? LastComputedAt);
}
