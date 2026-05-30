using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;

namespace MaintainArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapMaintainArrSettingsEndpoints(this WebApplication app)
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
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);

                var response = new MaintainArrSettingsManifestResponse(
                [
                    new("notification_settings", "/api/v1/notification-settings", "Maintenance notification dispatch configuration."),
                    new("pm_due_scan_settings", "/api/v1/pm-due-scan-settings", "PM due scan worker cadence and runtime controls."),
                    new("defect_escalation_settings", "/api/v1/defect-escalation-settings", "Defect escalation automation settings."),
                    new("asset_status_rollup_settings", "/api/v1/asset-status-rollup-settings", "Asset status rollup freshness and worker settings."),
                    new("downtime_tracking_settings", "/api/v1/downtime-tracking-settings", "Downtime synchronization and tracking controls."),
                    new("platform_event_settings", "/api/v1/platform-event-settings", "Maintenance platform outbox event processing settings."),
                    new("maintenance_history_rollup_settings", "/api/v1/maintenance-history-rollup-settings", "Maintenance history rollup staleness and processing settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
