namespace SupplyArr.Api.Contracts;

public sealed record StockReservationResponse(
    Guid ReservationId,
    string ReservationKey,
    string Status,
    string SourceType,
    Guid? SourceReferenceId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid BinId,
    string BinKey,
    string BinName,
    Guid LocationId,
    string LocationKey,
    string LocationName,
    Guid PartStockLevelId,
    decimal QuantityReserved,
    string Notes,
    Guid CreatedByUserId,
    Guid? FulfilledByUserId,
    DateTimeOffset? FulfilledAt,
    Guid? ReleasedByUserId,
    DateTimeOffset? ReleasedAt,
    string ReleaseReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateStockReservationRequest(
    string ReservationKey,
    Guid PartId,
    Guid BinId,
    decimal Quantity,
    string? SourceType,
    Guid? SourceReferenceId,
    string? Notes);

public sealed record ReleaseStockReservationRequest(string? Reason);
