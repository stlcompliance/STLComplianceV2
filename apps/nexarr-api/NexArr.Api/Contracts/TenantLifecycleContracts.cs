namespace NexArr.Api.Contracts;

public sealed record UpsertTenantLifecycleSettingsRequest(
    bool IsEnabled,
    bool AutoSuspendWhenNoValidLicense,
    int SuspendGraceDaysAfterLastLicenseExpiry,
    bool AutoReactivateWhenValidLicense,
    bool RevokeSessionsOnSuspend);

public sealed record TenantLifecycleSettingsResponse(
    bool IsEnabled,
    bool AutoSuspendWhenNoValidLicense,
    int SuspendGraceDaysAfterLastLicenseExpiry,
    bool AutoReactivateWhenValidLicense,
    bool RevokeSessionsOnSuspend,
    DateTimeOffset? UpdatedAt);

public sealed record ProcessTenantLifecycleRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingTenantLifecycleItem(
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string CurrentStatus,
    string ActionKind,
    bool HasValidLicense,
    DateTimeOffset? LastLicenseCoverageEndedAt,
    DateTimeOffset? EligibleAt);

public sealed record PendingTenantLifecycleResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingTenantLifecycleItem> Items);

public sealed record TenantLifecycleActionSkip(
    Guid TenantId,
    string ActionKind,
    string Reason);

public sealed record ProcessTenantLifecycleResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingCount,
    int SuspendedCount,
    int ReactivatedCount,
    int SessionsRevokedCount,
    int SkippedCount,
    IReadOnlyList<PendingTenantLifecycleItem> Applied,
    IReadOnlyList<TenantLifecycleActionSkip> Skipped);

public sealed record TenantLifecycleRunItem(
    Guid RunId,
    string Outcome,
    int PendingCount,
    int SuspendedCount,
    int ReactivatedCount,
    int SessionsRevokedCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record TenantLifecycleRunsResponse(
    IReadOnlyList<TenantLifecycleRunItem> Items);
