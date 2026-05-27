using StaffArr.Api.Contracts;
using StaffArr.Api.Services;

namespace StaffArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapStaffArrAuthEndpoints(this WebApplication app)
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
        .WithName("StaffArrRedeemHandoff");

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            StaffArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("StaffArrGetSessionBootstrap");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            StaffArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("StaffArrGetMe");
    }
}
