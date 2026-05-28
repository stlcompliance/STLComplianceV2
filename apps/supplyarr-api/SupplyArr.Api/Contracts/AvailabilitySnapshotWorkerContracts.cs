namespace SupplyArr.Api.Contracts;

public sealed record AvailabilitySnapshotSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertAvailabilitySnapshotSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingAvailabilitySnapshotCaptureItem(
    Guid PartVendorLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid VendorPartyId,
    string VendorPartyKey,
    string VendorDisplayName,
    string VendorPartNumber,
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus,
    decimal? CurrentQuantityAvailable,
    string? CurrentAvailabilityStatus,
    DateTimeOffset? LastCapturedAt);

public sealed record PendingAvailabilitySnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingAvailabilitySnapshotCaptureItem> Items);

public sealed record AvailabilitySnapshotRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record AvailabilitySnapshotRunsResponse(
    IReadOnlyList<AvailabilitySnapshotRunItem> Items);

public sealed record ProcessAvailabilitySnapshotCapturesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record AvailabilitySnapshotCaptureSkip(
    Guid PartVendorLinkId,
    string Reason);

public sealed record ProcessAvailabilitySnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    IReadOnlyList<AvailabilitySnapshotResponse> Captured,
    IReadOnlyList<AvailabilitySnapshotCaptureSkip> Skipped);

public sealed record UpsertPartVendorLinkCatalogAvailabilityRequest(
    decimal? CatalogQuantityAvailable,
    string? CatalogAvailabilityStatus);
