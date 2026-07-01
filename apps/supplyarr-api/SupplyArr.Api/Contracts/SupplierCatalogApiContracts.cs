namespace SupplyArr.Api.Contracts;

public sealed record SupplierCatalogApiSyncItem(
    string PartKey,
    string SupplierPartNumber,
    bool IsPreferred,
    decimal? CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    int? CatalogLeadTimeDays,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus);

public sealed record SupplierCatalogApiSyncRequest(
    string SupplierKey,
    bool DryRun,
    IReadOnlyList<SupplierCatalogApiSyncItem> Items);

public sealed record SupplierCatalogApiSyncIssue(
    int ItemNumber,
    string Code,
    string Message);

public sealed record SupplierCatalogApiSyncResponse(
    string SyncType,
    bool DryRun,
    bool Success,
    int ItemsRead,
    int ItemsAccepted,
    int ItemsApplied,
    IReadOnlyList<SupplierCatalogApiSyncIssue> Issues);
