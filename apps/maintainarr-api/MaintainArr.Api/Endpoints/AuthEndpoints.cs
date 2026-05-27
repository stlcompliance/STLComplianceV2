using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;

namespace MaintainArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapMaintainArrAuthEndpoints(this WebApplication app)
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
        .WithName("MaintainArrRedeemHandoff");

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            MaintainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("MaintainArrGetSessionBootstrap");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            MaintainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintainArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("MaintainArrGetMe");
    }
}
