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
                    new("platform_entitlement_reconciliation_settings", "/api/platform/entitlement-reconciliation/settings", "Entitlement reconciliation worker settings."),
                    new("platform_tenant_lifecycle_settings", "/api/platform/tenant-lifecycle/settings", "Tenant lifecycle worker automation settings."),
                    new("companion_notification_settings", "/api/v1/mobile/notification-settings", "Field Companion notification defaults and routing settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
