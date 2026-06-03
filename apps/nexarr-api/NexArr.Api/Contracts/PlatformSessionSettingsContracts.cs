namespace NexArr.Api.Contracts;

public sealed record UpsertPlatformSessionSettingsRequest(
    int AccessTokenMinutes,
    int RefreshTokenDays,
    int RememberedRefreshTokenDays,
    bool RequirePlatformAdminMfa,
    int PasswordMinLength,
    bool RequirePasswordComplexity);

public sealed record PlatformSessionSettingsResponse(
    int AccessTokenMinutes,
    int RefreshTokenDays,
    int RememberedRefreshTokenDays,
    bool RequirePlatformAdminMfa,
    int PasswordMinLength,
    bool RequirePasswordComplexity,
    DateTimeOffset? UpdatedAt);
