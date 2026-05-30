using TrainArr.Api.Contracts;
using TrainArr.Api.Services;

namespace TrainArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapTrainArrAuthEndpoints(this WebApplication app)
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
        .WithName("TrainArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("TrainArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

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

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", async (
            MeService service,
            TrainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("TrainArrGetSessionBootstrapV1");

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

        var meV1 = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();
        meV1.MapGet("/", async (
            MeService service,
            TrainArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("TrainArrGetMeV1");
    }
}
