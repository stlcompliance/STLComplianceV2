namespace RoutArr.Api.Contracts;

public sealed record TripPartsDemandLineResponse(
    Guid DemandLineId,
    int LineNumber,
    Guid? SupplyarrPartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    string Status,
    Guid? RoutarrPublicationId,
    Guid? SupplyarrDemandRefId,
    DateTimeOffset? PublishedAt,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    decimal QuantityReceived,
    string ProcurementStatusMessage,
    DateTimeOffset? LastProcurementStatusAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTripPartsDemandLineRequest(
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record PublishTripPartsDemandRequest(bool CreatePurchaseRequestDraft);

public sealed record PublishTripPartsDemandResponse(
    Guid PublicationId,
    Guid DemandRefId,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    IReadOnlyList<TripPartsDemandLineResponse> Lines);
