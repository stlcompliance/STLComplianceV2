using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class DispatchMessageEndpoints
{
    public static void MapRoutArrDispatchMessageEndpoints(this WebApplication app)
    {
        var tripGroup = app.MapGroup("/api/trips/{tripId:guid}/messages")
            .WithTags("DispatchMessages")
            .RequireAuthorization();

        tripGroup.MapGet("/", async (
            Guid tripId,
            HttpContext context,
            DispatchMessageService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetThreadAsync(context.User, tripId, cancellationToken)))
        .WithName("GetTripDispatchMessages");

        tripGroup.MapPost("/", async (
            Guid tripId,
            CreateDispatchMessageRequest request,
            HttpContext context,
            DispatchMessageService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateDispatcherMessageAsync(context.User, tripId, request, cancellationToken)))
        .WithName("CreateTripDispatchMessage");

        var driverGroup = app.MapGroup("/api/driver-portal/trips/{tripId:guid}/messages")
            .WithTags("DriverPortal")
            .RequireAuthorization();

        driverGroup.MapGet("/", async (
            Guid tripId,
            HttpContext context,
            DispatchMessageService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetThreadAsync(context.User, tripId, cancellationToken)))
        .WithName("DriverPortalGetTripDispatchMessages");

        driverGroup.MapPost("/", async (
            Guid tripId,
            CreateDispatchMessageRequest request,
            HttpContext context,
            DispatchMessageService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateDriverMessageAsync(context.User, tripId, request, cancellationToken)))
        .WithName("DriverPortalCreateTripDispatchMessage");

        driverGroup.MapPost("/{messageId:guid}/acknowledge", async (
            Guid tripId,
            Guid messageId,
            HttpContext context,
            DispatchMessageService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.AcknowledgeDriverMessageAsync(
                context.User,
                tripId,
                messageId,
                cancellationToken)))
        .WithName("DriverPortalAcknowledgeTripDispatchMessage");
    }
}
