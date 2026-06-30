namespace SupplyArr.Api.Contracts;

public sealed record PriceSnapshotSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertPriceSnapshotSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingPriceSnapshotCaptureItem(
    Guid PartVendorLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string VendorPartNumber,
    decimal CatalogUnitPrice,
    string CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity,
    decimal? CurrentUnitPrice,
    string? CurrentCurrencyCode,
    DateTimeOffset? LastCapturedAt);

public sealed record PendingPriceSnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingPriceSnapshotCaptureItem> Items);

public sealed record PriceSnapshotRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record PriceSnapshotRunsResponse(
    IReadOnlyList<PriceSnapshotRunItem> Items);

public sealed record ProcessPriceSnapshotCapturesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PriceSnapshotCaptureSkip(
    Guid PartVendorLinkId,
    string Reason);

public sealed record ProcessPriceSnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    IReadOnlyList<PricingSnapshotResponse> Captured,
    IReadOnlyList<PriceSnapshotCaptureSkip> Skipped);

public sealed record UpsertPartVendorLinkCatalogPriceRequest(
    decimal CatalogUnitPrice,
    string? CatalogCurrencyCode,
    decimal? CatalogMinimumOrderQuantity);
