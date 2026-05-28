namespace NexArr.Api.Contracts;

public sealed record UpsertEntitlementReconciliationSettingsRequest(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleEntitlements);

public sealed record EntitlementReconciliationSettingsResponse(
    bool IsEnabled,
    bool AutoGrantFromLicense,
    bool AutoRevokeStaleEntitlements,
    DateTimeOffset? UpdatedAt);

public sealed record TenantProductLicenseResponse(
    Guid LicenseId,
    Guid TenantId,
    string ProductKey,
    string ProductDisplayName,
    string Status,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    string? ExternalReference,
    DateTimeOffset ModifiedAt);

public sealed record UpsertTenantProductLicenseRequest(
    Guid TenantId,
    string ProductKey,
    string Status,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo,
    string? ExternalReference);

public sealed record TenantProductLicensesResponse(
    IReadOnlyList<TenantProductLicenseResponse> Items);

public sealed record ProcessEntitlementReconciliationRequest(
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PendingEntitlementReconciliationItem(
    Guid TenantId,
    string TenantDisplayName,
    string ProductKey,
    string ProductDisplayName,
    string DriftKind,
    bool EntitlementActive,
    bool LicenseValid,
    Guid? EntitlementId,
    Guid? LicenseId);

public sealed record PendingEntitlementReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingEntitlementReconciliationItem> Items);

public sealed record EntitlementReconciliationActionSkip(
    Guid TenantId,
    string ProductKey,
    string DriftKind,
    string Reason);

public sealed record ProcessEntitlementReconciliationResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    IReadOnlyList<PendingEntitlementReconciliationItem> Applied,
    IReadOnlyList<EntitlementReconciliationActionSkip> Skipped);

public sealed record EntitlementReconciliationRunItem(
    Guid RunId,
    string Outcome,
    int DriftFoundCount,
    int GrantedCount,
    int RevokedCount,
    int SkippedCount,
    string? SkipReason,
    DateTimeOffset ProcessedAt);

public sealed record EntitlementReconciliationRunsResponse(
    IReadOnlyList<EntitlementReconciliationRunItem> Items);
