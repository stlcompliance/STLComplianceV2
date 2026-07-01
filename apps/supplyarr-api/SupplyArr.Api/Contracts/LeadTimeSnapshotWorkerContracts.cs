namespace SupplyArr.Api.Contracts;

public sealed record LeadTimeSnapshotSettingsResponse(
    bool IsEnabled,
    int StalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertLeadTimeSnapshotSettingsRequest(
    bool IsEnabled,
    int StalenessHours);

public sealed record PendingLeadTimeSnapshotCaptureItem(
    Guid PartSupplierLinkId,
    Guid PartId,
    string PartKey,
    string PartDisplayName,
    Guid SupplierId,
    string SupplierKey,
    string SupplierDisplayName,
    string SupplierPartNumber,
    int CatalogLeadTimeDays,
    int? CurrentLeadTimeDays,
    DateTimeOffset? LastCapturedAt);

public sealed record PendingLeadTimeSnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingLeadTimeSnapshotCaptureItem> Items);

public sealed record LeadTimeSnapshotRunItem(
    Guid RunId,
    DateTimeOffset AsOfUtc,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    DateTimeOffset CreatedAt);

public sealed record LeadTimeSnapshotRunsResponse(
    IReadOnlyList<LeadTimeSnapshotRunItem> Items);

public sealed record ProcessLeadTimeSnapshotCapturesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record LeadTimeSnapshotCaptureSkip(
    Guid PartSupplierLinkId,
    string Reason);

public sealed record ProcessLeadTimeSnapshotCapturesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int CapturedCount,
    int SkippedCount,
    IReadOnlyList<LeadTimeSnapshotResponse> Captured,
    IReadOnlyList<LeadTimeSnapshotCaptureSkip> Skipped);

public sealed record UpsertPartSupplierLinkCatalogLeadTimeRequest(
    int CatalogLeadTimeDays);
