using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public static class PlatformSessionSettingsRules
{
    public const int DefaultAccessTokenMinutes = 15;
    public const int DefaultRefreshTokenDays = 7;

    public const int MinAccessTokenMinutes = 5;
    public const int MaxAccessTokenMinutes = 480;
    public const int MinRefreshTokenDays = 1;
    public const int MaxRefreshTokenDays = 90;
    public const int MinRememberedRefreshTokenDays = 1;
    public const int MaxRememberedRefreshTokenDays = 365;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    public static int ResolveConfiguredAccessTokenMinutes(StlJwtOptions options) =>
        NormalizeAccessTokenMinutes(null, options.AccessTokenMinutes > 0
            ? options.AccessTokenMinutes
            : DefaultAccessTokenMinutes);

    public static int ResolveConfiguredRefreshTokenDays(StlJwtOptions options) =>
        NormalizeRefreshTokenDays(null, options.RefreshTokenDays > 0
            ? options.RefreshTokenDays
            : DefaultRefreshTokenDays);

    public static int ResolveConfiguredRememberedRefreshTokenDays(
        StlJwtOptions options,
        int refreshTokenDays) =>
        NormalizeRememberedRefreshTokenDays(
            options.RememberedRefreshTokenDays,
            refreshTokenDays,
            refreshTokenDays);

    public static bool ResolveConfiguredRequirePlatformAdminMfa(StlJwtOptions options)
    {
        var configuredValue =
            Environment.GetEnvironmentVariable("AUTH_REQUIRE_PLATFORM_ADMIN_MFA")
            ?? Environment.GetEnvironmentVariable("Auth__RequirePlatformAdminMfa")
            ?? options.RequirePlatformAdminMfa?.ToString()
            ?? string.Empty;

        return bool.TryParse(configuredValue, out var requireMfa) && requireMfa;
    }

    public static int NormalizePasswordMinLength(int passwordMinLength, int fallbackLength) =>
        Math.Clamp(
            passwordMinLength > 0 ? passwordMinLength : fallbackLength,
            MinPasswordLength,
            MaxPasswordLength);

    public static int NormalizeAccessTokenMinutes(int? accessTokenMinutes, int fallbackMinutes) =>
        Math.Clamp(
            ResolvePositive(accessTokenMinutes, fallbackMinutes),
            MinAccessTokenMinutes,
            MaxAccessTokenMinutes);

    public static int NormalizeRefreshTokenDays(int? refreshTokenDays, int fallbackDays) =>
        Math.Clamp(
            ResolvePositive(refreshTokenDays, fallbackDays),
            MinRefreshTokenDays,
            MaxRefreshTokenDays);

    public static int NormalizeRememberedRefreshTokenDays(
        int? rememberedRefreshTokenDays,
        int refreshTokenDays,
        int fallbackDays)
    {
        var normalized = Math.Clamp(
            ResolvePositive(rememberedRefreshTokenDays, fallbackDays),
            MinRememberedRefreshTokenDays,
            MaxRememberedRefreshTokenDays);

        return Math.Clamp(
            Math.Max(normalized, refreshTokenDays),
            MinRememberedRefreshTokenDays,
            MaxRememberedRefreshTokenDays);
    }

    private static int ResolvePositive(int? value, int fallback) =>
        value is > 0 ? value.Value : fallback;
}
