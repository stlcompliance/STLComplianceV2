using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapRoutArrNotificationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notification-settings")
            .WithTags("NotificationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            DispatchNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetRoutArrNotificationSettings");

        group.MapPut("/", async (
            UpsertDispatchNotificationSettingsRequest request,
            RoutArrAuthorizationService authorization,
            DispatchNotificationSettingsService settingsService,
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
        })
        .WithName("UpsertRoutArrNotificationSettings");

        group.MapGet("/dispatches", async (
            int? limit,
            RoutArrAuthorizationService authorization,
            DispatchNotificationDispatchService dispatchService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListRoutArrNotificationDispatches");
    }
}
