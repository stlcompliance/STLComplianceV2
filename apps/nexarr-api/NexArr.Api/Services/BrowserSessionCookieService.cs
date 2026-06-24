using Microsoft.AspNetCore.Http;

namespace NexArr.Api.Services;

internal static class BrowserSessionCookieService
{
    public const string CookieSessionHeaderName = "X-Stl-Cookie-Session";
    private const string RefreshTokenCookieName = "stl.refresh_token";
    private const string RefreshTokenCookiePath = "/api";

    public static bool WantsCookieSession(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(CookieSessionHeaderName, out var values))
        {
            return false;
        }

        var value = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    public static string? ReadRefreshToken(HttpRequest request) =>
        request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken)
            ? refreshToken
            : null;

    public static void SetRefreshTokenCookie(
        HttpContext context,
        string refreshToken,
        DateTimeOffset expiresAt)
    {
        context.Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = RefreshTokenCookiePath,
                Expires = expiresAt.UtcDateTime,
                IsEssential = true,
            });
    }

    public static void ClearRefreshTokenCookie(HttpContext context) =>
        context.Response.Cookies.Delete(
            RefreshTokenCookieName,
            new CookieOptions
            {
                Path = RefreshTokenCookiePath,
                SameSite = SameSiteMode.Lax,
            });
}
