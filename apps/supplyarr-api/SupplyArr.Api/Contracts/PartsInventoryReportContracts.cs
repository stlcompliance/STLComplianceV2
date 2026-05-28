namespace SupplyArr.Api.Contracts;

public sealed record PartsInventoryReportTotalsResponse(
    int TotalPartCount,
    int ActivePartCount,
    int LocationCount,
    int BinCount,
    int BelowReorderPointCount,
    int ZeroStockPartCount,
    decimal TotalQuantityOnHand,
    decimal TotalQuantityReserved,
    decimal TotalQuantityAvailable);

public sealed record PartsInventoryLocationSummaryItemResponse(
    Guid InventoryLocationId,
    string LocationKey,
    string Name,
    string Status,
    int BinCount,
    int PartCountWithStock,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable);

public sealed record PartsInventoryPartSummaryItemResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    string Status,
    string CategoryKey,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    bool BelowReorderPoint,
    int VendorLinkCount);

public sealed record PartsInventoryReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    PartsInventoryReportTotalsResponse Totals,
    IReadOnlyList<PartsInventoryLocationSummaryItemResponse> Locations,
    IReadOnlyList<PartsInventoryPartSummaryItemResponse> Parts);

public sealed record PartsInventoryStockBinRowResponse(
    Guid PartStockLevelId,
    Guid InventoryBinId,
    string BinKey,
    string BinName,
    Guid InventoryLocationId,
    string LocationKey,
    string LocationName,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable);

public sealed record PartsInventoryPartVendorLinkRowResponse(
    Guid PartVendorLinkId,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string VendorPartNumber,
    bool IsPreferred);

public sealed record PartsInventoryPartDetailResponse(
    PartsInventoryPartSummaryItemResponse Summary,
    IReadOnlyList<PartsInventoryStockBinRowResponse> StockByBin,
    IReadOnlyList<PartsInventoryPartVendorLinkRowResponse> VendorLinks);

public sealed record PartsInventoryLocationBinRowResponse(
    Guid InventoryBinId,
    string BinKey,
    string BinName,
    string Status,
    int PartCountWithStock,
    decimal QuantityOnHand,
    decimal QuantityReserved);

public sealed record PartsInventoryLocationPartRowResponse(
    Guid PartId,
    string PartKey,
    string DisplayName,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable);

public sealed record PartsInventoryLocationDetailResponse(
    PartsInventoryLocationSummaryItemResponse Summary,
    IReadOnlyList<PartsInventoryLocationBinRowResponse> Bins,
    IReadOnlyList<PartsInventoryLocationPartRowResponse> Parts);
