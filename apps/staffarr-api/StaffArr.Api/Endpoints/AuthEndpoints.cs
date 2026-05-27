using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class AuthEndpoints
{
    private const string ProductKey = "staffarr";

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
        session.MapGet("/", (MeService service, HttpContext context) =>
        {
            EnsureAuthenticated(context.User);
            return Results.Ok(service.GetSessionBootstrap(context.User));
        })
        .WithName("StaffArrGetSessionBootstrap");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", (MeService service, HttpContext context) =>
        {
            EnsureAuthenticated(context.User);
            EnsureStaffArrEntitlement(context.User);
            return Results.Ok(service.GetMe(context.User));
        })
        .WithName("StaffArrGetMe");
    }

    private static void EnsureAuthenticated(System.Security.Claims.ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    private static void EnsureStaffArrEntitlement(System.Security.Claims.ClaimsPrincipal principal)
    {
        if (!principal.HasProductEntitlement(ProductKey))
        {
            throw new StlApiException("auth.not_entitled", "StaffArr entitlement is required.", 403);
        }
    }
}
