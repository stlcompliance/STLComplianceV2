using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapSupplyArrAuthEndpoints(this WebApplication app)
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
        .WithName("SupplyArrRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .WithName("SupplyArrRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("SupplyArrGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", async (
            MeService service,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("SupplyArrGetSessionBootstrapV1");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("SupplyArrGetMe");

        var meV1 = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();
        meV1.MapGet("/", async (
            MeService service,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyArrEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("SupplyArrGetMeV1");

        me.MapGet("/procurement-approval-authority", async (
            StaffarrProcurementApprovalAuthorityService authorityService,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await authorityService.GetMirrorForActorAsync(
                tenantId,
                context.User.GetUserId(),
                context.User.GetPersonId(),
                forceRefresh: false,
                allowStaleOnRefreshFailure: true,
                cancellationToken));
        })
        .WithName("SupplyArrGetProcurementApprovalAuthority");

        meV1.MapGet("/procurement-approval-authority", async (
            StaffarrProcurementApprovalAuthorityService authorityService,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await authorityService.GetMirrorForActorAsync(
                tenantId,
                context.User.GetUserId(),
                context.User.GetPersonId(),
                forceRefresh: false,
                allowStaleOnRefreshFailure: true,
                cancellationToken));
        })
        .WithName("SupplyArrGetProcurementApprovalAuthorityV1");
    }
}
