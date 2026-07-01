namespace RoutArr.Api.Contracts;

public sealed record DispatchBlockResponse(
    Guid DispatchBlockId,
    string BlockType,
    string BlockReason,
    string BlockingEntityType,
    string BlockingEntityId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    Guid? ResolvedByEventId,
    string? ResolvedByPersonId,
    string? OverrideReason);

public sealed record TripSupplierReadinessOverrideRequest(
    string Reason);

public sealed record IngestSupplyArrSupplierOrderEventRequest(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SupplierOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    string? PreviousStatus,
    string? NewStatus,
    Guid SupplierId,
    string SupplierNameSnapshot,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    string? Source,
    Guid? SelectedTripId,
    decimal? AuthorizedQuantity,
    Guid? ReadyChildSupplierOrderId,
    Guid? RemainingChildSupplierOrderId);

public sealed record IngestSupplyArrSupplierOrderEventResponse(
    Guid EventId,
    bool WasReplay,
    int MatchedTripCount);
