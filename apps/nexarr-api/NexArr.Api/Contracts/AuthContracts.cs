namespace NexArr.Api.Contracts;

public sealed record LoginRequest(string Email, string Password, Guid? TenantId);

public sealed record RenewSessionRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid SessionId,
    Guid UserId,
    Guid TenantId);

public sealed record MeResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    bool IsPlatformAdmin,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    IReadOnlyList<string> Entitlements);

public sealed record TenantSummary(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    string RoleKey);

public sealed record EntitlementSummary(
    string ProductKey,
    string DisplayName,
    string Status);

public sealed record NavigationItem(
    string ProductKey,
    string DisplayName,
    string RoutePath,
    int SortOrder);

public sealed record NavigationResponse(
    Guid TenantId,
    IReadOnlyList<NavigationItem> Products);
