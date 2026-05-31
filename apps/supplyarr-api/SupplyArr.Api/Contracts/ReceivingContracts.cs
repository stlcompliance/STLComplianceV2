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
    string Condition,
    decimal QuantityOrdered,
    decimal QuantityPreviouslyReceived,
    decimal QuantityRemainingOnOrder,
    IReadOnlyList<string> SerialLotNumbers,
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
    string PackingSlipReference,
    string PackingSlipFileName,
    string InvoiceReference,
    string InvoiceFileName,
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
    string? Notes,
    string? PackingSlipReference = null,
    string? PackingSlipFileName = null,
    IReadOnlyList<Guid>? PurchaseOrderLineIds = null);

public sealed record UpdateReceivingPackingSlipRequest(
    string? PackingSlipReference,
    string? PackingSlipFileName);

public sealed record UpdateReceivingInvoiceRequest(
    string? InvoiceReference,
    string? InvoiceFileName);

public sealed record UpdateReceivingInventoryBinRequest(Guid InventoryBinId);

public sealed record UpdateReceivingReceiptLineRequest(decimal QuantityReceived);

public sealed record UpdateReceivingReceiptLineConditionRequest(string Condition);

public sealed record UpdateReceivingReceiptLineTrackingRequest(
    IReadOnlyList<string> SerialLotNumbers);
