using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripVendorReadinessService(
    RoutArrDbContext db,
    SupplyArrVendorOrderClient supplyArrVendorOrderClient)
{
    public async Task ApplyInitialVendorOrderLinkAsync(
        Trip trip,
        CancellationToken cancellationToken = default)
    {
        if (!trip.VendorOrderId.HasValue)
        {
            return;
        }

        var vendorOrder = await supplyArrVendorOrderClient.GetVendorOrderAsync(
            trip.TenantId,
            trip.VendorOrderId.Value,
            cancellationToken);

        trip.BrokerOrderId ??= vendorOrder.BrokerOrderId;
        StampSnapshots(
            trip,
            vendorOrder.Status,
            vendorOrder.QuantityReady,
            vendorOrder.OrderedQuantity,
            vendorOrder.ExpectedReadyAt,
            vendorOrder.ConfirmedReadyAt);

        var now = DateTimeOffset.UtcNow;
        if (IsReadyForDispatch(vendorOrder.Status))
        {
            ResolveActiveVendorBlocks(trip, null, null, null, now);
            trip.ReleasedForDispatchAt ??= now;
        }
        else
        {
            EnsureActiveVendorBlock(
                trip,
                vendorOrder.VendorOrderId,
                MapBlockReason(vendorOrder.Status),
                now);
        }
    }

    public void EnsureDispatchAllowed(Trip trip)
    {
        if (GetActiveVendorBlock(trip) is not null)
        {
            throw new StlApiException(
                "trip.vendor_readiness_blocked",
                "Trip is blocked until vendor readiness is released or explicitly overridden.",
                409);
        }
    }

    public TripDetailResponse ApplyVendorReadinessOverride(
        Trip trip,
        string reason,
        string actorPersonId,
        DateTimeOffset occurredAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new StlApiException(
                "trip.vendor_readiness_override_reason_required",
                "Vendor readiness overrides require a non-empty reason.",
                400);
        }

        if (GetActiveVendorBlock(trip) is null)
        {
            throw new StlApiException(
                "trip.vendor_readiness_override_not_needed",
                "Trip does not have an active vendor-readiness block.",
                409);
        }

        ResolveActiveVendorBlocks(trip, null, actorPersonId, reason.Trim(), occurredAt);
        trip.DispatchOverrideAt = occurredAt;
        trip.DispatchOverrideByPersonId = actorPersonId.Trim();
        trip.DispatchOverrideReason = reason.Trim();
        trip.UpdatedAt = occurredAt;

        return TripMappings.MapDetail(trip);
    }

    public async Task<IngestSupplyArrVendorOrderEventResponse> IngestVendorOrderEventAsync(
        IngestSupplyArrVendorOrderEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var replay = await db.SupplyArrVendorOrderEventReceipts.AnyAsync(
            x => x.TenantId == request.TenantId && x.EventId == request.EventId,
            cancellationToken);
        if (replay)
        {
            return new IngestSupplyArrVendorOrderEventResponse(request.EventId, true, 0);
        }

        var trips = await LoadMatchingTripsAsync(request, cancellationToken);
        foreach (var trip in trips)
        {
            ApplyEventToTrip(trip, request, request.OccurredAt);
        }

        db.SupplyArrVendorOrderEventReceipts.Add(new SupplyArrVendorOrderEventReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            EventId = request.EventId,
            EventType = request.EventType,
            VendorOrderId = request.VendorOrderId,
            ProcessedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
        return new IngestSupplyArrVendorOrderEventResponse(request.EventId, false, trips.Count);
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

    public static DispatchBlock? GetActiveVendorBlock(Trip trip) =>
        trip.DispatchBlocks.FirstOrDefault(x =>
            string.Equals(x.BlockType, DispatchBlockTypes.VendorReadiness, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Status, DispatchBlockStatuses.Active, StringComparison.OrdinalIgnoreCase));

    public static void StampSnapshots(
        Trip trip,
        string? vendorStatus,
        decimal quantityReady,
        decimal orderedQuantity,
        DateTimeOffset? expectedReadyAt,
        DateTimeOffset? confirmedReadyAt)
    {
        trip.VendorReadinessStatusSnapshot = NormalizeOptional(vendorStatus, 64);
        trip.VendorQuantityReadySnapshot = quantityReady;
        trip.VendorOrderedQuantitySnapshot = orderedQuantity;
        trip.VendorExpectedReadyAtSnapshot = expectedReadyAt;
        trip.VendorConfirmedReadyAtSnapshot = confirmedReadyAt;
    }

    public static void EnsureActiveVendorBlock(
        Trip trip,
        Guid vendorOrderId,
        string blockReason,
        DateTimeOffset now)
    {
        var existing = GetActiveVendorBlock(trip);
        if (existing is null)
        {
            trip.DispatchBlocks.Add(new DispatchBlock
            {
                Id = Guid.NewGuid(),
                TenantId = trip.TenantId,
                TripId = trip.Id,
                BlockType = DispatchBlockTypes.VendorReadiness,
                BlockReason = blockReason,
                BlockingEntityType = "vendor_order",
                BlockingEntityId = vendorOrderId.ToString(),
                Status = DispatchBlockStatuses.Active,
                CreatedAt = now,
            });
        }
        else
        {
            existing.BlockReason = blockReason;
            existing.BlockingEntityId = vendorOrderId.ToString();
            existing.Status = DispatchBlockStatuses.Active;
            existing.ResolvedAt = null;
            existing.ResolvedByEventId = null;
            existing.ResolvedByPersonId = null;
            existing.OverrideReason = null;
        }

        trip.DispatchBlockReason = blockReason;
        trip.UpdatedAt = now;
    }

    public static void ResolveActiveVendorBlocks(
        Trip trip,
        Guid? eventId,
        string? personId,
        string? overrideReason,
        DateTimeOffset now)
    {
        foreach (var block in trip.DispatchBlocks.Where(x =>
                     string.Equals(x.BlockType, DispatchBlockTypes.VendorReadiness, StringComparison.OrdinalIgnoreCase)
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

    public static bool IsReadyForDispatch(string? vendorStatus) =>
        string.Equals(vendorStatus, "completed_ready_for_dispatch", StringComparison.OrdinalIgnoreCase)
        || string.Equals(vendorStatus, "partial_dispatch_authorized", StringComparison.OrdinalIgnoreCase);

    public static string MapBlockReason(string? vendorStatus) =>
        vendorStatus?.Trim().ToLowerInvariant() switch
        {
            "unable_to_fulfill" => DispatchBlockReasons.VendorUnableToFulfill,
            "partially_ready" => DispatchBlockReasons.VendorOrderPartiallyReady,
            _ => DispatchBlockReasons.VendorOrderNotComplete,
        };

    private async Task<List<Trip>> LoadMatchingTripsAsync(
        IngestSupplyArrVendorOrderEventRequest request,
        CancellationToken cancellationToken)
    {
        var query = db.Trips
            .Include(x => x.DispatchBlocks)
            .Where(x => x.TenantId == request.TenantId);

        if (request.SelectedTripId.HasValue)
        {
            query = query.Where(x => x.Id == request.SelectedTripId.Value
                || x.VendorOrderId == request.VendorOrderId
                || (request.BrokerOrderId.HasValue && x.BrokerOrderId == request.BrokerOrderId.Value));
        }
        else if (request.BrokerOrderId.HasValue)
        {
            query = query.Where(x => x.VendorOrderId == request.VendorOrderId || x.BrokerOrderId == request.BrokerOrderId.Value);
        }
        else
        {
            query = query.Where(x => x.VendorOrderId == request.VendorOrderId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static void ApplyEventToTrip(
        Trip trip,
        IngestSupplyArrVendorOrderEventRequest request,
        DateTimeOffset occurredAt)
    {
        trip.BrokerOrderId ??= request.BrokerOrderId;
        if (!trip.VendorOrderId.HasValue || trip.VendorOrderId == request.VendorOrderId || request.ReadyChildVendorOrderId.HasValue)
        {
            trip.VendorOrderId ??= request.VendorOrderId;
        }

        if (string.Equals(request.EventType, "supplyarr.vendor_order.completed_for_dispatch", StringComparison.OrdinalIgnoreCase))
        {
            StampSnapshots(
                trip,
                request.NewStatus ?? "completed_ready_for_dispatch",
                request.QuantityReady,
                request.OrderedQuantity,
                request.ExpectedReadyAt,
                request.ConfirmedReadyAt);
            ResolveActiveVendorBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (string.Equals(request.EventType, "supplyarr.vendor_order.partial_dispatch_authorized", StringComparison.OrdinalIgnoreCase))
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
            ResolveActiveVendorBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (string.Equals(request.EventType, "supplyarr.vendor_order.split_created", StringComparison.OrdinalIgnoreCase))
        {
            if (request.SelectedTripId.HasValue && trip.Id != request.SelectedTripId.Value)
            {
                return;
            }

            if (request.ReadyChildVendorOrderId.HasValue)
            {
                trip.VendorOrderId = request.ReadyChildVendorOrderId.Value;
            }

            StampSnapshots(
                trip,
                "completed_ready_for_dispatch",
                request.QuantityReady,
                request.OrderedQuantity,
                request.ExpectedReadyAt,
                request.ConfirmedReadyAt);
            ResolveActiveVendorBlocks(trip, request.EventId, null, null, occurredAt);
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
            ResolveActiveVendorBlocks(trip, request.EventId, null, null, occurredAt);
            trip.ReleasedForDispatchAt = occurredAt;
            trip.ReleasedForDispatchByEventId = request.EventId;
            return;
        }

        if (!trip.VendorOrderId.HasValue)
        {
            trip.VendorOrderId = request.VendorOrderId;
        }

        EnsureActiveVendorBlock(
            trip,
            trip.VendorOrderId ?? request.VendorOrderId,
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
            trip.VendorOrderId,
            trip.BrokerOrderId,
            trip.DispatchBlockReason,
            trip.VendorReadinessStatusSnapshot,
            trip.VendorQuantityReadySnapshot,
            trip.VendorOrderedQuantitySnapshot,
            trip.VendorExpectedReadyAtSnapshot,
            trip.VendorConfirmedReadyAtSnapshot,
            trip.ReleasedForDispatchAt,
            trip.ReleasedForDispatchByEventId,
            trip.DispatchOverrideAt,
            trip.DispatchOverrideByPersonId,
            trip.DispatchOverrideReason,
            TripVendorReadinessService.MapBlocks(trip),
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
