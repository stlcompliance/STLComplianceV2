namespace SupplyArr.Api.Contracts;

public sealed record ReceivingExceptionResponse(
    Guid ReceivingExceptionId,
    Guid ReceivingReceiptId,
    Guid ReceivingReceiptLineId,
    int LineNumber,
    string PartKey,
    string ExceptionType,
    decimal Quantity,
    string Notes,
    string Status,
    Guid CreatedByUserId,
    Guid? ResolvedByUserId,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateReceivingExceptionRequest(
    string ExceptionType,
    decimal Quantity,
    string? Notes);
