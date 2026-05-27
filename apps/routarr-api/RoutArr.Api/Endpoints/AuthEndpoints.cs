using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapRoutArrAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        auth.MapPost("/handoff/redeem", async (
            RedeemHandoffRequest request,
            HandoffAuthService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RedeemAsync(request, cancellationToken));
        })
        .AllowAnonymous()
        .WithName("RoutArrRedeemHandoff");

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
    }
}
