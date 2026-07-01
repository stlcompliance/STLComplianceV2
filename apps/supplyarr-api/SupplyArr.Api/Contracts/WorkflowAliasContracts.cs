namespace SupplyArr.Api.Contracts;

public sealed record ApprovalQueueItemResponse(
    string SubjectType,
    Guid SubjectId,
    string SubjectKey,
    string Status,
    Guid? SupplierId,
    DateTimeOffset UpdatedAt);

public sealed record StockTransactionItemResponse(
    Guid EventId,
    string Action,
    string? TargetId,
    string Result,
    DateTimeOffset OccurredAt);

public sealed record CreateStockTransactionRequest(
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    string TransactionType);

public sealed record CycleCountItemResponse(
    Guid StockLevelId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid BinId,
    string BinKey,
    Guid LocationId,
    string LocationKey,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    DateTimeOffset UpdatedAt);

public sealed record CreateCycleCountRequest(
    Guid PartId,
    Guid BinId,
    decimal QuantityOnHand);
