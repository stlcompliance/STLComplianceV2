namespace TrainArr.Api.Contracts;

public sealed record IngestSupplyarrDemandStatusRequest(
    Guid TenantId,
    Guid TrainarrPublicationId,
    Guid SupplyarrDemandRefId,
    Guid SupplyarrCallbackPublicationId,
    string EventType,
    string ProcurementStatus,
    Guid? SupplyarrPurchaseRequestId,
    Guid? SupplyarrPurchaseOrderId,
    Guid? SupplyarrReceivingReceiptId,
    decimal? QuantityReceivedDelta,
    string? Message,
    DateTimeOffset OccurredAt);

public sealed record IngestSupplyarrDemandStatusResponse(
    Guid StatusEventId,
    string ProcurementStatus,
    int UpdatedLineCount,
    bool IdempotentReplay);

