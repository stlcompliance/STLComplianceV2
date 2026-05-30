using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapSupplyArrNotificationSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("NotificationSettings").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            ProcurementNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrNotificationSettings{nameSuffix}");

        group.MapPut("/", async (
            UpsertProcurementNotificationSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            ProcurementNotificationSettingsService settingsService,
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
        .WithName($"UpsertSupplyArrNotificationSettings{nameSuffix}");

        group.MapGet("/dispatches", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            ProcurementNotificationDispatchService dispatchService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrNotificationDispatches{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/notification-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/notification-settings"), "V1");
    }
}
