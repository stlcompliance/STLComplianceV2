using CustomArr.Api.Data;
using CustomArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace CustomArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapCustomArrAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        static async Task<IResult> RedeemHandoffAsync(
            StlNexArrRedeemHandoffRequest request,
            HandoffAuthService service,
            CancellationToken cancellationToken)
        {
            return Results.Ok(await service.RedeemAsync(request, cancellationToken));
        }

        auth.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("CustomArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("CustomArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context, CustomArrStore store) =>
        {
            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetLaunchableProductKeys()));
        }).WithName("CustomArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context, CustomArrStore store) =>
        {
            return Results.Ok(store.BuildSession(
                context.User.GetUserId().ToString(),
                context.User.GetPersonId().ToString(),
                context.User.GetTenantId().ToString(),
                context.User.GetTenantRoleKey(),
                context.User.IsPlatformAdmin(),
                context.User.GetLaunchableProductKeys()));
        }).WithName("CustomArrGetSessionBootstrapV1");
    }
}

