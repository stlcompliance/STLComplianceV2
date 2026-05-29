using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class OffboardingEndpoints
{
    public static void MapStaffArrOffboardingEndpoints(this WebApplication app)
    {
        var offboarding = app.MapGroup("/api/offboarding").WithTags("Offboarding").RequireAuthorization();

        offboarding.MapPost("/", async (
            StartPersonOffboardingRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonOffboardingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.StartAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/offboarding/{created.OffboardingId}", created);
        })
        .WithName("StartPersonOffboarding");

        offboarding.MapGet("/{offboardingId:guid}", async (
            Guid offboardingId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonOffboardingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByIdAsync(tenantId, offboardingId, cancellationToken));
        })
        .WithName("GetPersonOffboarding");

        offboarding.MapPost("/{offboardingId:guid}/execute", async (
            Guid offboardingId,
            ExecutePersonOffboardingRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonOffboardingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePeopleWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ExecuteAsync(
                tenantId,
                actorUserId,
                offboardingId,
                request,
                cancellationToken));
        })
        .WithName("ExecutePersonOffboarding");

        app.MapGet("/api/people/{personId:guid}/offboarding", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            PersonOffboardingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStaffArrEntitlement(context.User);
            var tenantId = context.User.GetTenantId();
            var currentPersonId = context.User.GetPersonId();
            if (currentPersonId != personId)
            {
                authorization.RequirePeopleRead(context.User);
            }

            var offboarding = await service.GetActiveForPersonAsync(tenantId, personId, cancellationToken);
            return offboarding is null ? Results.NotFound() : Results.Ok(offboarding);
        })
        .WithTags("Offboarding")
        .RequireAuthorization()
        .WithName("GetPersonOffboardingForPerson");
    }
}
