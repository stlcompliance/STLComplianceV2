using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionNotificationEndpoints
{
    public static void MapFieldCompanionNotificationEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/notification-settings", "/api/v1/mobile/notification-settings", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var getSettings = group.MapGet("/", async (
                FieldCompanionNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                FieldCompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            });
            if (isCanonical)
            {
                getSettings.WithName("GetFieldCompanionNotificationSettings");
            }

            var putSettings = group.MapPut("/", async (
                UpsertFieldCompanionNotificationSettingsRequest request,
                FieldCompanionNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                FieldCompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            });
            if (isCanonical)
            {
                putSettings.WithName("UpsertFieldCompanionNotificationSettings");
            }

            var dispatches = group.MapGet("/dispatches", async (
                int? limit,
                FieldCompanionNotificationDispatchService dispatchService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                FieldCompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
            });
            if (isCanonical)
            {
                dispatches.WithName("ListFieldCompanionNotificationDispatches");
            }
        });
    }
}
