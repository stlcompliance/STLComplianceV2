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
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements);

public sealed record StaffArrSessionBootstrapResponse(
    Guid UserId,
    Guid PersonId,
    Guid TenantId,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasStaffArrEntitlement,
    IReadOnlyList<string> Entitlements);

public sealed record StaffArrMeResponse(
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasStaffArrEntitlement,
    string? PrimaryOrgUnitName,
    string? JobTitle,
    IReadOnlyList<string> Entitlements);
