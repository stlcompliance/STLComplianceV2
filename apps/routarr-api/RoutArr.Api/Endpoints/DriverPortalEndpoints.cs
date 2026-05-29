using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DriverPortalEndpoints
{
    public static void MapRoutArrDriverPortalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/driver-portal").WithTags("DriverPortal").RequireAuthorization();

        group.MapGet("/schedule", async (
            HttpContext context,
            DriverPortalService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetScheduleAsync(context.User, cancellationToken)))
        .WithName("GetDriverPortalSchedule");

        group.MapPost("/trips/{tripId:guid}/dispatch", async (
            Guid tripId,
            HttpContext context,
            DriverPortalService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.DispatchTripAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalDispatchTrip");

        group.MapPost("/trips/{tripId:guid}/start", async (
            Guid tripId,
            HttpContext context,
            DriverPortalService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.StartTripAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalStartTrip");

        group.MapPost("/trips/{tripId:guid}/complete", async (
            Guid tripId,
            HttpContext context,
            DriverPortalService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CompleteTripAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalCompleteTrip");

        group.MapPost("/trips/{tripId:guid}/close", async (
            Guid tripId,
            HttpContext context,
            DriverPortalService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CloseTripAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalCloseTrip");

        group.MapGet("/trips/{tripId:guid}/execution", async (
            Guid tripId,
            HttpContext context,
            TripProofDvirService proofDvirService,
            CancellationToken cancellationToken) =>
            Results.Ok(await proofDvirService.GetExecutionSummaryAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalTripExecution");

        group.MapPost("/trips/{tripId:guid}/proofs", async (
            Guid tripId,
            CreateTripProofRequest request,
            HttpContext context,
            TripProofDvirService proofDvirService,
            CancellationToken cancellationToken) =>
            Results.Ok(await proofDvirService.CreateProofAsync(context.User, tripId, request, cancellationToken)))
        .WithName("DriverPortalCreateTripProof");

        group.MapPost("/trips/{tripId:guid}/dvir", async (
            Guid tripId,
            SubmitTripDvirRequest request,
            HttpContext context,
            TripProofDvirService proofDvirService,
            CancellationToken cancellationToken) =>
            Results.Ok(await proofDvirService.SubmitDvirAsync(context.User, tripId, request, cancellationToken)))
        .WithName("DriverPortalSubmitTripDvir");

        group.MapGet("/trips/{tripId:guid}/capture-readiness", async (
            Guid tripId,
            HttpContext context,
            TripExecutionCaptureService captureService,
            CancellationToken cancellationToken) =>
            Results.Ok(await captureService.GetCaptureReadinessAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalTripCaptureReadiness");

        var tripGroup = group.MapGroup("/trips/{tripId:guid}");
        tripGroup.MapRoutArrDriverPortalCaptureAttachmentEndpoints();
    }
}
