namespace SupplyArr.Api.Contracts;

public sealed record PartCatalogResponse(
    Guid CatalogId,
    string CatalogKey,
    string Name,
    string Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePartCatalogRequest(
    string CatalogKey,
    string Name,
    string Description);

public sealed record UpdatePartCatalogRequest(
    string Name,
    string Description);

public sealed record UpdatePartCatalogStatusRequest(string Status);

public sealed record PartManufacturerAliasResponse(
    Guid AliasId,
    string AliasKey,
    string ManufacturerName,
    string ManufacturerPartNumber,
    DateTimeOffset CreatedAt);

public sealed record PartVendorLinkResponse(
    Guid LinkId,
    Guid PartyId,
    string PartyKey,
    string PartyDisplayName,
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    DateTimeOffset CreatedAt);

public sealed record PartResponse(
    Guid PartId,
    string PartKey,
    Guid? CatalogId,
    string? CatalogKey,
    string DisplayName,
    string Description,
    string CategoryKey,
    string UnitOfMeasure,
    string ManufacturerName,
    string ManufacturerPartNumber,
    string Status,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    IReadOnlyList<PartManufacturerAliasResponse> ManufacturerAliases,
    IReadOnlyList<PartVendorLinkResponse> VendorLinks,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreatePartRequest(
    string PartKey,
    Guid? CatalogId,
    string DisplayName,
    string Description,
    string CategoryKey,
    string UnitOfMeasure,
    string ManufacturerName,
    string ManufacturerPartNumber);

public sealed record UpdatePartRequest(
    Guid? CatalogId,
    string DisplayName,
    string Description,
    string CategoryKey,
    string UnitOfMeasure,
    string ManufacturerName,
    string ManufacturerPartNumber);

public sealed record UpdatePartStatusRequest(string Status);

public sealed record CreatePartManufacturerAliasRequest(
    string AliasKey,
    string ManufacturerName,
    string ManufacturerPartNumber);

public sealed record CreatePartVendorLinkRequest(
    Guid PartyId,
    string VendorPartNumber,
    bool IsPreferred);
