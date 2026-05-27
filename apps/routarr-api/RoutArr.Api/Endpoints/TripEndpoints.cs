using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripEndpoints
{
    public static void MapRoutArrTripEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trips").WithTags("Trips").RequireAuthorization();

        group.MapGet("/", async (
            string? dispatchStatus,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                dispatchStatus,
                cancellationToken));
        })
        .WithName("ListTrips");

        group.MapGet("/{tripId:guid}", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedDriverPersonId);
            return Results.Ok(detail);
        })
        .WithName("GetTrip");

        group.MapPost("/", async (
            CreateTripRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/trips/{created.TripId}", created);
        })
        .WithName("CreateTrip");

        group.MapPatch("/{tripId:guid}/assign-driver", async (
            Guid tripId,
            AssignTripDriverRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.AssignDriverAsync(
                tenantId,
                actorUserId,
                tripId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("AssignTripDriver");

        group.MapPatch("/{tripId:guid}/assign-vehicle", async (
            Guid tripId,
            AssignTripVehicleRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.AssignVehicleAsync(
                tenantId,
                actorUserId,
                tripId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("AssignTripVehicle");

        group.MapPatch("/{tripId:guid}/status", async (
            Guid tripId,
            UpdateTripDispatchStatusRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedDriverPersonId);

            var canManageAny = authorization.CanViewAllTrips(context.User)
                && !string.Equals(context.User.GetTenantRoleKey(), "routarr_driver", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(request.DispatchStatus, "cancelled", StringComparison.OrdinalIgnoreCase))
            {
                authorization.RequireTripsManage(context.User);
                canManageAny = true;
            }

            var updated = await service.UpdateDispatchStatusAsync(
                tenantId,
                actorUserId,
                tripId,
                request,
                canManageAny,
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateTripDispatchStatus");
    }
}
