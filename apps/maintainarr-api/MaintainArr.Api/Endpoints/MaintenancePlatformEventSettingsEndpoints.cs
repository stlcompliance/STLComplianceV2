using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenancePlatformEventSettingsEndpoints
{
    public static void MapMaintainArrMaintenancePlatformEventSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/platform-event-settings"),
            app.MapGroup("/api/v1/platform-event-settings"),
        };

        foreach (var group in groups)
        {
            group.WithTags("PlatformEventSettings").RequireAuthorization();

            group.MapGet("/", async (
                MaintainArrAuthorizationService authorization,
                MaintenancePlatformEventSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePlatformEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            });

            group.MapPut("/", async (
                UpsertMaintenancePlatformEventSettingsRequest request,
                MaintainArrAuthorizationService authorization,
                MaintenancePlatformEventSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePlatformEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            });

            group.MapGet("/outbox", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintenancePlatformEventProcessingService processingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePlatformEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await processingService.ListRecentAsync(tenantId, limit, cancellationToken));
            });

            group.MapGet("/runs", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintenancePlatformEventProcessingService processingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePlatformEventSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await processingService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
            });
        }
    }
}
