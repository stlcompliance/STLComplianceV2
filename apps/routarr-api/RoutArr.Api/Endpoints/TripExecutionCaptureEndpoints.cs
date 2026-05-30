using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class TripExecutionCaptureEndpoints
{
    public static void MapRoutArrTripExecutionCaptureEndpoints(this WebApplication app)
    {
        static void MapSettingsRoutes(RouteGroupBuilder settingsGroup, string nameSuffix)
        {
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
            .WithName($"GetRoutArrTripExecutionSettings{nameSuffix}");

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
            .WithName($"UpsertRoutArrTripExecutionSettings{nameSuffix}");
        }

        static void MapTripRoutes(RouteGroupBuilder tripGroup, string nameSuffix)
        {
            tripGroup.MapGet("/capture-readiness", async (
            Guid tripId,
            TripExecutionCaptureService captureService,
            HttpContext context,
            CancellationToken cancellationToken) =>
            Results.Ok(await captureService.GetCaptureReadinessAsync(context.User, tripId, cancellationToken)))
            .WithName($"GetTripCaptureReadiness{nameSuffix}");
        }

        MapSettingsRoutes(
            app.MapGroup("/api/trip-execution-settings")
                .WithTags("TripExecutionCapture")
                .RequireAuthorization(),
            string.Empty);

        MapSettingsRoutes(
            app.MapGroup("/api/v1/trip-execution-settings")
                .WithTags("TripExecutionCapture")
                .RequireAuthorization(),
            "V1");

        MapTripRoutes(
            app.MapGroup("/api/trips/{tripId:guid}")
                .WithTags("TripExecutionCapture")
                .RequireAuthorization(),
            string.Empty);

        MapTripRoutes(
            app.MapGroup("/api/v1/trips/{tripId:guid}")
                .WithTags("TripExecutionCapture")
                .RequireAuthorization(),
            "V1");
    }
}
