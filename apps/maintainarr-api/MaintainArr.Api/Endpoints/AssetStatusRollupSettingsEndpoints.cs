using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetStatusRollupSettingsEndpoints
{
    public static void MapMaintainArrAssetStatusRollupSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/asset-status-rollup-settings", Suffix: string.Empty),
            (Route: "/api/v1/asset-status-rollup-settings", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("AssetStatusRollupSettings")
                .RequireAuthorization();

            group.MapGet("/", async (
                MaintainArrAuthorizationService authorization,
                AssetStatusRollupSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetStatusRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
            })
            .WithName($"GetMaintainArrAssetStatusRollupSettings{suffix}");

            group.MapPut("/", async (
                UpsertAssetStatusRollupSettingsRequest request,
                MaintainArrAuthorizationService authorization,
                AssetStatusRollupSettingsService settingsService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetStatusRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await settingsService.UpsertAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"UpsertMaintainArrAssetStatusRollupSettings{suffix}");

            group.MapGet("/pending", async (
                MaintainArrAuthorizationService authorization,
                AssetStatusRollupWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetStatusRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var result = await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken);
                return Results.Ok(result);
            })
            .WithName($"ListMaintainArrPendingAssetStatusRollups{suffix}");

            group.MapGet("/runs", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                AssetStatusRollupWorkerService workerService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetStatusRollupSettingsManage(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
            })
            .WithName($"ListMaintainArrAssetStatusRollupRuns{suffix}");
        }
    }
}

