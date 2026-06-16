using OrdArr.Api.Data;
using OrdArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace OrdArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapOrdArrAuthEndpoints(this WebApplication app)
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
            .WithName("OrdArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("OrdArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        static IResult GetSession(HttpContext context, OrdArrStore store)
        {
            if (!context.User.HasProductEntitlement("ordarr"))
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
        }

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", GetSession).WithName("OrdArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", GetSession).WithName("OrdArrGetSessionBootstrapV1");
    }
}
