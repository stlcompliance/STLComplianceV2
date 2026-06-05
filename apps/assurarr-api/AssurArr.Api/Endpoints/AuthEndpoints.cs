using AssurArr.Api.Contracts;
using AssurArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace AssurArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAssurArrAuthEndpoints(this WebApplication app)
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
            .WithName("AssurArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("AssurArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", (HttpContext context) =>
        {
            if (!context.User.HasProductEntitlement("assurarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "assurarr",
                hasAssurArrEntitlement = true,
                entitlements = context.User.GetEntitlements()
            });
        })
        .WithName("AssurArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", (HttpContext context) =>
        {
            if (!context.User.HasProductEntitlement("assurarr"))
            {
                return Results.Forbid();
            }

            return Results.Ok(new
            {
                userId = context.User.GetUserId(),
                personId = context.User.GetPersonId(),
                tenantId = context.User.GetTenantId(),
                sessionId = context.User.GetSessionId(),
                tenantRoleKey = context.User.GetTenantRoleKey(),
                isPlatformAdmin = context.User.IsPlatformAdmin(),
                productKey = "assurarr",
                hasAssurArrEntitlement = true,
                entitlements = context.User.GetEntitlements()
            });
        })
        .WithName("AssurArrGetSessionBootstrapV1");
    }
}
