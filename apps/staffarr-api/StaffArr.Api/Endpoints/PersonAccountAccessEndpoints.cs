using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class PersonAccountAccessEndpoints
{
    public static void MapStaffArrPersonAccountAccessEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/people").WithTags("People").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/people").WithTags("People").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder people, string nameSuffix)
    {
        people.MapGet("/{personId:guid}/account-access", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, personId, cancellationToken));
        })
        .WithName($"GetStaffPersonAccountAccess{nameSuffix}");

        people.MapPost("/{personId:guid}/account-access/provision", async (
            Guid personId,
            ProvisionPersonAccountRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountProvision(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ProvisionAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"ProvisionStaffPersonAccountAccess{nameSuffix}");

        people.MapPatch("/{personId:guid}/account-access/login-email", async (
            Guid personId,
            UpdatePersonLoginEmailRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountEdit(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLoginEmailAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"UpdateStaffPersonLoginEmail{nameSuffix}");

        people.MapPost("/{personId:guid}/account-access/password-reset", async (
            Guid personId,
            PersonAccountActionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountSecurityReset(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RequestPasswordResetAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"RequestStaffPersonPasswordReset{nameSuffix}");

        people.MapPost("/{personId:guid}/account-access/mfa-reset", async (
            Guid personId,
            PersonAccountActionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountSecurityReset(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ResetMfaAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"ResetStaffPersonMfa{nameSuffix}");

        people.MapPost("/{personId:guid}/account-access/disable-login", async (
            Guid personId,
            PersonAccountActionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountDisable(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.DisableLoginAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"DisableStaffPersonLogin{nameSuffix}");

        people.MapPost("/{personId:guid}/account-access/enable-login", async (
            Guid personId,
            PersonAccountActionRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonAccountAccessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonAccountEdit(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.EnableLoginAsync(tenantId, personId, actorUserId, request, cancellationToken));
        })
        .WithName($"EnableStaffPersonLogin{nameSuffix}");
    }
}
