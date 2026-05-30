using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapRoutArrAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        static async Task<IResult> RedeemHandoffAsync(
            RedeemHandoffRequest request,
            HandoffAuthService service,
            CancellationToken cancellationToken)
        {
            return Results.Ok(await service.RedeemAsync(request, cancellationToken));
        }

        auth.MapPost("/handoff/redeem", RedeemHandoffAsync)
        .AllowAnonymous()
        .WithName("RoutArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("RoutArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            RoutArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("RoutArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", async (
            MeService service,
            RoutArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("RoutArrGetSessionBootstrapV1");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            RoutArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("RoutArrGetMe");

        var meV1 = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();
        meV1.MapGet("/", async (
            MeService service,
            RoutArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("RoutArrGetMeV1");
    }
}
