namespace SupplyArr.Api.Contracts;

public sealed record InventoryLocationResponse(
    Guid LocationId,
    string LocationKey,
    string Name,
    string LocationType,
    string AddressLine,
    Guid? StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string StaffarrSiteResolutionStatus,
    string Status,
    int BinCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateInventoryLocationRequest(
    string LocationKey,
    string Name,
    string LocationType,
    string AddressLine,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record UpdateInventoryLocationRequest(
    string Name,
    string LocationType,
    string AddressLine,
    Guid? StaffarrSiteOrgUnitId = null);

public sealed record UpdateInventoryLocationStatusRequest(string Status);

public sealed record InventoryBinResponse(
    Guid BinId,
    Guid LocationId,
    string LocationKey,
    string BinKey,
    string Name,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateInventoryBinRequest(
    string BinKey,
    string Name);

public sealed record UpdateInventoryBinRequest(string Name);

public sealed record UpdateInventoryBinStatusRequest(string Status);

public sealed record PartStockLevelResponse(
    Guid StockLevelId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid BinId,
    string BinKey,
    string BinName,
    Guid LocationId,
    string LocationKey,
    string LocationName,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record UpsertPartStockLevelRequest(
    Guid PartId,
    Guid BinId,
    decimal QuantityOnHand);
