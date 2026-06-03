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
    Guid? CancelledByUserId,
    DateTimeOffset? CancelledAt,
    string CancellationReason,
    Guid? ReopenedByUserId,
    DateTimeOffset? ReopenedAt,
    string LastReopenReason,
    int ReopenCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateReceivingExceptionRequest(
    string ExceptionType,
    decimal Quantity,
    string? Notes);

public sealed record CancelReceivingExceptionRequest(string Reason);

public sealed record ReopenReceivingExceptionRequest(string Reason);
