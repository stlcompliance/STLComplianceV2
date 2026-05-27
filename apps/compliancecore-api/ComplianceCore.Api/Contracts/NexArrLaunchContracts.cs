namespace ComplianceCore.Api.Contracts;

public sealed record NexArrRedeemHandoffRequest(string HandoffCode, string ServiceToken);

public sealed record NexArrHandoffRedeemedResponse(
    Guid UserId,
    Guid TenantId,
    string TenantSlug,
    Guid SessionId,
    string Email,
    string DisplayName,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    string TargetProductKey);
