using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class RouteEndpoints
{
    public static void MapRoutArrRouteEndpoints(this WebApplication app)
    {
        var routes = app.MapGroup("/api/routes").WithTags("Routes").RequireAuthorization();

        routes.MapGet("/", async (
            Guid? tripId,
            string? routeStatus,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                tripId,
                routeStatus,
                cancellationToken));
        })
        .WithName("ListRoutes");

        routes.MapGet("/{routeId:guid}", async (
            Guid routeId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var access = await service.GetAccessContextAsync(tenantId, routeId, cancellationToken);
            authorization.RequireRouteAccess(
                context.User,
                access.RouteCreatedByUserId,
                access.TripCreatedByUserId,
                access.TripAssignedDriverPersonId);
            return Results.Ok(await service.GetAsync(tenantId, routeId, cancellationToken));
        })
        .WithName("GetRoute");

        routes.MapPost("/", async (
            CreateRouteRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/routes/{created.RouteId}", created);
        })
        .WithName("CreateRoute");

        routes.MapPatch("/{routeId:guid}/link-trip", async (
            Guid routeId,
            LinkRouteTripRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.LinkTripAsync(
                tenantId,
                actorUserId,
                routeId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("LinkRouteTrip");

        routes.MapPatch("/{routeId:guid}/status", async (
            Guid routeId,
            UpdateRouteStatusRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateRouteStatusAsync(
                tenantId,
                actorUserId,
                routeId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateRouteStatus");

        routes.MapPut("/{routeId:guid}/stops/reorder", async (
            Guid routeId,
            ReorderRouteStopsRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.ReorderStopsAsync(
                tenantId,
                actorUserId,
                routeId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("ReorderRouteStops");

        routes.MapPost("/{routeId:guid}/stops", async (
            Guid routeId,
            AddRouteStopRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.AddStopAsync(
                tenantId,
                actorUserId,
                routeId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("AddRouteStop");

        var stops = app.MapGroup("/api/stops").WithTags("Stops").RequireAuthorization();

        stops.MapGet("/", async (
            Guid routeId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var access = await service.GetAccessContextAsync(tenantId, routeId, cancellationToken);
            authorization.RequireRouteAccess(
                context.User,
                access.RouteCreatedByUserId,
                access.TripCreatedByUserId,
                access.TripAssignedDriverPersonId);
            return Results.Ok(await service.ListStopsAsync(tenantId, routeId, cancellationToken));
        })
        .WithName("ListRouteStops");

        stops.MapPatch("/{stopId:guid}/status", async (
            Guid stopId,
            UpdateRouteStopStatusRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStopsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var canManageAny = authorization.CanViewAllTrips(context.User)
                && !string.Equals(context.User.GetTenantRoleKey(), "routarr_driver", StringComparison.OrdinalIgnoreCase);

            var updated = await service.UpdateStopStatusAsync(
                tenantId,
                actorUserId,
                stopId,
                request,
                canManageAny,
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateRouteStopStatus");
    }
}
