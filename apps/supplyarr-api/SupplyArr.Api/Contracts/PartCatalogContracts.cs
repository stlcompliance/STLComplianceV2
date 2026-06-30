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
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid? ParentSupplierId,
    string? ParentSupplierKey,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string SupplierAddressLine1,
    string SupplierLocality,
    string SupplierRegionCode,
    string SupplierPostalCode,
    string SupplierCountryCode,
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus,
    DateTimeOffset CreatedAt)
{
    public Guid PartyId => SupplierId;

    public string PartyKey => SupplierKey;

    public string PartyDisplayName => SupplierDisplayName;
}

public sealed record PartSourceResponse(
    Guid SourceId,
    string SourceType,
    string Label,
    string Notes,
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
    bool IsTrackable,
    bool IsStocked,
    bool RequiresSerialLotTracking,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    IReadOnlyList<PartManufacturerAliasResponse> ManufacturerAliases,
    IReadOnlyList<PartSourceResponse> Sources,
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
    string ManufacturerPartNumber,
    bool? IsTrackable = null,
    bool? IsStocked = null,
    bool RequiresSerialLotTracking = false);

public sealed record UpdatePartRequest(
    Guid? CatalogId,
    string DisplayName,
    string Description,
    string CategoryKey,
    string UnitOfMeasure,
    string ManufacturerName,
    string ManufacturerPartNumber,
    bool? IsTrackable = null,
    bool? IsStocked = null,
    bool RequiresSerialLotTracking = false);

public sealed record UpdatePartStatusRequest(string Status);

public sealed record CreatePartManufacturerAliasRequest(
    string AliasKey,
    string ManufacturerName,
    string ManufacturerPartNumber);

public sealed record CreatePartSourceRequest(
    string SourceType,
    string Label,
    string Notes);

public sealed record CreatePartVendorLinkRequest(
    Guid? SupplierUnitId,
    Guid? SupplierId,
    Guid? PartyId,
    string VendorPartNumber,
    bool IsPreferred)
{
    public CreatePartVendorLinkRequest(
        Guid? partyId,
        string vendorPartNumber,
        bool isPreferred)
        : this(partyId, partyId, partyId, vendorPartNumber, isPreferred)
    {
    }
}
