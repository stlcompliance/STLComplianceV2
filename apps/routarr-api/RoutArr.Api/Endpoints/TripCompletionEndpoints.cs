using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripCompletionEndpoints
{
    public static void MapRoutArrTripCompletionEndpoints(this WebApplication app)
    {
        var tripCompletions = app.MapGroup("/api/trip-completions")
            .WithTags("TripCompletions")
            .RequireAuthorization();

        tripCompletions.MapGet("/", async (
            string? dispatchStatus,
            RoutArrAuthorizationService authorization,
            TripCompletionService completionService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var items = await completionService.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                dispatchStatus,
                cancellationToken);
            return Results.Ok(new { items });
        })
        .WithName("ListRoutArrTripCompletions");

        tripCompletions.MapGet("/{tripId:guid}", async (
            Guid tripId,
            RoutArrAuthorizationService authorization,
            TripCompletionService completionService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var trip = await completionService.GetAsync(tenantId, tripId, cancellationToken);

            var viewAll = authorization.CanViewAllTrips(context.User);
            if (!viewAll)
            {
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                var createdByUserId = await GetTripCreatedByUserIdAsync(
                    completionService,
                    tenantId,
                    tripId,
                    cancellationToken);
                authorization.RequireTripAccess(
                    context.User,
                    createdByUserId,
                    trip.Summary.AssignedDriverPersonId);
            }

            return Results.Ok(trip);
        })
        .WithName("GetRoutArrTripCompletion");

        var routeCompletions = app.MapGroup("/api/route-completions")
            .WithTags("RouteCompletions")
            .RequireAuthorization();

        routeCompletions.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            TripCompletionService completionService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var items = await completionService.ListRouteCompletionsAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                cancellationToken);
            return Results.Ok(new { items });
        })
        .WithName("ListRoutArrRouteCompletions");
    }

    private static Task<Guid> GetTripCreatedByUserIdAsync(
        TripCompletionService completionService,
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken) =>
        completionService.GetTripCreatedByUserIdAsync(tenantId, tripId, cancellationToken);
}
