namespace SupplyArr.Api.Contracts;

public sealed record ItemCategorySummaryResponse(
    string CategoryKey,
    int ItemCount);

public sealed record ManufacturerSummaryResponse(
    string ManufacturerName,
    int ItemCount);

public sealed record SupplierItemResponse(
    Guid LinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    string CategoryKey,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string SupplierPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus,
    DateTimeOffset CreatedAt);

public sealed record CreateSupplierItemRequest(
    Guid PartId,
    Guid SupplierId,
    string SupplierPartNumber,
    bool IsPreferred);
