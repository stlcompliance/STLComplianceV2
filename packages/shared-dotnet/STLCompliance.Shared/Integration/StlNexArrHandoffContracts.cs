namespace STLCompliance.Shared.Integration;

public sealed record StlNexArrRedeemHandoffRequest(string HandoffCode, string? ServiceToken);

public sealed record StlNexArrHandoffRedeemedResponse(
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
