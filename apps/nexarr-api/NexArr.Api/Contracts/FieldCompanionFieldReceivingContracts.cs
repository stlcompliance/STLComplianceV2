namespace NexArr.Api.Contracts;

public sealed record FieldCompanionFieldReceivingLine(
    Guid LineId,
    int LineNumber,
    string PartKey,
    string PartDisplayName,
    decimal QuantityExpected,
    decimal QuantityReceived,
    decimal QuantityOrdered,
    decimal QuantityRemainingOnOrder,
    int OpenExceptionCount);

public sealed record FieldCompanionFieldReceivingDetailResponse(
    string TaskKey,
    string ProductKey,
    Guid ReceivingReceiptId,
    string ReceiptKey,
    string Status,
    string PurchaseOrderKey,
    string BinKey,
    string BinName,
    string LocationName,
    string Notes,
    IReadOnlyList<FieldCompanionFieldReceivingLine> Lines);

public sealed record UpdateFieldCompanionFieldReceivingLineRequest(
    string TaskKey,
    Guid LineId,
    decimal QuantityReceived);

public sealed record FieldCompanionFieldReceivingLineResponse(
    string TaskKey,
    string ProductKey,
    Guid ReceivingReceiptId,
    Guid LineId,
    decimal QuantityReceived,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record PostFieldCompanionFieldReceivingRequest(string TaskKey);

public sealed record FieldCompanionFieldReceivingPostResponse(
    string TaskKey,
    string ProductKey,
    Guid ReceivingReceiptId,
    string Status,
    DateTimeOffset PostedAt);

public sealed record SupplyArrReceivingReceiptUpstreamResponse(
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
    IReadOnlyList<SupplyArrReceivingReceiptLineUpstreamResponse> Lines,
    IReadOnlyList<SupplyArrReceivingExceptionUpstreamResponse> Exceptions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SupplyArrReceivingReceiptLineUpstreamResponse(
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
    IReadOnlyList<SupplyArrReceivingExceptionUpstreamResponse> Exceptions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SupplyArrReceivingExceptionUpstreamResponse(
    Guid ReceivingExceptionId,
    Guid ReceivingReceiptId,
    Guid ReceivingReceiptLineId,
    string ExceptionType,
    decimal Quantity,
    string Status,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SupplyArrUpdateReceivingReceiptLineUpstreamRequest(decimal QuantityReceived);
