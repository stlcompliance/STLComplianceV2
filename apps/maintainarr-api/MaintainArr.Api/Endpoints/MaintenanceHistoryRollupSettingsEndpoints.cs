using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceHistoryRollupSettingsEndpoints
{
    public static void MapMaintainArrMaintenanceHistoryRollupSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/maintenance-history-rollup-settings"),
            app.MapGroup("/api/v1/maintenance-history-rollup-settings"),
        };

        foreach (var group in groups)
        {
            group.WithTags("MaintenanceHistoryRollupSettings").RequireAuthorization();

            group.MapGet("/", async (
                MaintainArrAuthorizationService authorization,
                MaintenanceHistoryRollupSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintenanceHistoryRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            });

            group.MapPut("/", async (
                UpsertMaintenanceHistoryRollupSettingsRequest request,
                MaintainArrAuthorizationService authorization,
                MaintenanceHistoryRollupSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintenanceHistoryRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            });

            group.MapGet("/pending", async (
                MaintainArrAuthorizationService authorization,
                MaintenanceHistoryRollupWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintenanceHistoryRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var result = await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken);
                return Results.Ok(result);
            });

            group.MapGet("/runs", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintenanceHistoryRollupWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireMaintenanceHistoryRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
            });
        }
    }
}
