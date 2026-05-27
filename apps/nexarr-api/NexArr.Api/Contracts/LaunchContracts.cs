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
    string TargetProductKey,
    Guid SessionId,
    IReadOnlyList<string> Entitlements,
    string? CallbackUrl);

public sealed record ValidateCallbackRequest(string ProductKey, string CallbackUrl, Guid? TenantId);

public sealed record ValidateCallbackResponse(bool IsAllowed, string? ReasonCode);

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
