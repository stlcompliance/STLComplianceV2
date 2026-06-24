using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").RequireRateLimiting("NexArrAuthThrottle");
        var v1 = app.MapGroup("/api/v1/auth").WithTags("Auth").RequireRateLimiting("NexArrAuthThrottle");

        static async Task<IResult> LoginEndpoint(
            LoginRequest request,
            HttpContext httpContext,
            AuthService auth,
            CancellationToken cancellationToken)
        {
            var response = await auth.LoginAsync(
                request,
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            if (BrowserSessionCookieService.WantsCookieSession(httpContext.Request))
            {
                BrowserSessionCookieService.SetRefreshTokenCookie(
                    httpContext,
                    response.RefreshToken,
                    response.RefreshTokenExpiresAt);
                return Results.Ok(response with { RefreshToken = string.Empty });
            }

            return Results.Ok(response);
        }

        group.MapPost("/login", LoginEndpoint)
        .AllowAnonymous()
        .WithName("AuthLogin");

        v1.MapPost("/login", LoginEndpoint)
        .AllowAnonymous()
        .WithName("AuthLoginV1");

        static async Task<IResult> RenewEndpoint(
            RenewSessionRequest request,
            HttpContext httpContext,
            AuthService auth,
            CancellationToken cancellationToken)
        {
            var cookieSession = BrowserSessionCookieService.WantsCookieSession(httpContext.Request);
            var refreshToken = !string.IsNullOrWhiteSpace(request.RefreshToken)
                ? request.RefreshToken
                : BrowserSessionCookieService.ReadRefreshToken(httpContext.Request);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new StlApiException("auth.refresh_token_missing", "Refresh token is required.", 401);
            }

            var response = await auth.RenewAsync(new RenewSessionRequest(refreshToken), cancellationToken);

            if (cookieSession || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                BrowserSessionCookieService.SetRefreshTokenCookie(
                    httpContext,
                    response.RefreshToken,
                    response.RefreshTokenExpiresAt);
                return Results.Ok(response with { RefreshToken = string.Empty });
            }

            return Results.Ok(response);
        }

        group.MapPost("/renew", RenewEndpoint)
        .AllowAnonymous()
        .WithName("AuthRenew");

        v1.MapPost("/refresh", RenewEndpoint)
        .AllowAnonymous()
        .WithName("AuthRefreshV1");

        static async Task<IResult> LogoutEndpoint(
            LogoutRequest request,
            HttpContext httpContext,
            AuthService auth,
            CancellationToken cancellationToken)
        {
            var cookieSession = BrowserSessionCookieService.WantsCookieSession(httpContext.Request);
            var refreshToken = !string.IsNullOrWhiteSpace(request.RefreshToken)
                ? request.RefreshToken
                : BrowserSessionCookieService.ReadRefreshToken(httpContext.Request);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await auth.LogoutAsync(new LogoutRequest(refreshToken), cancellationToken);
            }

            if (cookieSession || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                BrowserSessionCookieService.ClearRefreshTokenCookie(httpContext);
            }

            return Results.NoContent();
        }

        group.MapPost("/logout", LogoutEndpoint)
        .AllowAnonymous()
        .WithName("AuthLogout");

        v1.MapPost("/logout", LogoutEndpoint)
        .AllowAnonymous()
        .WithName("AuthLogoutV1");

        static async Task<IResult> ForgotPasswordEndpoint(
            ForgotPasswordRequest request,
            PasswordResetService passwordReset,
            CancellationToken cancellationToken)
        {
            var response = await passwordReset.RequestForgotAsync(request, cancellationToken);
            return Results.Ok(response);
        }

        group.MapPost("/password/forgot", ForgotPasswordEndpoint)
        .AllowAnonymous()
        .WithName("AuthPasswordForgot");

        v1.MapPost("/password/forgot", ForgotPasswordEndpoint)
        .AllowAnonymous()
        .WithName("AuthPasswordForgotV1");

        static async Task<IResult> ResetPasswordEndpoint(
            ResetPasswordRequest request,
            PasswordResetService passwordReset,
            CancellationToken cancellationToken)
        {
            await passwordReset.ResetPasswordAsync(request, cancellationToken);
            return Results.NoContent();
        }

        group.MapPost("/password/reset", ResetPasswordEndpoint)
        .AllowAnonymous()
        .WithName("AuthPasswordReset");

        v1.MapPost("/password/reset", ResetPasswordEndpoint)
        .AllowAnonymous()
        .WithName("AuthPasswordResetV1");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();
        var v1Me = v1.RequireAuthorization();

        static async Task<IResult> GetMeEndpoint(AuthService auth, HttpContext context, CancellationToken cancellationToken)
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
            }

            return Results.Ok(await auth.GetMeAsync(user, cancellationToken));
        }

        me.MapGet("/", GetMeEndpoint)
        .WithName("GetMe");

        v1Me.MapGet("/me", GetMeEndpoint)
        .WithName("GetMeV1");

        static async Task<IResult> UpdateMyPreferencesEndpoint(
            UpdateMyPreferencesRequest request,
            AuthService auth,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            return Results.Ok(await auth.UpdateMyPreferencesAsync(context.User, request, cancellationToken));
        }

        me.MapPatch("/preferences", UpdateMyPreferencesEndpoint)
        .WithName("UpdateMyPreferences");

        static async Task<IResult> UpdateMyPasswordEndpoint(
            UpdateMyPasswordRequest request,
            AuthService auth,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            return Results.Ok(await auth.UpdateMyPasswordAsync(context.User, request, cancellationToken));
        }

        me.MapPost("/password", UpdateMyPasswordEndpoint)
        .WithName("UpdateMyPassword");

        v1Me.MapPatch("/me/preferences", UpdateMyPreferencesEndpoint)
        .WithName("UpdateMyPreferencesV1");

        me.MapGet("/tenants", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await auth.GetMyTenantsAsync(context.User, cancellationToken));
        })
        .WithName("GetMyTenants");

        me.MapGet("/entitlements", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await auth.GetMyEntitlementsAsync(context.User, cancellationToken));
        })
        .WithName("GetMyEntitlements");

        me.MapGet("/navigation", async (
            string? currentProductKey,
            AuthService auth,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await auth.GetNavigationAsync(context.User, currentProductKey, cancellationToken));
        })
        .WithName("GetMyNavigation");

        static async Task<IResult> GetSessionsEndpoint(AuthService auth, HttpContext context, CancellationToken cancellationToken)
        {
            return Results.Ok(await auth.GetMySessionsAsync(context.User, cancellationToken));
        }

        me.MapGet("/sessions", GetSessionsEndpoint)
        .WithName("GetMySessions");

        v1Me.MapGet("/sessions", GetSessionsEndpoint)
        .WithName("GetMySessionsV1");

        static async Task<IResult> RevokeSessionEndpoint(
            Guid sessionId,
            AuthService auth,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            await auth.RevokeMySessionAsync(context.User, sessionId, cancellationToken);
            return Results.NoContent();
        }

        me.MapDelete("/sessions/{sessionId:guid}", RevokeSessionEndpoint)
        .WithName("RevokeMySession");

        v1Me.MapDelete("/sessions/{id:guid}", async (
            Guid id,
            AuthService auth,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            await auth.RevokeMySessionAsync(context.User, id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("RevokeMySessionV1");
    }
}
