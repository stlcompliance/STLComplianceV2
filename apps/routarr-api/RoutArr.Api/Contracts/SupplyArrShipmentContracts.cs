namespace RoutArr.Api.Contracts;

public sealed record SupplyArrShipmentLinePayload(
    Guid SupplyarrShipmentLineId,
    Guid PartId,
    string PartDisplayName,
    decimal Quantity);

public sealed record CreateSupplyArrShipmentIntentRequest(
    Guid TenantId,
    Guid SupplyarrShipmentId,
    string ShipmentKey,
    string DestinationName,
    string DestinationAddressSnapshot,
    IReadOnlyList<SupplyArrShipmentLinePayload> Lines);

public sealed record CreateSupplyArrShipmentIntentResponse(
    Guid ShipmentIntentId,
    Guid? RouteId,
    string Status);
