namespace SupplyArr.Api.Contracts;

public sealed record IngestRoutarrDemandRequest(
    Guid TenantId,
    Guid RoutarrPublicationId,
    Guid RoutarrTripId,
    string RoutarrTripNumber,
    string RoutarrVehicleRefKey,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<IngestRoutarrDemandLineRequest> Lines);

public sealed record IngestRoutarrDemandLineRequest(
    Guid RoutarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record RoutarrDemandIntakeResponse(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed record RoutArrDemandRefLineResponse(
    Guid LineId,
    int LineNumber,
    Guid RoutarrDemandLineId,
    Guid? PartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes);

public sealed record RoutArrDemandRefResponse(
    Guid DemandRefId,
    Guid RoutarrPublicationId,
    Guid RoutarrTripId,
    string RoutarrTripNumber,
    string RoutarrVehicleRefKey,
    string Title,
    string Notes,
    string Status,
    string ProcurementStatus,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    DateTimeOffset ReceivedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<RoutArrDemandRefLineResponse> Lines);

public sealed record CreatePurchaseRequestFromRoutarrDemandRefRequest(
    string RequestKey,
    string Title,
    string? Notes);
