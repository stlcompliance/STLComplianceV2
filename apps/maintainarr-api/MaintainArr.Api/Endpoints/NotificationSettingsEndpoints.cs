using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapMaintainArrNotificationSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/notification-settings"),
            app.MapGroup("/api/v1/notification-settings"),
        };

        foreach (var group in groups)
        {
            group
                .WithTags("NotificationSettings")
                .RequireAuthorization();

            group.MapGet("/", async (
                MaintainArrAuthorizationService authorization,
                MaintenanceNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            });

            group.MapPut("/", async (
                UpsertMaintenanceNotificationSettingsRequest request,
                MaintainArrAuthorizationService authorization,
                MaintenanceNotificationSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            });

            group.MapGet("/dispatches", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintenanceNotificationDispatchService dispatchService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
            });
        }
    }
}
