using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class ReadinessEndpoints
{
    public static void MapStaffArrReadinessEndpoints(this WebApplication app)
    {
        var readiness = app.MapGroup("/api/readiness")
            .WithTags("Readiness")
            .RequireAuthorization();

        readiness.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessService service,
            CancellationToken cancellationToken) =>
        {
            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new { code = "readiness.validation", message = "personId query parameter is required." });
            }

            authorization.RequireReadinessRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetPersonReadinessAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetPersonReadinessByQuery");

        var personReadiness = app.MapGroup("/api/people/{personId:guid}/readiness")
            .WithTags("Readiness")
            .RequireAuthorization();

        personReadiness.MapGet("/", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetPersonReadinessAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GetPersonReadiness");

        personReadiness.MapPost("/override", async (
            Guid personId,
            GrantReadinessOverrideRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService overrideService,
            ReadinessService readinessService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await overrideService.GrantOverrideAsync(tenantId, actorUserId, personId, request, cancellationToken);
            return Results.Ok(await readinessService.GetPersonReadinessAsync(tenantId, personId, cancellationToken));
        })
        .WithName("GrantPersonReadinessOverride");

        personReadiness.MapDelete("/override", async (
            Guid personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessOverrideService overrideService,
            ReadinessService readinessService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessOverrideWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await overrideService.ClearOverrideAsync(tenantId, actorUserId, personId, cancellationToken);
            return Results.Ok(await readinessService.GetPersonReadinessAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ClearPersonReadinessOverride");
    }
}
