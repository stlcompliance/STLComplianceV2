namespace NexArr.Api.Contracts;

public sealed record LaunchContextResponse(
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    Guid UserId,
    string UserEmail,
    string ProductKey,
    string ProductDisplayName,
    string BaseLaunchUrl,
    string LaunchUrl,
    bool CanLaunch,
    string? DenialReasonCode);

public sealed record CreateHandoffRequest(string ProductKey, string? CallbackUrl);

public sealed record HandoffCreatedResponse(
    string HandoffCode,
    Guid HandoffId,
    DateTimeOffset ExpiresAt,
    string LaunchUrl);

public sealed record RedeemHandoffRequest(string HandoffCode, string? ServiceToken);

public sealed record HandoffRedeemedResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string TargetProductKey,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    string? CallbackUrl);

public sealed record ValidateCallbackRequest(string ProductKey, string CallbackUrl, Guid? TenantId);

public sealed record ValidateCallbackResponse(bool IsAllowed, string? ReasonCode);

public sealed record LaunchCatalogResponse(
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string? CurrentProductKey,
    IReadOnlyList<LaunchCatalogItemResponse> Products,
    DateTimeOffset GeneratedAt);

public sealed record LaunchCatalogItemResponse(
    string ProductKey,
    string DisplayName,
    string ProductStatus,
    string LaunchUrl,
    bool IsCurrentProduct);

public sealed record ValidateLaunchRequest(
    string ProductKey,
    Guid? TenantId);

public sealed record ValidateLaunchResponse(
    Guid TenantId,
    string ProductKey,
    bool CanLaunch,
    string? ReasonCode,
    string? LaunchUrl);

public sealed record CreateCallbackAllowlistEntryRequest(
    string ProductKey,
    Guid? TenantId,
    string UrlPattern,
    string PatternType);

public sealed record CallbackAllowlistEntryResponse(
    Guid EntryId,
    string ProductKey,
    Guid? TenantId,
    string UrlPattern,
    string PatternType,
    bool IsActive,
    DateTimeOffset CreatedAt);
