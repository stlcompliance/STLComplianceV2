using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.RateLimiting;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapComplianceCoreAuthEndpoints(this WebApplication app)
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
            .RequireRateLimiting("ComplianceCoreAuthThrottle")
            .WithName("ComplianceCoreRedeemHandoff");
        auth.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .RequireRateLimiting("ComplianceCoreAuthThrottle")
            .ExcludeFromDescription();

        var authV1 = app.MapGroup("/api/v1/auth").WithTags("Auth");
        authV1.MapPost("/handoff/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .RequireRateLimiting("ComplianceCoreAuthThrottle")
            .WithName("ComplianceCoreRedeemHandoffV1");
        authV1.MapPost("/nexarr/redeem", RedeemHandoffAsync)
            .AllowAnonymous()
            .RequireRateLimiting("ComplianceCoreAuthThrottle")
            .ExcludeFromDescription();

        var session = app.MapGroup("/api/session").WithTags("Session").RequireAuthorization();
        session.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            await AuditDeniedPlatformAdminHydrationAsync(
                context,
                audit,
                "session",
                cancellationToken);
            authorization.RequirePlatformAdmin(
                context.User,
                "Compliance Core session hydration requires NexArr-confirmed platform administrator access.");
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, authorization, cancellationToken));
        })
        .WithName("ComplianceCoreGetSessionBootstrap");

        var sessionV1 = app.MapGroup("/api/v1/session").WithTags("Session").RequireAuthorization();
        sessionV1.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            await AuditDeniedPlatformAdminHydrationAsync(
                context,
                audit,
                "session",
                cancellationToken);
            authorization.RequirePlatformAdmin(
                context.User,
                "Compliance Core session hydration requires NexArr-confirmed platform administrator access.");
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, authorization, cancellationToken));
        })
        .WithName("ComplianceCoreGetSessionBootstrapV1");

        var me = app.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        me.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            await AuditDeniedPlatformAdminHydrationAsync(
                context,
                audit,
                "me",
                cancellationToken);
            authorization.RequirePlatformAdmin(
                context.User,
                "Compliance Core user hydration requires NexArr-confirmed platform administrator access.");
            return Results.Ok(await service.GetMeAsync(context.User, authorization, cancellationToken));
        })
        .WithName("ComplianceCoreGetMe");

        var meV1 = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();
        meV1.MapGet("/", async (
            MeService service,
            ComplianceCoreAuthorizationService authorization,
            IComplianceCoreAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            await AuditDeniedPlatformAdminHydrationAsync(
                context,
                audit,
                "me",
                cancellationToken);
            authorization.RequirePlatformAdmin(
                context.User,
                "Compliance Core user hydration requires NexArr-confirmed platform administrator access.");
            return Results.Ok(await service.GetMeAsync(context.User, authorization, cancellationToken));
        })
        .WithName("ComplianceCoreGetMeV1");
    }

    private static async Task AuditDeniedPlatformAdminHydrationAsync(
        HttpContext context,
        IComplianceCoreAuditService audit,
        string targetType,
        CancellationToken cancellationToken)
    {
        var principal = context.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (principal.HasProductEntitlement("compliancecore") && principal.IsPlatformAdmin())
        {
            return;
        }

        var reasonCode = principal.HasProductEntitlement("compliancecore")
            ? "auth.platform_admin_required"
            : "auth.not_entitled";

        await audit.WriteAsync(
            "compliancecore.admin_access.denied",
            principal.GetTenantId(),
            principal.GetUserId(),
            targetType,
            principal.GetSessionId().ToString(),
            "denied",
            reasonCode,
            cancellationToken);
    }
}
