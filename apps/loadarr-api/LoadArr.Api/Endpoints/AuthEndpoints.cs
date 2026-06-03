using STLCompliance.Shared.Auth;

namespace LoadArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapLoadArrAuthEndpoints(this WebApplication app)
    {
        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context) =>
        {
            if (!context.User.HasProductEntitlement("loadarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "loadarr",
                hasLoadArrEntitlement = true,
                entitlements = context.User.GetEntitlements()
            });
        })
        .WithName("LoadArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context) =>
        {
            if (!context.User.HasProductEntitlement("loadarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "loadarr",
                hasLoadArrEntitlement = true,
                entitlements = context.User.GetEntitlements()
            });
        })
        .WithName("LoadArrGetSessionBootstrapV1");
    }
}
