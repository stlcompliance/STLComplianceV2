namespace NexArr.Api.Contracts;

public sealed record UpsertPlatformSessionSettingsRequest(
    int AccessTokenMinutes,
    int RefreshTokenDays,
    int RememberedRefreshTokenDays);

public sealed record PlatformSessionSettingsResponse(
    int AccessTokenMinutes,
    int RefreshTokenDays,
    int RememberedRefreshTokenDays,
    DateTimeOffset? UpdatedAt);
