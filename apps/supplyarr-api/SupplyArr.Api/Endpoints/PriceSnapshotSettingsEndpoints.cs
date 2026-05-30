using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PriceSnapshotSettingsEndpoints
{
    public static void MapSupplyArrPriceSnapshotSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("PriceSnapshotSettings").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            PriceSnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePriceSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrPriceSnapshotSettings{nameSuffix}");

        group.MapPut("/", async (
            UpsertPriceSnapshotSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            PriceSnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePriceSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertSupplyArrPriceSnapshotSettings{nameSuffix}");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            PriceSnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePriceSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken));
        })
        .WithName($"ListSupplyArrPendingPriceSnapshotCaptures{nameSuffix}");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            PriceSnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePriceSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrPriceSnapshotRuns{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/price-snapshot-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/price-snapshot-settings"), "V1");
    }
}
