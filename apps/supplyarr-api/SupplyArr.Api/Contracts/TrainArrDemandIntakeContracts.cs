namespace SupplyArr.Api.Contracts;

public sealed record IngestTrainarrDemandRequest(
    Guid TenantId,
    Guid TrainarrPublicationId,
    Guid TrainarrAssignmentId,
    string TrainarrAssignmentRefKey,
    Guid StaffarrPersonId,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<IngestTrainarrDemandLineRequest> Lines);

public sealed record IngestTrainarrDemandLineRequest(
    Guid TrainarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record TrainarrDemandIntakeResponse(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed record TrainArrDemandRefLineResponse(
    Guid LineId,
    int LineNumber,
    Guid TrainarrDemandLineId,
    Guid? PartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes);

public sealed record TrainArrDemandRefResponse(
    Guid DemandRefId,
    Guid TrainarrPublicationId,
    Guid TrainarrAssignmentId,
    string TrainarrAssignmentRefKey,
    Guid StaffarrPersonId,
    string Title,
    string Notes,
    string Status,
    string ProcurementStatus,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    DateTimeOffset ReceivedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TrainArrDemandRefLineResponse> Lines);

public sealed record CreatePurchaseRequestFromTrainarrDemandRefRequest(
    string RequestKey,
    string Title,
    string? Notes);
