using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripPartsDemandEndpoints
{
    public static void MapRoutArrTripPartsDemandEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trips/{tripId:guid}/parts-demand")
            .WithTags("TripPartsDemand")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService tripService,
            TripPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await tripService.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedDriverPersonId);
            return Results.Ok(await partsDemandService.ListAsync(tenantId, tripId, cancellationToken));
        })
        .WithName("ListTripPartsDemand");

        group.MapPost("/", async (
            Guid tripId,
            CreateTripPartsDemandLineRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService tripService,
            TripPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await tripService.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedDriverPersonId);
            var created = await partsDemandService.CreateAsync(
                tenantId,
                actorUserId,
                tripId,
                request,
                cancellationToken);
            return Results.Created($"/api/trips/{tripId}/parts-demand/{created.DemandLineId}", created);
        })
        .WithName("CreateTripPartsDemandLine");

        group.MapPost("/publish", async (
            Guid tripId,
            PublishTripPartsDemandRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService tripService,
            TripPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await tripService.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedDriverPersonId);
            return Results.Ok(await partsDemandService.PublishAsync(
                tenantId,
                actorUserId,
                tripId,
                request,
                cancellationToken));
        })
        .WithName("PublishTripPartsDemand");
    }
}
