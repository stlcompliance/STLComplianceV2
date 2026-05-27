using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            HttpContext httpContext,
            AuthService auth,
            CancellationToken cancellationToken) =>
        {
            var response = await auth.LoginAsync(
                request,
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("AuthLogin");

        group.MapPost("/renew", async (
            RenewSessionRequest request,
            AuthService auth,
            CancellationToken cancellationToken) =>
        {
            var response = await auth.RenewAsync(request, cancellationToken);
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("AuthRenew");

        group.MapPost("/logout", async (
            LogoutRequest request,
            AuthService auth,
            CancellationToken cancellationToken) =>
        {
            await auth.LogoutAsync(request, cancellationToken);
            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithName("AuthLogout");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
            }

            return Results.Ok(await auth.GetMeAsync(user, cancellationToken));
        })
        .WithName("GetMe");

        me.MapGet("/tenants", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var userId = context.User.GetUserId();
            return Results.Ok(await auth.GetMyTenantsAsync(userId, cancellationToken));
        })
        .WithName("GetMyTenants");

        me.MapGet("/entitlements", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await auth.GetMyEntitlementsAsync(tenantId, cancellationToken));
        })
        .WithName("GetMyEntitlements");

        me.MapGet("/navigation", async (AuthService auth, HttpContext context, CancellationToken cancellationToken) =>
        {
            return Results.Ok(await auth.GetNavigationAsync(context.User, cancellationToken));
        })
        .WithName("GetMyNavigation");
    }
}
