using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PriceSnapshotSettingsEndpoints
{
    public static void MapSupplyArrPriceSnapshotSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/price-snapshot-settings")
            .WithTags("PriceSnapshotSettings")
            .RequireAuthorization();

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
        .WithName("GetSupplyArrPriceSnapshotSettings");

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
        .WithName("UpsertSupplyArrPriceSnapshotSettings");

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
        .WithName("ListSupplyArrPendingPriceSnapshotCaptures");

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
        .WithName("ListSupplyArrPriceSnapshotRuns");
    }
}
