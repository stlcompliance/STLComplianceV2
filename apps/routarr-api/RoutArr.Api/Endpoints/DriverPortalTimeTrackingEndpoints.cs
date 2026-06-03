using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class DriverPortalTimeTrackingEndpoints
{
    public static void MapRoutArrDriverPortalTimeTrackingEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("DriverPortal").RequireAuthorization();

        group.MapGet("/time-tracking", async (
            string? date,
            HttpContext context,
            DriverTimeTrackingService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(context.User, date, cancellationToken)))
        .WithName("GetDriverPortalTimeTracking");

        group.MapPost("/time-tracking", async (
            CreateDriverTimeEntryRequest request,
            HttpContext context,
            DriverTimeTrackingService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.CreateAsync(context.User, request, cancellationToken)))
        .WithName("DriverPortalCreateTimeTrackingEntry");

        group.MapPatch("/time-tracking/{entryId:guid}", async (
            Guid entryId,
            UpdateDriverTimeEntryRequest request,
            HttpContext context,
            DriverTimeTrackingService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAsync(context.User, entryId, request, cancellationToken)))
        .WithName("DriverPortalUpdateTimeTrackingEntry");
    }
}
