using RecordArr.Api.Data;
using STLCompliance.Shared.Auth;

namespace RecordArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapRecordArrAuthEndpoints(this WebApplication app)
    {
        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context, RecordArrStore store) =>
        {
            if (!context.User.HasProductEntitlement("recordarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetEntitlements()));
        }).WithName("RecordArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context, RecordArrStore store) =>
        {
            if (!context.User.HasProductEntitlement("recordarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetEntitlements()));
        }).WithName("RecordArrGetSessionBootstrapV1");
    }
}
