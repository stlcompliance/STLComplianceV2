using LoadArr.Api.Contracts;
using LoadArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace LoadArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapLoadArrAuthEndpoints(this WebApplication app)
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
            .WithName("LoadArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("LoadArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context) =>
        {
            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "loadarr",
                launchableProductKeys = LoadArrSuiteLaunchCatalog.OrdinaryProductKeys
            });
        })
        .WithName("LoadArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context) =>
        {
            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "loadarr",
                launchableProductKeys = LoadArrSuiteLaunchCatalog.OrdinaryProductKeys
            });
        })
        .WithName("LoadArrGetSessionBootstrapV1");
    }
}

