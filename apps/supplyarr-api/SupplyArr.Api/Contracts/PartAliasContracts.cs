namespace SupplyArr.Api.Contracts;

public sealed record ItemCategorySummaryResponse(
    string CategoryKey,
    int ItemCount);

public sealed record ManufacturerSummaryResponse(
    string ManufacturerName,
    int ItemCount);

public sealed record VendorItemResponse(
    Guid LinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    string CategoryKey,
    Guid PartyId,
    string PartyKey,
    string PartyDisplayName,
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus,
    DateTimeOffset CreatedAt);

public sealed record CreateVendorItemRequest(
    Guid PartId,
    Guid PartyId,
    string VendorPartNumber,
    bool IsPreferred);

