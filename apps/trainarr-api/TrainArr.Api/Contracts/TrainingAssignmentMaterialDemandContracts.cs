namespace TrainArr.Api.Contracts;

public sealed record TrainingAssignmentMaterialDemandLineResponse(
    Guid DemandLineId,
    int LineNumber,
    Guid? SupplyarrPartId,
    string PartNumber,
    string Description,
    decimal QuantityRequested,
    string UnitOfMeasure,
    string Notes,
    string Status,
    Guid? TrainarrPublicationId,
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

public sealed record CreateTrainingAssignmentMaterialDemandLineRequest(
    Guid? SupplyarrPartId,
    string? PartNumber,
    string? Description,
    decimal QuantityRequested,
    string? UnitOfMeasure,
    string? Notes);

public sealed record PublishTrainingAssignmentMaterialDemandRequest(bool CreatePurchaseRequestDraft);

public sealed record PublishTrainingAssignmentMaterialDemandResponse(
    Guid PublicationId,
    Guid DemandRefId,
    Guid? PurchaseRequestId,
    bool CreatedPurchaseRequestDraft,
    IReadOnlyList<TrainingAssignmentMaterialDemandLineResponse> Lines);

public sealed record TrainingAssignmentMaterialDemandStatusEventResponse(
    Guid StatusEventId,
    Guid TrainarrPublicationId,
    Guid SupplyarrDemandRefId,
    string EventType,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    Guid? SupplyarrReceivingReceiptId,
    string Message,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
