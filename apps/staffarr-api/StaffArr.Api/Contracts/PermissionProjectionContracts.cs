namespace StaffArr.Api.Contracts;

public sealed record PersonPermissionProjectionSummaryResponse(
    Guid PersonId,
    int PermissionCount,
    DateTimeOffset ComputedAt,
    IReadOnlyList<EffectivePermissionResponse> Permissions);

public sealed record PendingPermissionProjectionItem(
    Guid PersonId,
    string DisplayName,
    DateTimeOffset? LastComputedAt);

public sealed record PendingPermissionProjectionsResponse(
    DateTimeOffset AsOfUtc,
    int StalenessHours,
    int BatchSize,
    IReadOnlyList<PendingPermissionProjectionItem> Items);

public sealed record ProcessPermissionProjectionsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize,
    int? StalenessHours);

public sealed record PermissionProjectionRefreshSkip(
    Guid PersonId,
    string Reason);

public sealed record ProcessPermissionProjectionsResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int StalenessHours,
    int CandidatesFound,
    int RefreshedCount,
    int SkippedCount,
    IReadOnlyList<PersonPermissionProjectionSummaryResponse> RefreshedProjections,
    IReadOnlyList<PermissionProjectionRefreshSkip> Skipped);
