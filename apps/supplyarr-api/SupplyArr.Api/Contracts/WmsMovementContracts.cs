namespace SupplyArr.Api.Contracts;

public sealed record WmsStockLedgerEntryResponse(
    Guid LedgerEntryId,
    Guid MovementGroupId,
    string IdempotencyKey,
    string MovementType,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid InventoryBinId,
    string BinKey,
    string BinName,
    Guid LocationId,
    string LocationKey,
    string LocationName,
    Guid? StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    Guid? RelatedInventoryBinId,
    decimal QuantityOnHandDelta,
    decimal QuantityReservedDelta,
    decimal QuantityOnHandAfter,
    decimal QuantityReservedAfter,
    string SourceType,
    Guid? SourceReferenceId,
    string Notes,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt);

public sealed record WmsMovementResponse(
    Guid MovementGroupId,
    string IdempotencyKey,
    IReadOnlyList<WmsStockLedgerEntryResponse> Entries);

public sealed record TransferStockRequest(
    string IdempotencyKey,
    Guid PartId,
    Guid FromBinId,
    Guid ToBinId,
    decimal Quantity,
    string? Notes = null);

public sealed record ReserveStockRequest(
    string IdempotencyKey,
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    string? SourceType = null,
    Guid? SourceReferenceId = null,
    string? Notes = null);

public sealed record PickStockRequest(
    string IdempotencyKey,
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    Guid? OutboundShipmentLineId = null,
    string? Notes = null);

public sealed record ShipStockRequest(
    string IdempotencyKey,
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    Guid? OutboundShipmentLineId = null,
    string? Notes = null);

public sealed record CancelStockMovementRequest(
    string IdempotencyKey,
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    string? Reason = null);

public sealed record CreateOutboundShipmentLineRequest(
    Guid PartId,
    Guid FromBinId,
    decimal Quantity);

public sealed record CreateOutboundShipmentRequest(
    string IdempotencyKey,
    string ShipmentKey,
    string ShipVia,
    string DestinationName,
    string DestinationAddressSnapshot,
    IReadOnlyList<CreateOutboundShipmentLineRequest> Lines);

public sealed record OutboundShipmentLineResponse(
    Guid ShipmentLineId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid FromBinId,
    string FromBinKey,
    decimal QuantityRequested,
    decimal QuantityReserved,
    decimal QuantityPicked,
    decimal QuantityShipped,
    string Status);

public sealed record OutboundShipmentResponse(
    Guid ShipmentId,
    string ShipmentKey,
    string Status,
    string ShipVia,
    string DestinationName,
    string DestinationAddressSnapshot,
    Guid? RoutarrShipmentIntentId,
    Guid? RoutarrRouteId,
    string RoutarrStatus,
    string IdempotencyKey,
    IReadOnlyList<OutboundShipmentLineResponse> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RoutArrShipmentStatusUpdateRequest(
    Guid TenantId,
    Guid SupplyarrShipmentId,
    string Status,
    Guid? RoutarrRouteId,
    string? Message,
    DateTimeOffset OccurredAt);
