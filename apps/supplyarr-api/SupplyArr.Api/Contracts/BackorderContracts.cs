namespace SupplyArr.Api.Contracts;

public sealed record BackorderResponse(
    Guid BackorderId,
    string BackorderKey,
    string Status,
    string SourceType,
    Guid PurchaseOrderId,
    string PurchaseOrderKey,
    Guid PurchaseOrderLineId,
    int PurchaseOrderLineNumber,
    Guid? PurchaseRequestId,
    string? PurchaseRequestKey,
    Guid? PurchaseRequestLineId,
    Guid? ReceivingReceiptId,
    string? ReceivingReceiptKey,
    Guid? ReceivingReceiptLineId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityBackordered,
    decimal QuantityFulfilled,
    decimal QuantityOpen,
    DateTimeOffset? ExpectedBy,
    string Notes,
    Guid CreatedByUserId,
    Guid? FulfilledByUserId,
    DateTimeOffset? FulfilledAt,
    Guid? CancelledByUserId,
    DateTimeOffset? CancelledAt,
    string CancellationReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateBackorderFromPurchaseOrderLineRequest(
    string BackorderKey,
    decimal? QuantityBackordered,
    DateTimeOffset? ExpectedBy,
    string? Notes);

public sealed record CancelBackorderRequest(string Reason);
