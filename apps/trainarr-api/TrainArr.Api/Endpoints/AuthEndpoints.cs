using TrainArr.Api.Contracts;
using TrainArr.Api.Services;

namespace TrainArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapTrainArrAuthEndpoints(this WebApplication app)
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
        .WithName("TrainArrRedeemHandoff");

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            TrainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("TrainArrGetSessionBootstrap");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            TrainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("TrainArrGetMe");
    }
}
