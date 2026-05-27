namespace StaffArr.Api.Contracts;

public sealed record RedeemHandoffRequest(string HandoffCode);

public sealed record HandoffSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    Guid SessionId,
    IReadOnlyList<string> Entitlements);

public sealed record StaffArrSessionBootstrapResponse(
    Guid UserId,
    Guid PersonId,
    Guid TenantId,
    Guid SessionId,
    string ProductKey,
    bool HasStaffArrEntitlement,
    IReadOnlyList<string> Entitlements);

public sealed record StaffArrMeResponse(
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string ProductKey,
    bool HasStaffArrEntitlement,
    IReadOnlyList<string> Entitlements);
