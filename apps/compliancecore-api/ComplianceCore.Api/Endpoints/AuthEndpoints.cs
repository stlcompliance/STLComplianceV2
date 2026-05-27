using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;

namespace ComplianceCore.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapComplianceCoreAuthEndpoints(this WebApplication app)
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
        .WithName("ComplianceCoreRedeemHandoff");

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithName("ComplianceCoreGetSessionBootstrap");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireComplianceCoreEntitlement(context.User);
            return Results.Ok(await service.GetMeAsync(context.User, cancellationToken));
        })
        .WithName("ComplianceCoreGetMe");
    }
}
