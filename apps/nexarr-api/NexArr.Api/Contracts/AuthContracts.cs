namespace NexArr.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password,
    Guid? TenantId,
    bool RememberDevice = false,
    string? MfaCode = null,
    string? RecoveryCode = null);

public sealed record LocalDevBypassLoginRequest(
    string Email,
    Guid? TenantId,
    bool RememberDevice = false);

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
    bool RequiresPasswordChange,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string ThemePreference,
    IReadOnlyList<string> LaunchableProductKeys);

public sealed record UpdateMyPreferencesRequest(string? ThemePreference);

public sealed record UserPreferencesResponse(string ThemePreference);

public sealed record TenantSummary(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    string RoleKey);

public record LaunchableProductSummary(
    string ProductKey,
    string DisplayName,
    string Status);

public sealed record NavigationSurfaceItem(
    string SurfaceKey,
    string Label,
    string RelativePath,
    string IconKey,
    int SortOrder,
    bool IsEnabled,
    string? PermissionHint);

public sealed record NavigationItem(
    string ProductKey,
    string DisplayName,
    string ProductCategory,
    string ProductStatus,
    string RoutePath,
    string LaunchUrl,
    bool IsCurrent,
    int SortOrder,
    IReadOnlyList<NavigationSurfaceItem> Surfaces);

public sealed record NavigationResponse(
    Guid TenantId,
    IReadOnlyList<NavigationItem> Products);

public sealed record UserSessionSummary(
    Guid SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? UserAgent,
    string? IpAddress,
    Guid? ActiveTenantId,
    bool IsCurrent,
    bool IsActive,
    bool IsRemembered);

public sealed record UserSessionsResponse(IReadOnlyList<UserSessionSummary> Sessions);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ForgotPasswordResponse(string Message, string? DevResetToken);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record UpdateMyPasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record UpdateMyPasswordResponse(
    DateTimeOffset PasswordChangedAt);
