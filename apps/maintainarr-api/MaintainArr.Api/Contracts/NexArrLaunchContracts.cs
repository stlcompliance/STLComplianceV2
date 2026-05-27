namespace MaintainArr.Api.Contracts;

public sealed record NexArrRedeemHandoffRequest(string HandoffCode, string? ServiceToken);

public sealed record NexArrHandoffRedeemedResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TargetProductKey,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    string? CallbackUrl);
