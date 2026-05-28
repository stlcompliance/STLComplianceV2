namespace TrainArr.Api.Contracts;

public sealed record UpsertOrphanReferenceSettingsRequest(
    bool IsEnabled,
    int ScanStalenessHours);

public sealed record OrphanReferenceSettingsResponse(
    bool IsEnabled,
    int ScanStalenessHours,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessOrphanReferenceScansRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PendingOrphanReferenceScanItem(
    Guid TenantId,
    DateTimeOffset? LastScannedAt);

public sealed record PendingOrphanReferenceScansResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingOrphanReferenceScanItem> Items);

public sealed record OrphanReferenceScanSkip(
    Guid TenantId,
    string Reason);

public sealed record ProcessOrphanReferenceScansResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int TenantsScanned,
    int ReferencesChecked,
    int FindingsDetected,
    int FindingsResolved,
    int SkippedCount,
    IReadOnlyList<Guid> ScannedTenantIds,
    IReadOnlyList<OrphanReferenceScanSkip> Skipped);

public sealed record OrphanReferenceFindingItem(
    Guid FindingId,
    string ReferenceKind,
    string ReferenceKey,
    string SampleSourceEntityType,
    Guid SampleSourceEntityId,
    int AffectedSourceCount,
    bool IsActive,
    DateTimeOffset FirstDetectedAt,
    DateTimeOffset LastDetectedAt,
    DateTimeOffset? ResolvedAt);

public sealed record OrphanReferenceFindingsResponse(
    IReadOnlyList<OrphanReferenceFindingItem> Items);

public sealed record OrphanReferenceRunItem(
    Guid RunId,
    string Outcome,
    int ReferencesCheckedCount,
    int FindingsDetectedCount,
    int FindingsResolvedCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record OrphanReferenceRunsResponse(
    IReadOnlyList<OrphanReferenceRunItem> Items);
