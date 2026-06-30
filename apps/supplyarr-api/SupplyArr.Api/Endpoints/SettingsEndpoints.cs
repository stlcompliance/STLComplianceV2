using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSupplyArrSettingsEndpoints(this WebApplication app)
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
                SupplyArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);

                var response = new SupplyArrSettingsManifestResponse(
                [
                    new("notification_settings", "/api/v1/notification-settings", "Procurement notification dispatch configuration."),
                    new("price_snapshot_settings", "/api/v1/price-snapshot-settings", "Price snapshot worker cadence and controls."),
                    new("lead_time_snapshot_settings", "/api/v1/lead-time-snapshot-settings", "Lead-time snapshot worker settings."),
                    new("availability_snapshot_settings", "/api/v1/availability-snapshot-settings", "Availability snapshot worker settings."),
                    new("procurement_coordination_settings", "/api/v1/procurement-coordination-settings", "Cross-product procurement coordination controls."),
                    new("approval_reminder_settings", "/api/v1/approval-reminder-settings", "Approval reminder scheduling and notification settings."),
                    new("procurement_exception_escalation_settings", "/api/v1/procurement-exception-escalation-settings", "Exception escalation thresholds and behavior."),
                    new("demand_processing_settings", "/api/v1/demand-processing-settings", "Demand processing worker controls and publication settings."),
                    new("integration_event_settings", "/api/v1/integration-event-settings", "Integration outbox and processing worker settings."),
                    new("supplier_order_settings", "/api/v1/supplier-order-settings", "Supplier-order portal visibility and magic-link TTL settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
