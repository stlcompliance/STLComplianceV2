namespace SupplyArr.Api.Contracts;

public sealed record ReceivingReceiptLineResponse(
    Guid LineId,
    int LineNumber,
    Guid PurchaseOrderLineId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    decimal QuantityExpected,
    decimal QuantityReceived,
    decimal QuantityOrdered,
    decimal QuantityPreviouslyReceived,
    decimal QuantityRemainingOnOrder,
    IReadOnlyList<ReceivingExceptionResponse> Exceptions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReceivingReceiptResponse(
    Guid ReceivingReceiptId,
    string ReceiptKey,
    string Status,
    Guid PurchaseOrderId,
    string PurchaseOrderKey,
    Guid InventoryBinId,
    string BinKey,
    string BinName,
    Guid InventoryLocationId,
    string LocationKey,
    string LocationName,
    string Notes,
    Guid CreatedByUserId,
    DateTimeOffset? PostedAt,
    Guid? PostedByUserId,
    IReadOnlyList<ReceivingReceiptLineResponse> Lines,
    IReadOnlyList<ReceivingExceptionResponse> Exceptions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateReceivingReceiptFromPurchaseOrderRequest(
    string ReceiptKey,
    Guid InventoryBinId,
    string? Notes);

public sealed record UpdateReceivingReceiptLineRequest(decimal QuantityReceived);
