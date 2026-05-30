namespace MaintainArr.Api.Contracts;

public sealed record WorkOrderPartsDemandLineResponse(
    Guid DemandLineId,
    int LineNumber,
    Guid? SupplyarrPartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    string Status,
    Guid? MaintainarrPublicationId,
    Guid? SupplyarrDemandRefId,
    DateTimeOffset? PublishedAt,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    decimal QuantityReceived,
    string ProcurementStatusMessage,
    DateTimeOffset? LastProcurementStatusAt,
    DateTimeOffset CreatedAt);

public sealed record CreateWorkOrderPartsDemandLineRequest(
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record CreateWorkOrderPartsUsageAliasRequest(
    Guid WorkOrderId,
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record PublishWorkOrderPartsDemandRequest(
    bool CreatePurchaseRequestDraft = false);

public sealed record PublishWorkOrderPartsDemandResponse(
    Guid PublicationId,
    Guid SupplyarrDemandRefId,
    Guid? SupplyarrPurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    IReadOnlyList<WorkOrderPartsDemandLineResponse> Lines);

public sealed record WorkOrderPartsDemandStatusEventResponse(
    Guid StatusEventId,
    Guid MaintainarrPublicationId,
    Guid SupplyarrDemandRefId,
    string EventType,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    Guid? SupplyarrReceivingReceiptId,
    string Message,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
