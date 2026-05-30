using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapRoutArrSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/settings"),
            app.MapGroup("/api/v1/settings"),
            app.MapGroup("/api/config"),
            app.MapGroup("/api/v1/config"),
        };

        foreach (var group in groups)
        {
            group.WithTags("Settings").RequireAuthorization();

            group.MapGet("/", (
                RoutArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);

                var response = new RoutArrSettingsManifestResponse(
                [
                    new("notification_settings", "/api/v1/notification-settings", "Dispatch notification dispatch configuration."),
                    new("integration_event_settings", "/api/v1/integration-event-settings", "RoutArr integration outbox event settings."),
                    new("trip_completion_rollup_settings", "/api/v1/trip-completion-rollup-settings", "Trip completion rollup worker settings."),
                    new("attachment_retention_settings", "/api/v1/attachment-retention-settings", "Trip attachment retention worker settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
