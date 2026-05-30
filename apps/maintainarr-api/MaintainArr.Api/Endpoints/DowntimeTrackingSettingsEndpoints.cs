using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DowntimeTrackingSettingsEndpoints
{
    public static void MapMaintainArrDowntimeTrackingSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/downtime-tracking-settings", Suffix: string.Empty),
            (Route: "/api/v1/downtime-tracking-settings", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("DowntimeTrackingSettings")
                .RequireAuthorization();

            group.MapGet("/", async (
                MaintainArrAuthorizationService authorization,
                DowntimeTrackingSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeTrackingSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            })
            .WithName($"GetMaintainArrDowntimeTrackingSettings{suffix}");

            group.MapPut("/", async (
                UpsertDowntimeTrackingSettingsRequest request,
                MaintainArrAuthorizationService authorization,
                DowntimeTrackingSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeTrackingSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"UpsertMaintainArrDowntimeTrackingSettings{suffix}");

            group.MapGet("/pending", async (
                MaintainArrAuthorizationService authorization,
                AssetDowntimeSyncWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeTrackingSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var result = await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"ListMaintainArrPendingDowntimeSync{suffix}");

            group.MapGet("/runs", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeSyncWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeTrackingSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListMaintainArrDowntimeSyncRuns{suffix}");
        }
    }
}
