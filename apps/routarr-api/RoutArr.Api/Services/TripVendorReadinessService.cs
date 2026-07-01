using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripSupplierReadinessService(
    RoutArrDbContext db,
    SupplyArrSupplierOrderClient supplyArrSupplierOrderClient)
{
    public async Task ApplyInitialSupplierOrderLinkAsync(
        Trip trip,
        CancellationToken cancellationToken = default)
    {
        if (!trip.SupplierOrderId.HasValue)
        {
            return;
        }

        var supplierOrder = await supplyArrSupplierOrderClient.GetSupplierOrderAsync(
            trip.TenantId,
            trip.SupplierOrderId.Value,
            cancellationToken);

        trip.BrokerOrderId ??= supplierOrder.BrokerOrderId;
        StampSnapshots(
            trip,
            supplierOrder.Status,
            supplierOrder.QuantityReady,
            supplierOrder.OrderedQuantity,
            supplierOrder.ExpectedReadyAt,
            supplierOrder.ConfirmedReadyAt);

        var now = DateTimeOffset.UtcNow;
        if (IsReadyForDispatch(supplierOrder.Status))
        {
            ResolveActiveSupplierBlocks(trip, null, null, null, now);
            trip.ReleasedForDispatchAt ??= now;
        }
        else
        {
            EnsureActiveSupplierBlock(
                trip,
                supplierOrder.SupplierOrderId,
                MapBlockReason(supplierOrder.Status),
                now);
        }
    }

    public void EnsureDispatchAllowed(Trip trip)
    {
        if (GetActiveSupplierBlock(trip) is not null)
        {
            throw new StlApiException(
                "trip.supplier_readiness_blocked",
                "Trip is blocked until supplier readiness is released or explicitly overridden.",
                409);
        }
    }

    public TripDetailResponse ApplySupplierReadinessOverride(
        Trip trip,
        string reason,
        string actorPersonId,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new StlApiException(
                "trip.supplier_readiness_override_reason_required",
                "Supplier readiness overrides require a non-empty reason.",
                400);
        }

        if (GetActiveSupplierBlock(trip) is null)
        {
            throw new StlApiException(
                "trip.supplier_readiness_override_not_needed",
                "Trip does not have an active supplier-readiness block.",
                409);
        }

        ResolveActiveSupplierBlocks(trip, null, actorPersonId, reason.Trim(), occurredAt);
        trip.DispatchOverrideAt = occurredAt;
        trip.DispatchOverrideByPersonId = actorPersonId.Trim();
        trip.DispatchOverrideReason = reason.Trim();
        trip.UpdatedAt = occurredAt;

        return TripMappings.MapDetail(trip);
    }

    public async Task<IngestSupplyArrSupplierOrderEventResponse> IngestSupplierOrderEventAsync(
        IngestSupplyArrSupplierOrderEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var replay = await db.SupplyArrSupplierOrderEventReceipts.AnyAsync(
            x => x.TenantId == request.TenantId && x.EventId == request.EventId,
            cancellationToken);
        if (replay)
        {
            return new IngestSupplyArrSupplierOrderEventResponse(request.EventId, true, 0);
        }

        var trips = await LoadMatchingTripsAsync(request, cancellationToken);
        foreach (var trip in trips)
        {
            ApplyEventToTrip(trip, request, request.OccurredAt);
        }

        db.SupplyArrSupplierOrderEventReceipts.Add(new SupplyArrSupplierOrderEventReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            EventId = request.EventId,
            EventType = request.EventType,
            SupplierOrderId = request.SupplierOrderId,
            ProcessedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
        return new IngestSupplyArrSupplierOrderEventResponse(request.EventId, false, trips.Count);
    }

    public static IReadOnlyList<DispatchBlockResponse> MapBlocks(Trip trip) =>
        trip.DispatchBlocks
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapBlock)
            .ToList();

    public static DispatchBlockResponse MapBlock(DispatchBlock block) =>
        new(
            block.Id,
            block.BlockType,
            block.BlockReason,
            block.BlockingEntityType,
            block.BlockingEntityId,
            block.Status,
            block.CreatedAt,
            block.ResolvedAt,
            block.ResolvedByEventId,
            block.ResolvedByPersonId,
            block.OverrideReason);

    public static DispatchBlock? GetActiveSupplierBlock(Trip trip) =>
        trip.DispatchBlocks.FirstOrDefault(x =>
            string.Equals(x.BlockType, DispatchBlockTypes.SupplierReadiness, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Status, DispatchBlockStatuses.Active, StringComparison.OrdinalIgnoreCase));

    public static void StampSnapshots(
        Trip trip,
        string? supplierStatus,
        decimal quantityReady,
        decimal orderedQuantity,
        DateTimeOffset? expectedReadyAt,
        DateTimeOffset? confirmedReadyAt)
    {
        trip.SupplierReadinessStatusSnapshot = NormalizeOptional(supplierStatus, 64);
        trip.SupplierQuantityReadySnapshot = quantityReady;
        trip.SupplierOrderedQuantitySnapshot = orderedQuantity;
        trip.SupplierExpectedReadyAtSnapshot = expectedReadyAt;
        trip.SupplierConfirmedReadyAtSnapshot = confirmedReadyAt;
    }

    public static void EnsureActiveSupplierBlock(
        Trip trip,
        Guid supplierOrderId,
        string blockReason,
        DateTimeOffset now)
    {
        var existing = GetActiveSupplierBlock(trip);
        if (existing is null)
        {
            trip.DispatchBlocks.Add(new DispatchBlock
            {
                Id = Guid.NewGuid(),
                TenantId = trip.TenantId,
                TripId = trip.Id,
                BlockType = DispatchBlockTypes.SupplierReadiness,
                BlockReason = blockReason,
                BlockingEntityType = "supplier_order",
                BlockingEntityId = supplierOrderId.ToString(),
                Status = DispatchBlockStatuses.Active,
                CreatedAt = now,
            });
        }
        else
        {
            existing.BlockReason = blockReason;
            existing.BlockingEntityId = supplierOrderId.ToString();
            existing.Status = DispatchBlockStatuses.Active;
            existing.ResolvedAt = null;
            existing.ResolvedByEventId = null;
            existing.ResolvedByPersonId = null;
            existing.OverrideReason = null;
        }

        trip.DispatchBlockReason = blockReason;
        trip.UpdatedAt = now;
    }

    public static void ResolveActiveSupplierBlocks(
        Trip trip,
        Guid? eventId,
        string? personId,
        string? overrideReason,
        DateTimeOffset now)
    {
        foreach (var block in trip.DispatchBlocks.Where(x =>
                     string.Equals(x.BlockType, DispatchBlockTypes.SupplierReadiness, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(x.Status, DispatchBlockStatuses.Active, StringComparison.OrdinalIgnoreCase)))
        {
            block.Status = DispatchBlockStatuses.Resolved;
            block.ResolvedAt = now;
            block.ResolvedByEventId = eventId;
            block.ResolvedByPersonId = NormalizeOptional(personId, 128);
            block.OverrideReason = NormalizeOptional(overrideReason, 1024);
        }

        trip.DispatchBlockReason = null;
        trip.UpdatedAt = now;
    }

    public static bool IsReadyForDispatch(string? supplierStatus) =>
        string.Equals(supplierStatus, "completed_ready_for_dispatch", StringComparison.OrdinalIgnoreCase)
        || string.Equals(supplierStatus, "partial_dispatch_authorized", StringComparison.OrdinalIgnoreCase);

    public static string MapBlockReason(string? supplierStatus) =>
        supplierStatus?.Trim().ToLowerInvariant() switch
        {
            "unable_to_fulfill" => DispatchBlockReasons.SupplierUnableToFulfill,
            "partially_ready" => DispatchBlockReasons.SupplierOrderPartiallyReady,
            _ => DispatchBlockReasons.SupplierOrderNotComplete,
        };

    private async Task<List<Trip>> LoadMatchingTripsAsync(
        IngestSupplyArrSupplierOrderEventRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Trips
            .Include(x => x.DispatchBlocks)
            .Where(x => x.TenantId == request.TenantId);

        if (request.SelectedTripId.HasValue)
        {
            query = query.Where(x => x.Id == request.SelectedTripId.Value
                || x.SupplierOrderId == request.SupplierOrderId
                || (request.BrokerOrderId.HasValue && x.BrokerOrderId == request.BrokerOrderId.Value));
        }
        else if (request.BrokerOrderId.HasValue)
        {
            query = query.Where(x => x.SupplierOrderId == request.SupplierOrderId || x.BrokerOrderId == request.BrokerOrderId.Value);
        }
        else
        {
            query = query.Where(x => x.SupplierOrderId == request.SupplierOrderId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static void ApplyEventToTrip(
        Trip trip,
        IngestSupplyArrSupplierOrderEventRequest request,
        DateTimeOffset occurredAt)
    {
        trip.BrokerOrderId ??= request.BrokerOrderId;
        if (!trip.SupplierOrderId.HasValue || trip.SupplierOrderId == request.SupplierOrderId || request.ReadyChildSupplierOrderId.HasValue)
        {
            trip.SupplierOrderId ??= request.SupplierOrderId;
        }

        if (string.Equals(request.EventType, "supplyarr.supplier_order.completed_for_dispatch", StringComparison.OrdinalIgnoreCase))
        {
            StampSnapshots(
                trip,
                request.NewStatus ?? "completed_ready_for_dispatch",
                request.QuantityReady,
                request.OrderedQuantity,
                request.ExpectedReadyAt,
                request.ConfirmedReadyAt);
            ResolveActiveSupplierBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (string.Equals(request.EventType, "supplyarr.supplier_order.partial_dispatch_authorized", StringComparison.OrdinalIgnoreCase))
        {
            if (request.SelectedTripId.HasValue && trip.Id != request.SelectedTripId.Value)
            {
                return;
            }

            StampSnapshots(
                trip,
                "partial_dispatch_authorized",
                request.AuthorizedQuantity ?? request.QuantityReady,
                request.OrderedQuantity,
                request.ExpectedReadyAt,
                request.ConfirmedReadyAt);
            ResolveActiveSupplierBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (string.Equals(request.EventType, "supplyarr.supplier_order.split_created", StringComparison.OrdinalIgnoreCase))
        {
            if (request.SelectedTripId.HasValue && trip.Id != request.SelectedTripId.Value)
            {
                return;
            }

            if (request.ReadyChildSupplierOrderId.HasValue)
            {
                trip.SupplierOrderId = request.ReadyChildSupplierOrderId.Value;
            }

            StampSnapshots(
                trip,
                "completed_ready_for_dispatch",
                request.QuantityReady,
                request.OrderedQuantity,
                request.ExpectedReadyAt,
                request.ConfirmedReadyAt);
            ResolveActiveSupplierBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt ??= occurredAt;
            trip.ReleasedForDispatchByEventId ??= request.EventId;
            return;
        }

        StampSnapshots(
            trip,
            request.NewStatus,
            request.QuantityReady,
            request.OrderedQuantity,
            request.ExpectedReadyAt,
            request.ConfirmedReadyAt);

        if (IsReadyForDispatch(request.NewStatus))
        {
            ResolveActiveSupplierBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (!trip.SupplierOrderId.HasValue)
        {
            trip.SupplierOrderId = request.SupplierOrderId;
        }

        EnsureActiveSupplierBlock(
            trip,
            trip.SupplierOrderId ?? request.SupplierOrderId,
            MapBlockReason(request.NewStatus),
            occurredAt);
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

internal static class TripMappings
{
    public static TripDetailResponse MapDetail(Trip trip) =>
        new(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.Description,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            trip.Loads
                .OrderBy(x => x.SequenceNumber)
                .Select(load => new TripLoadSummaryResponse(
                    load.Id,
                    load.LoadKey,
                    load.Description,
                    load.LoadType,
                    load.Status,
                    load.SequenceNumber,
                    load.OriginLabel,
                    load.DestinationLabel,
                    load.CreatedAt,
                    load.UpdatedAt))
                .ToList(),
            trip.CreatedByUserId,
            trip.CreatedAt,
            trip.UpdatedAt,
            trip.AssignedAt,
            trip.AcceptedAt,
            trip.DispatchedAt,
            trip.StartedAt,
            trip.CompletedAt,
            trip.ClosedAt,
            trip.CancelledAt,
            trip.SupplierOrderId,
            trip.BrokerOrderId,
            trip.DispatchBlockReason,
            trip.SupplierReadinessStatusSnapshot,
            trip.SupplierQuantityReadySnapshot,
            trip.SupplierOrderedQuantitySnapshot,
            trip.SupplierExpectedReadyAtSnapshot,
            trip.SupplierConfirmedReadyAtSnapshot,
            trip.ReleasedForDispatchAt,
            trip.ReleasedForDispatchByEventId,
            trip.DispatchOverrideAt,
            trip.DispatchOverrideByPersonId,
            trip.DispatchOverrideReason,
            TripSupplierReadinessService.MapBlocks(trip),
            trip.DispatchReleaseSnapshot is null
                ? null
                : new TripDispatchReleaseSnapshotResponse(
                    trip.DispatchReleaseSnapshot.Id,
                    trip.DispatchReleaseSnapshot.ReleasedAt,
                    trip.DispatchReleaseSnapshot.ReleasedByUserId,
                    trip.DispatchReleaseSnapshot.DriverCanAssign,
                    trip.DispatchReleaseSnapshot.VehicleCanAssign,
                    trip.DispatchReleaseSnapshot.HasMissingExternalData,
                    trip.DispatchReleaseSnapshot.HasStaleExternalData,
                    trip.DispatchReleaseSnapshot.Summary));
}
