using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class CompanionNotificationEndpoints
{
    public static void MapCompanionNotificationEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion/notification-settings", "/api/v1/mobile/notification-settings", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var getSettings = group.MapGet("/", async (
                CompanionNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            });
            if (isCanonical)
            {
                getSettings.WithName("GetCompanionNotificationSettings");
            }

            var putSettings = group.MapPut("/", async (
                UpsertCompanionNotificationSettingsRequest request,
                CompanionNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
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
                putSettings.WithName("UpsertCompanionNotificationSettings");
            }

            var dispatches = group.MapGet("/dispatches", async (
                int? limit,
                CompanionNotificationDispatchService dispatchService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                CompanionNotificationAuthorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
            });
            if (isCanonical)
            {
                dispatches.WithName("ListCompanionNotificationDispatches");
            }
        });
    }
}
