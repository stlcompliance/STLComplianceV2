using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TmsRuntimeEndpoints
{
    public static void MapRoutArrTmsRuntimeEndpoints(this WebApplication app)
    {
        MapTransportationDemandRoutes(app.MapGroup("/api/transportation-demands"), string.Empty);
        MapTransportationDemandRoutes(app.MapGroup("/api/v1/transportation-demands"), "V1");
        MapTenderRoutes(app.MapGroup("/api/tenders"), string.Empty);
        MapTenderRoutes(app.MapGroup("/api/v1/tenders"), "V1");
        MapFreightRatingRoutes(app.MapGroup("/api/freight-ratings"), string.Empty);
        MapFreightRatingRoutes(app.MapGroup("/api/v1/freight-ratings"), "V1");
        MapVisibilityRoutes(app.MapGroup("/api/visibility-events"), string.Empty);
        MapVisibilityRoutes(app.MapGroup("/api/v1/visibility-events"), "V1");
        MapPlanningRoutes(app.MapGroup("/api/planning"), string.Empty);
        MapPlanningRoutes(app.MapGroup("/api/v1/planning"), "V1");
        MapCapacityRoutes(app.MapGroup("/api/capacity"), string.Empty);
        MapCapacityRoutes(app.MapGroup("/api/v1/capacity"), "V1");
        MapYardRoutes(app.MapGroup("/api/yard"), string.Empty);
        MapYardRoutes(app.MapGroup("/api/v1/yard"), "V1");
        MapCollaborationRoutes(app.MapGroup("/api/collaboration"), string.Empty);
        MapCollaborationRoutes(app.MapGroup("/api/v1/collaboration"), "V1");
        MapClaimRoutes(app.MapGroup("/api/freight-claims"), string.Empty);
        MapClaimRoutes(app.MapGroup("/api/v1/freight-claims"), "V1");
        MapDocumentPacketRoutes(app.MapGroup("/api/document-packets"), string.Empty);
        MapDocumentPacketRoutes(app.MapGroup("/api/v1/document-packets"), "V1");
        MapFinanceRoutes(app.MapGroup("/api/finance-packet-contributions"), string.Empty);
        MapFinanceRoutes(app.MapGroup("/api/v1/finance-packet-contributions"), "V1");
    }

    private static void MapTransportationDemandRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationDemands").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            string? sourceProduct,
            Guid? tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListDemandsAsync(
                context.User.GetTenantId(),
                status,
                sourceProduct,
                tripId,
                cancellationToken));
        }).WithName($"ListTransportationDemands{nameSuffix}");

        group.MapGet("/{demandId:guid}", async (
            Guid demandId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.GetDemandAsync(context.User.GetTenantId(), demandId, cancellationToken));
        }).WithName($"GetTransportationDemand{nameSuffix}");

        group.MapPost("/", async (
            CreateTransportationDemandRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandCreate(context.User);
            var created = await service.CreateDemandAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/transportation-demands/{created.TransportationDemandId}", created);
        }).WithName($"CreateTransportationDemand{nameSuffix}");

        group.MapPatch("/{demandId:guid}/status", async (
            Guid demandId,
            UpdateTransportationDemandStatusRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationPlanning(context.User);
            return Results.Ok(await service.UpdateDemandStatusAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                demandId,
                request,
                cancellationToken));
        }).WithName($"UpdateTransportationDemandStatus{nameSuffix}");

        group.MapPatch("/{demandId:guid}/link-trip", async (
            Guid demandId,
            LinkTransportationDemandTripRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationPlanning(context.User);
            return Results.Ok(await service.LinkDemandAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                demandId,
                request,
                cancellationToken));
        }).WithName($"LinkTransportationDemandTrip{nameSuffix}");
    }

    private static void MapTenderRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("Tenders").RequireAuthorization();
        group.MapGet("/", async (
            Guid? transportationDemandId,
            string? status,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListTendersAsync(context.User.GetTenantId(), transportationDemandId, status, cancellationToken));
        }).WithName($"ListTenders{nameSuffix}");

        group.MapPost("/", async (
            CreateCarrierTenderRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationTenderManage(context.User);
            var created = await service.CreateTenderAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/tenders/{created.TenderId}", created);
        }).WithName($"CreateTender{nameSuffix}");

        group.MapPatch("/{tenderId:guid}/status", async (
            Guid tenderId,
            UpdateTenderStatusRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationTenderManage(context.User);
            return Results.Ok(await service.UpdateTenderStatusAsync(context.User.GetTenantId(), context.User.GetUserId(), tenderId, request, cancellationToken));
        }).WithName($"UpdateTenderStatus{nameSuffix}");
    }

    private static void MapFreightRatingRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("FreightRatings").RequireAuthorization();
        group.MapGet("/", async (
            Guid? transportationDemandId,
            Guid? tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListFreightRatingsAsync(context.User.GetTenantId(), transportationDemandId, tripId, cancellationToken));
        }).WithName($"ListFreightRatings{nameSuffix}");

        group.MapPost("/", async (
            CreateFreightRatingRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationRatingManage(context.User);
            var created = await service.CreateFreightRatingAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/freight-ratings/{created.FreightRatingId}", created);
        }).WithName($"CreateFreightRating{nameSuffix}");

        group.MapPost("/{freightRatingId:guid}/accessorials", async (
            Guid freightRatingId,
            CreateFreightAccessorialRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationRatingManage(context.User);
            return Results.Ok(await service.CreateAccessorialAsync(
                context.User.GetTenantId(),
                context.User.GetUserId(),
                freightRatingId,
                request,
                cancellationToken));
        }).WithName($"CreateFreightAccessorial{nameSuffix}");
    }

    private static void MapVisibilityRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("VisibilityEvents").RequireAuthorization();
        group.MapGet("/", async (
            Guid? transportationDemandId,
            Guid? tripId,
            string? reviewStatus,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListVisibilityEventsAsync(context.User.GetTenantId(), transportationDemandId, tripId, reviewStatus, cancellationToken));
        }).WithName($"ListVisibilityEvents{nameSuffix}");

        group.MapPost("/", async (
            CreateVisibilityEventRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationVisibilityWrite(context.User);
            var created = await service.CreateVisibilityEventAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/visibility-events/{created.VisibilityEventId}", created);
        }).WithName($"CreateVisibilityEvent{nameSuffix}");
    }

    private static void MapPlanningRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationPlanning").RequireAuthorization();
        group.MapGet("/scenarios", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListPlanningScenariosAsync(context.User.GetTenantId(), cancellationToken));
        }).WithName($"ListPlanningScenarios{nameSuffix}");

        group.MapPost("/scenarios", async (
            CreatePlanningScenarioRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationPlanning(context.User);
            var created = await service.CreatePlanningScenarioAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/planning/scenarios/{created.PlanningScenarioId}", created);
        }).WithName($"CreatePlanningScenario{nameSuffix}");
    }

    private static void MapCapacityRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationCapacity").RequireAuthorization();
        group.MapGet("/driver-snapshots", async (
            string? personId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListCapacitySnapshotsAsync(context.User.GetTenantId(), personId, cancellationToken));
        }).WithName($"ListDriverCapacitySnapshots{nameSuffix}");

        group.MapPost("/driver-snapshots", async (
            CreateDriverCapacitySnapshotRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationPlanning(context.User);
            var created = await service.CreateCapacitySnapshotAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/capacity/driver-snapshots/{created.DriverCapacitySnapshotId}", created);
        }).WithName($"CreateDriverCapacitySnapshot{nameSuffix}");
    }

    private static void MapYardRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationYard").RequireAuthorization();
        group.MapGet("/events", async (
            Guid? transportationDemandId,
            Guid? tripId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListYardEventsAsync(context.User.GetTenantId(), transportationDemandId, tripId, cancellationToken));
        }).WithName($"ListYardEvents{nameSuffix}");

        group.MapPost("/events", async (
            CreateYardEventRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationYardManage(context.User);
            var created = await service.CreateYardEventAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/yard/events/{created.YardEventId}", created);
        }).WithName($"CreateYardEvent{nameSuffix}");
    }

    private static void MapCollaborationRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationCollaboration").RequireAuthorization();
        group.MapGet("/submissions", async (
            string? status,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListCollaborationSubmissionsAsync(context.User.GetTenantId(), status, cancellationToken));
        }).WithName($"ListCollaborationSubmissions{nameSuffix}");

        group.MapPost("/submissions", async (
            CreateCollaborationSubmissionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationVisibilityWrite(context.User);
            var created = await service.CreateCollaborationSubmissionAsync(context.User.GetTenantId(), request, cancellationToken);
            return Results.Created($"/api/collaboration/submissions/{created.SubmissionId}", created);
        }).WithName($"CreateCollaborationSubmission{nameSuffix}");

        group.MapPatch("/submissions/{submissionId:guid}/review", async (
            Guid submissionId,
            ReviewCollaborationSubmissionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationTenderManage(context.User);
            return Results.Ok(await service.ReviewCollaborationSubmissionAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                submissionId,
                request,
                cancellationToken));
        }).WithName($"ReviewCollaborationSubmission{nameSuffix}");
    }

    private static void MapClaimRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("FreightClaims").RequireAuthorization();
        group.MapGet("/", async (
            Guid? transportationDemandId,
            string? status,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListFreightClaimsAsync(context.User.GetTenantId(), transportationDemandId, status, cancellationToken));
        }).WithName($"ListFreightClaims{nameSuffix}");

        group.MapPost("/", async (
            CreateFreightClaimRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationTenderManage(context.User);
            var created = await service.CreateFreightClaimAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/freight-claims/{created.FreightClaimId}", created);
        }).WithName($"CreateFreightClaim{nameSuffix}");
    }

    private static void MapDocumentPacketRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationDocumentPackets").RequireAuthorization();
        group.MapGet("/", async (
            Guid? transportationDemandId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationDemandRead(context.User);
            return Results.Ok(await service.ListDocumentPacketsAsync(context.User.GetTenantId(), transportationDemandId, cancellationToken));
        }).WithName($"ListDocumentPackets{nameSuffix}");

        group.MapPost("/", async (
            CreateDocumentPacketRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationTenderManage(context.User);
            var created = await service.CreateDocumentPacketAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/document-packets/{created.DocumentPacketRequestId}", created);
        }).WithName($"CreateDocumentPacket{nameSuffix}");
    }

    private static void MapFinanceRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("TransportationFinance").RequireAuthorization();
        group.MapGet("/", async (
            string? targetProduct,
            string? status,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationFinanceManage(context.User);
            return Results.Ok(await service.ListFinanceContributionsAsync(context.User.GetTenantId(), targetProduct, status, cancellationToken));
        }).WithName($"ListFinancePacketContributions{nameSuffix}");

        group.MapPost("/", async (
            CreateFinancePacketContributionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            TmsRuntimeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTransportationFinanceManage(context.User);
            var created = await service.CreateFinanceContributionAsync(context.User.GetTenantId(), context.User.GetUserId(), request, cancellationToken);
            return Results.Created($"/api/finance-packet-contributions/{created.FinancePacketContributionId}", created);
        }).WithName($"CreateFinancePacketContribution{nameSuffix}");
    }
}
