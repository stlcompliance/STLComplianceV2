namespace SupplyArr.Api.Contracts;

public sealed record IngestMaintainarrDemandRequest(
    Guid TenantId,
    Guid MaintainarrPublicationId,
    Guid MaintainarrWorkOrderId,
    string MaintainarrWorkOrderNumber,
    Guid MaintainarrAssetId,
    string Title,
    string? Notes,
    bool CreatePurchaseRequestDraft,
    IReadOnlyList<IngestMaintainarrDemandLineRequest> Lines);

public sealed record IngestMaintainarrDemandLineRequest(
    Guid MaintainarrDemandLineId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record MaintainarrDemandIntakeResponse(
    Guid DemandRefId,
    string Status,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    bool IdempotentReplay);

public sealed record MaintainArrDemandRefLineResponse(
    Guid LineId,
    int LineNumber,
    Guid MaintainarrDemandLineId,
    Guid? PartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes);

public sealed record MaintainArrDemandRefResponse(
    Guid DemandRefId,
    Guid MaintainarrPublicationId,
    Guid MaintainarrWorkOrderId,
    string MaintainarrWorkOrderNumber,
    Guid MaintainarrAssetId,
    string Title,
    string Notes,
    string Status,
    string ProcurementStatus,
    Guid? PurchaseRequestId,
    Guid? PurchaseOrderId,
    DateTimeOffset? LastStatusCallbackAt,
    DateTimeOffset ReceivedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<MaintainArrDemandRefLineResponse> Lines);

public sealed record CreatePurchaseRequestFromDemandRefRequest(
    string RequestKey,
    string Title,
    string? Notes);
