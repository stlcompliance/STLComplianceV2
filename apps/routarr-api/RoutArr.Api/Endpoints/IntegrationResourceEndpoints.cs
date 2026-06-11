using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class IntegrationResourceEndpoints
{
    public static void MapRoutArrIntegrationResourceEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("Integrations").RequireAuthorization();

        MapDispatchPlanRoutes(group, nameSuffix);
        MapRouteRoutes(group, nameSuffix);
        MapTripRoutes(group, nameSuffix);
        MapStopRoutes(group, nameSuffix);
        MapProofRoutes(group, nameSuffix);
        MapExceptionRoutes(group, nameSuffix);
        MapEtaRoutes(group, nameSuffix);
    }

    private static void MapDispatchPlanRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/dispatch-plans", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchPlanService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(tenantId, viewAll, actorPersonId, cancellationToken));
        })
        .WithName($"ListDispatchPlans{nameSuffix}");

        group.MapGet("/dispatch-plans/{dispatchPlanId:guid}", async (
            Guid dispatchPlanId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchPlanService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, dispatchPlanId, cancellationToken));
        })
        .WithName($"GetDispatchPlan{nameSuffix}");

        group.MapPost("/dispatch-plans", async (
            CreateDispatchPlanRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchPlanService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesCreate(context.User);
            return Results.Ok(await service.CreateAsync(context.User, request, cancellationToken));
        })
        .WithName($"CreateDispatchPlan{nameSuffix}");
    }

    private static void MapRouteRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/routes", async (
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
        .WithName($"ListRoutes{nameSuffix}Integration");

        group.MapGet("/routes/{routeId:guid}", async (
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
        .WithName($"GetRoute{nameSuffix}Integration");

        group.MapPost("/routes", async (
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
            return Results.Created($"/api/v1/integrations/routes/{created.RouteId}", created);
        })
        .WithName($"CreateRoute{nameSuffix}Integration");

        group.MapPost("/routes/{routeId:guid}/release", async (
            Guid routeId,
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
                new UpdateRouteStatusRequest(RouteStatuses.Active),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"ReleaseRoute{nameSuffix}Integration");

        group.MapPost("/routes/{routeId:guid}/cancel", async (
            Guid routeId,
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
                new UpdateRouteStatusRequest(RouteStatuses.Cancelled),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"CancelRoute{nameSuffix}Integration");
    }

    private static void MapTripRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/trips", async (
            string? dispatchStatus,
            Guid? vendorOrderId,
            Guid? brokerOrderId,
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
                vendorOrderId,
                brokerOrderId,
                cancellationToken));
        })
        .WithName($"ListTrips{nameSuffix}Integration");

        group.MapGet("/trips/{tripId:guid}", async (
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
        .WithName($"GetTrip{nameSuffix}Integration");

        group.MapPost("/trips", async (
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
            return Results.Created($"/api/v1/integrations/trips/{created.TripId}", created);
        })
        .WithName($"CreateTrip{nameSuffix}Integration");

        group.MapPost("/trips/{tripId:guid}/assign-driver", async (
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
            var updated = await service.AssignDriverAsync(tenantId, actorUserId, tripId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"AssignTripDriver{nameSuffix}Integration");

        group.MapPost("/trips/{tripId:guid}/assign-equipment", async (
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
            var updated = await service.AssignVehicleAsync(tenantId, actorUserId, tripId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"AssignTripEquipment{nameSuffix}Integration");

        group.MapPost("/trips/{tripId:guid}/start", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateDispatchStatusAsync(
                tenantId,
                actorUserId,
                tripId,
                new UpdateTripDispatchStatusRequest(TripDispatchStatuses.InProgress),
                authorization.CanViewAllTrips(context.User),
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"StartTrip{nameSuffix}Integration");

        group.MapPost("/trips/{tripId:guid}/complete", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateDispatchStatusAsync(
                tenantId,
                actorUserId,
                tripId,
                new UpdateTripDispatchStatusRequest(TripDispatchStatuses.Completed),
                authorization.CanViewAllTrips(context.User),
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"CompleteTrip{nameSuffix}Integration");

        group.MapPost("/trips/{tripId:guid}/cancel", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateDispatchStatusAsync(
                tenantId,
                actorUserId,
                tripId,
                new UpdateTripDispatchStatusRequest(TripDispatchStatuses.Cancelled),
                true,
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"CancelTrip{nameSuffix}Integration");
    }

    private static void MapStopRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/stops/{stopId:guid}", async (
            Guid stopId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutesRead(context.User);
            var tenantId = context.User.GetTenantId();
            var access = await service.GetStopAccessContextAsync(tenantId, stopId, cancellationToken);
            authorization.RequireRouteAccess(
                context.User,
                access.RouteCreatedByUserId,
                access.TripCreatedByUserId,
                access.TripAssignedDriverPersonId);
            return Results.Ok(await service.GetStopAsync(tenantId, stopId, cancellationToken));
        })
        .WithName($"GetStop{nameSuffix}Integration");

        group.MapPost("/stops", async (
            CreateIntegrationRouteStopRequest request,
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
                request.RouteId,
                new AddRouteStopRequest(
                    request.StopKey,
                    request.Label,
                    request.AddressLabel,
                    request.StopType,
                    request.SequenceNumber,
                    request.GeofenceAnchorLatitude,
                    request.GeofenceAnchorLongitude,
                    request.GeofenceRadiusMeters,
                    request.ScheduledArrivalAt,
                    request.StaffarrSiteOrgUnitId),
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"CreateStop{nameSuffix}Integration");

        MapStopTransition(group, nameSuffix, "arrive", RouteStopStatuses.Arrived);
        MapStopTransition(group, nameSuffix, "depart", RouteStopStatuses.EnRoute);
        MapStopTransition(group, nameSuffix, "complete", RouteStopStatuses.Completed);
        MapStopTransition(group, nameSuffix, "fail", RouteStopStatuses.Failed);
    }

    private static void MapStopTransition(RouteGroupBuilder group, string nameSuffix, string path, string status)
    {
        group.MapPost($"/stops/{{stopId:guid}}/{path}", async (
            Guid stopId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireStopsPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var updated = await service.UpdateStopStatusAsync(
                tenantId,
                actorUserId,
                stopId,
                new UpdateRouteStopStatusRequest(status),
                authorization.CanViewAllTrips(context.User),
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateStop{char.ToUpperInvariant(path[0])}{path[1..]}{nameSuffix}Integration");
    }

    private static void MapProofRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapPost("/proof-events", async (
            CreateProofEventRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripProofWrite(context.User);
            var result = await service.CreateProofAsync(
                context.User,
                request.TripId,
                new CreateTripProofRequest(
                    request.ProofType,
                    request.VehicleRefKey,
                    request.ReferenceKey,
                    request.Notes,
                    request.CapturedAt),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"CreateProofEvent{nameSuffix}Integration");

        group.MapGet("/proof-events/{proofEventId:guid}", async (
            Guid proofEventId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripProofRead(context.User);
            return Results.Ok(await service.GetProofByIdAsync(context.User, proofEventId, cancellationToken));
        })
        .WithName($"GetProofEvent{nameSuffix}Integration");
    }

    private static void MapExceptionRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapPost("/exceptions", async (
            CreateDispatchExceptionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.CreateAsync(context.User, request, cancellationToken));
        })
        .WithName($"CreateException{nameSuffix}Integration");

        group.MapGet("/exceptions/{exceptionId:guid}", async (
            Guid exceptionId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionRead(context.User);
            return Results.Ok(await service.GetAsync(context.User, exceptionId, cancellationToken));
        })
        .WithName($"GetException{nameSuffix}Integration");

        group.MapPost("/exceptions/{exceptionId:guid}/resolve", async (
            Guid exceptionId,
            ResolveDispatchExceptionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.ResolveAsync(context.User, exceptionId, request, cancellationToken));
        })
        .WithName($"ResolveException{nameSuffix}Integration");

        group.MapPost("/exceptions/{exceptionId:guid}/close", async (
            Guid exceptionId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.CloseAsync(context.User, exceptionId, null, cancellationToken));
        })
        .WithName($"CloseException{nameSuffix}Integration");
    }

    private static void MapEtaRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/eta/{tripId:guid}", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripEtaService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var eta = await service.GetAsync(tenantId, tripId, cancellationToken);
            return Results.Ok(eta);
        })
        .WithName($"GetTripEta{nameSuffix}Integration");

        group.MapPost("/eta-updates", async (
            UpdateTripEtaRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripEtaService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsManage(context.User);
            return Results.Ok(await service.UpdateAsync(context.User, request, cancellationToken));
        })
        .WithName($"UpdateTripEta{nameSuffix}Integration");
    }
}
