using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class TripCaptureAttachmentEndpoints
{
    public static void MapTripCaptureAttachmentRoutes(
        RouteGroupBuilder group,
        string subjectType,
        string routePrefix,
        string endpointNamePrefix)
    {
        group.MapGet($"{routePrefix}/{{subjectId:guid}}/attachments", async (
            Guid tripId,
            Guid subjectId,
            HttpContext context,
            TripCaptureAttachmentService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListForSubjectAsync(
                context.User,
                tripId,
                subjectType,
                subjectId,
                cancellationToken)))
        .WithName($"{endpointNamePrefix}ListTrip{routePrefix}Attachments");

        group.MapPost($"{routePrefix}/{{subjectId:guid}}/attachments", async (
            Guid tripId,
            Guid subjectId,
            UploadTripCaptureAttachmentRequest request,
            HttpContext context,
            TripCaptureAttachmentService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UploadAsync(
                context.User,
                tripId,
                subjectType,
                subjectId,
                request,
                cancellationToken)))
        .WithName($"{endpointNamePrefix}UploadTrip{routePrefix}Attachment");

        group.MapGet($"{routePrefix}/{{subjectId:guid}}/attachments/{{attachmentId:guid}}/content", async (
            Guid tripId,
            Guid subjectId,
            Guid attachmentId,
            HttpContext context,
            TripCaptureAttachmentService service,
            CancellationToken cancellationToken) =>
        {
            var (entity, stream) = await service.OpenContentAsync(
                context.User,
                tripId,
                subjectType,
                subjectId,
                attachmentId,
                cancellationToken);
            return Results.File(stream, entity.ContentType, entity.FileName);
        })
        .WithName($"{endpointNamePrefix}DownloadTrip{routePrefix}Attachment");
    }

    public static void MapRoutArrTripCaptureAttachmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/trips/{tripId:guid}")
            .WithTags("TripCaptureAttachments")
            .RequireAuthorization();

        MapTripCaptureAttachmentRoutes(group, TripCaptureAttachmentSubjects.Proof, "/proofs", "RoutArr");
        MapTripCaptureAttachmentRoutes(group, TripCaptureAttachmentSubjects.Dvir, "/dvir", "RoutArr");
    }

    public static void MapRoutArrDriverPortalCaptureAttachmentEndpoints(this RouteGroupBuilder group)
    {
        MapTripCaptureAttachmentRoutes(group, TripCaptureAttachmentSubjects.Proof, "/proofs", "DriverPortal");
        MapTripCaptureAttachmentRoutes(group, TripCaptureAttachmentSubjects.Dvir, "/dvir", "DriverPortal");
    }
}
