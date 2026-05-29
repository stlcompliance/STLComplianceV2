using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripExecutionCaptureEndpoints
{
    public static void MapRoutArrTripExecutionCaptureEndpoints(this WebApplication app)
    {
        var settingsGroup = app.MapGroup("/api/trip-execution-settings")
            .WithTags("TripExecutionCapture")
            .RequireAuthorization();

        settingsGroup.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            TripExecutionCaptureService captureService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await captureService.GetSettingsAsync(tenantId, cancellationToken));
        })
        .WithName("GetRoutArrTripExecutionSettings");

        settingsGroup.MapPut("/", async (
            UpsertTripExecutionSettingsRequest request,
            RoutArrAuthorizationService authorization,
            TripExecutionCaptureService captureService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await captureService.UpsertSettingsAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertRoutArrTripExecutionSettings");
    }
}
