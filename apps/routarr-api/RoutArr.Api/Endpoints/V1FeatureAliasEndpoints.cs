using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class V1FeatureAliasEndpoints
{
    public static void MapRoutArrV1FeatureAliasEndpoints(this WebApplication app)
    {
        MapHealthAndDashboardAliases(app);
        MapCoreEntityAliases(app);
        MapOperationsAliases(app);
        MapComplianceAndAvailabilityAliases(app);
        MapReportAndIntegrationAliases(app);
    }

    private static void MapHealthAndDashboardAliases(WebApplication app)
    {
        app.MapGet("/api/v1/dashboard", async (
            string? scope,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCommandCenterService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            return Results.Ok(await service.GetAsync(context.User, scope, cancellationToken));
        })
        .WithTags("Dispatch")
        .RequireAuthorization()
        .WithName("GetRoutArrDashboardV1");
    }

    private static void MapCoreEntityAliases(WebApplication app)
    {
        app.MapGet("/api/v1/routes", async (
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
        .WithTags("Routes")
        .RequireAuthorization()
        .WithName("ListRoutesV1Alias");

        app.MapGet("/api/v1/trips", async (
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
        .WithTags("Trips")
        .RequireAuthorization()
        .WithName("ListTripsV1Alias");

        app.MapGet("/api/v1/stops", async (
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
        .WithTags("Stops")
        .RequireAuthorization()
        .WithName("ListStopsV1Alias");

        app.MapGet("/api/v1/loads", async (
            Guid tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TripService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, tripId, cancellationToken);
            authorization.RequireTripAccess(context.User, detail.CreatedByUserId, detail.AssignedDriverPersonId);
            return Results.Ok(detail.Loads);
        })
        .WithTags("Trips")
        .RequireAuthorization()
        .WithName("ListTripLoadsV1Alias");

        app.MapGet("/api/v1/route-templates", () => Results.Ok(new
        {
            items = Array.Empty<object>(),
            source = "/api/v1/routes",
            description = "Route templates are represented through reusable route definitions in v1."
        }))
        .WithTags("Routes")
        .RequireAuthorization()
        .WithName("ListRouteTemplatesV1Alias");
    }

    private static void MapOperationsAliases(WebApplication app)
    {
        app.MapGet("/api/v1/dispatch-board", async (
            string? scope,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchBoardService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.GetBoardAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                scope,
                cancellationToken));
        })
        .WithTags("Dispatch")
        .RequireAuthorization()
        .WithName("GetDispatchBoardV1Alias");

        app.MapGet("/api/v1/driver-work", async (
            string? scope,
            bool? attentionOnly,
            string? statusFilter,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            ActiveTripsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            return Results.Ok(await service.GetAsync(
                context.User,
                scope,
                attentionOnly == true,
                statusFilter,
                cancellationToken));
        })
        .WithTags("Dispatch")
        .RequireAuthorization()
        .WithName("GetDriverWorkV1Alias");

        app.MapGet("/api/v1/equipment-assignments", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            VehicleRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithTags("VehicleRefs")
        .RequireAuthorization()
        .WithName("ListEquipmentAssignmentsV1Alias");

        app.MapGet("/api/v1/driver-assignments", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            StaffarrPersonRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithTags("Drivers")
        .RequireAuthorization()
        .WithName("ListDriverAssignmentsV1Alias");

        app.MapGet("/api/v1/proofs", async (
            Guid tripId,
            HttpContext context,
            TripProofDvirService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListProofsAsync(context.User, tripId, cancellationToken)))
        .WithTags("TripProofDvir")
        .RequireAuthorization()
        .WithName("ListProofsV1Alias");

        app.MapGet("/api/v1/exceptions", async (
            string? status,
            bool? overdueOnly,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionRead(context.User);
            return Results.Ok(await service.ListOpenAsync(
                context.User,
                status,
                overdueOnly == true,
                cancellationToken));
        })
        .WithTags("Dispatch")
        .RequireAuthorization()
        .WithName("ListExceptionsV1Alias");

        app.MapGet("/api/v1/incidents", async (
            string? status,
            bool? overdueOnly,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionRead(context.User);
            return Results.Ok(await service.ListOpenAsync(
                context.User,
                status,
                overdueOnly == true,
                cancellationToken));
        })
        .WithTags("Dispatch")
        .RequireAuthorization()
        .WithName("ListIncidentsV1Alias");
    }

    private static void MapComplianceAndAvailabilityAliases(WebApplication app)
    {
        app.MapGet("/api/v1/compliance-checks", () => Results.Ok(new
        {
            items = new[]
            {
                new { key = "driver-eligibility", path = "/api/v1/compliance-checks/driver-eligibility" },
                new { key = "asset-dispatchability", path = "/api/v1/compliance-checks/asset-dispatchability" }
            }
        }))
        .WithTags("ComplianceChecks")
        .RequireAuthorization()
        .WithName("ListComplianceChecksV1Alias");

        app.MapPost("/api/v1/compliance-checks/driver-eligibility", async (
            DriverEligibilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverEligibilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CheckAsync(
                tenantId,
                actorUserId,
                request.PersonId,
                request.QualificationKey,
                request.RulePackKey,
                cancellationToken));
        })
        .WithTags("ComplianceChecks")
        .RequireAuthorization()
        .WithName("CheckDriverEligibilityV1Alias");

        app.MapPost("/api/v1/compliance-checks/asset-dispatchability", async (
            AssetDispatchabilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            AssetDispatchabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CheckAsync(
                tenantId,
                actorUserId,
                request.VehicleRefKey,
                request.AssetTag,
                cancellationToken));
        })
        .WithTags("ComplianceChecks")
        .RequireAuthorization()
        .WithName("CheckAssetDispatchabilityV1Alias");

        app.MapGet("/api/v1/availability", async (
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService driverAvailabilityService,
            EquipmentAvailabilityService equipmentAvailabilityService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();

            var drivers = await driverAvailabilityService.GetPanelAsync(
                tenantId,
                viewAll,
                actorPersonId,
                scope,
                start,
                end,
                cancellationToken);

            var equipment = await equipmentAvailabilityService.GetPanelAsync(
                tenantId,
                scope,
                start,
                end,
                cancellationToken);

            return Results.Ok(new { drivers, equipment });
        })
        .WithTags("Availability")
        .RequireAuthorization()
        .WithName("GetAvailabilityV1Alias");
    }

    private static void MapReportAndIntegrationAliases(WebApplication app)
    {
        app.MapGet("/api/v1/reports", (
            RoutArrAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireDispatchReportRead(context.User);
            var items = new[]
            {
                new { key = "dispatch", path = "/api/reports/dispatch" },
                new { key = "routes", path = "/api/reports/routes" },
                new { key = "proof-dvir", path = "/api/reports/proof-dvir" },
                new { key = "dispatch-overrides", path = "/api/reports/dispatch-overrides" }
            };
            return Results.Ok(new { items });
        })
        .WithTags("Reports")
        .RequireAuthorization()
        .WithName("GetReportsIndexV1Alias");
    }
}
