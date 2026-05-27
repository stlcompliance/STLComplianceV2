using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DriverAvailabilityEndpoints
{
    public static void MapRoutArrDriverAvailabilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/driver-availability").WithTags("DriverAvailability").RequireAuthorization();

        group.MapGet("/", async (
            string? personId,
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorPersonId,
                personId,
                scope,
                start,
                end,
                cancellationToken));
        })
        .WithName("ListDriverAvailability");

        group.MapGet("/{availabilityId:guid}", async (
            Guid availabilityId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.GetAsync(
                tenantId,
                availabilityId,
                viewAll,
                actorPersonId,
                cancellationToken));
        })
        .WithName("GetDriverAvailability");

        group.MapPost("/", async (
            CreateDriverAvailabilityRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityWrite(context.User, request.PersonId);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/driver-availability/{created.AvailabilityId}", created);
        })
        .WithName("CreateDriverAvailability");

        group.MapPatch("/{availabilityId:guid}", async (
            Guid availabilityId,
            UpdateDriverAvailabilityRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(
                tenantId,
                availabilityId,
                viewAll,
                actorPersonId,
                cancellationToken);
            authorization.RequireDriverAvailabilityWrite(context.User, existing.PersonId);
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                availabilityId,
                request,
                cancellationToken));
        })
        .WithName("UpdateDriverAvailability");

        group.MapDelete("/{availabilityId:guid}", async (
            Guid availabilityId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(
                tenantId,
                availabilityId,
                viewAll,
                actorPersonId,
                cancellationToken);
            authorization.RequireDriverAvailabilityWrite(context.User, existing.PersonId);
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(tenantId, actorUserId, availabilityId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteDriverAvailability");
    }
}
