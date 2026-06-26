namespace NexArr.Api.Contracts;

public record UpsertLaunchDestinationReconciliationSettingsRequest(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleLaunchDestinations)
{
    public bool AutoRevokeStaleEntitlements => AutoRevokeStaleLaunchDestinations;
}

public sealed record UpsertLaunchAvailabilityReconciliationSettingsRequest(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleLaunchDestinations)
    : UpsertLaunchDestinationReconciliationSettingsRequest(
        IsEnabled,
        AutoGrantFromLicense,
        AutoRevokeStaleLaunchDestinations);

public record LaunchDestinationReconciliationSettingsResponse(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleLaunchDestinations,
    DateTimeOffset? UpdatedAt)
{
    public bool AutoRevokeStaleEntitlements => AutoRevokeStaleLaunchDestinations;
}

public sealed record LaunchAvailabilityReconciliationSettingsResponse(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleLaunchDestinations,
    DateTimeOffset? UpdatedAt)
    : LaunchDestinationReconciliationSettingsResponse(
        IsEnabled,
        AutoGrantFromLicense,
        AutoRevokeStaleLaunchDestinations,
        UpdatedAt);

public record ProcessLaunchDestinationReconciliationRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record ProcessLaunchAvailabilityReconciliationRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize)
    : ProcessLaunchDestinationReconciliationRequest(AsOfUtc, BatchSize);

public record PendingLaunchDestinationReconciliationItem(
    Guid TenantId,
    string TenantDisplayName,
    string ProductKey,
    string ProductDisplayName,
    string DriftKind,
    bool LaunchDestinationActive,
    bool LicenseValid,
    Guid? LaunchDestinationRecordId,
    Guid? LicenseId)
{
    public bool EntitlementActive => LaunchDestinationActive;

    public Guid? EntitlementId => LaunchDestinationRecordId;
}

public sealed record PendingLaunchAvailabilityReconciliationItem(
    Guid TenantId,
    string TenantDisplayName,
    string ProductKey,
    string ProductDisplayName,
    string DriftKind,
    bool LaunchDestinationActive,
    bool LicenseValid,
    Guid? LaunchDestinationRecordId,
    Guid? LicenseId)
    : PendingLaunchDestinationReconciliationItem(
        TenantId,
        TenantDisplayName,
        ProductKey,
        ProductDisplayName,
        DriftKind,
        LaunchDestinationActive,
        LicenseValid,
        LaunchDestinationRecordId,
        LicenseId);

public record PendingLaunchDestinationReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingLaunchDestinationReconciliationItem> Items);

public sealed record PendingLaunchAvailabilityReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingLaunchDestinationReconciliationItem> Items)
    : PendingLaunchDestinationReconciliationResponse(AsOfUtc, BatchSize, Items);

public record LaunchDestinationReconciliationActionSkip(
    Guid TenantId,
    string ProductKey,
    string DriftKind,
    string Reason);

public sealed record LaunchAvailabilityReconciliationActionSkip(
    Guid TenantId,
    string ProductKey,
    string DriftKind,
    string Reason)
    : LaunchDestinationReconciliationActionSkip(TenantId, ProductKey, DriftKind, Reason);

public record ProcessLaunchDestinationReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    IReadOnlyList<PendingLaunchDestinationReconciliationItem> Applied,
    IReadOnlyList<LaunchDestinationReconciliationActionSkip> Skipped);

public sealed record ProcessLaunchAvailabilityReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    IReadOnlyList<PendingLaunchDestinationReconciliationItem> Applied,
    IReadOnlyList<LaunchDestinationReconciliationActionSkip> Skipped)
    : ProcessLaunchDestinationReconciliationResponse(
        AsOfUtc,
        BatchSize,
        DriftFoundCount,
        GrantedCount,
        RevokedCount,
        SkippedCount,
        Applied,
        Skipped);

public record LaunchDestinationReconciliationRunItem(
    Guid RunId,
    string Outcome,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record LaunchAvailabilityReconciliationRunItem(
    Guid RunId,
    string Outcome,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt)
    : LaunchDestinationReconciliationRunItem(
        RunId,
        Outcome,
        DriftFoundCount,
        GrantedCount,
        RevokedCount,
        SkippedCount,
        SkipReason,
        ProcessedAt);

public record LaunchDestinationReconciliationRunsResponse(
    IReadOnlyList<LaunchDestinationReconciliationRunItem> Items);

public sealed record LaunchAvailabilityReconciliationRunsResponse(
    IReadOnlyList<LaunchDestinationReconciliationRunItem> Items)
    : LaunchDestinationReconciliationRunsResponse(Items);
