namespace SupplyArr.Api.Contracts;

public sealed record VendorCatalogApiSyncItem(
    string PartKey,
    string VendorPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus);

public sealed record VendorCatalogApiSyncRequest(
    string SupplierKey,
    string? VendorPartyKey,
    bool DryRun,
    IReadOnlyList<VendorCatalogApiSyncItem> Items);

public sealed record VendorCatalogApiSyncIssue(
    int ItemNumber,
    string Code,
    string Message);

public sealed record VendorCatalogApiSyncResponse(
    string SyncType,
    bool DryRun,
    bool Success,
    int ItemsRead,
    int ItemsAccepted,
    int ItemsApplied,
    IReadOnlyList<VendorCatalogApiSyncIssue> Issues);
