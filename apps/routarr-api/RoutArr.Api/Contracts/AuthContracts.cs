namespace RoutArr.Api.Contracts;

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
    string TenantDisplayName,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements);

public sealed record RoutArrSessionBootstrapResponse(
    Guid UserId,
    Guid PersonId,
    Guid TenantId,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasRoutArrEntitlement,
    IReadOnlyList<string> Entitlements);

public sealed record RoutArrMeResponse(
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasRoutArrEntitlement,
    IReadOnlyList<string> Entitlements);
