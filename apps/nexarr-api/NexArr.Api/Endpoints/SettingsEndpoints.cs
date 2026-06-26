using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapSettingsEndpoints(this WebApplication app)
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

            group.MapGet("/", async (
                HttpContext context,
                PlatformAuthorizationService authorization,
                CancellationToken cancellationToken) =>
            {
                await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);

                var response = new NexArrSettingsManifestResponse(
                [
                    new("platform_session_settings", "/api/platform-admin/session-settings", "Access-token, refresh-session, MFA, and password policy settings."),
                    new("platform_service_token_cleanup_settings", "/api/platform/service-token-cleanup/settings", "Service-token cleanup worker cadence and retention settings."),
                    new("platform_outbox_publisher_settings", "/api/platform/outbox-publisher/settings", "Platform outbox publishing worker settings."),
                    new("platform_launch_destination_reconciliation_settings", "/api/platform-admin/launch-destination-reconciliation/settings", "Launch-destination reconciliation worker settings for compatibility audit and support workflows."),
                    new("platform_tenant_lifecycle_settings", "/api/platform/tenant-lifecycle/settings", "Tenant lifecycle worker automation settings."),
                    new("tenant_integrations", "/api/v1/integrations/catalog", "Tenant-scoped external integration catalog, credentials, mappings, sync, and intake routes."),
                    new("fieldcompanion_notification_settings", "/api/v1/mobile/notification-settings", "Field Companion notification defaults and routing settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
