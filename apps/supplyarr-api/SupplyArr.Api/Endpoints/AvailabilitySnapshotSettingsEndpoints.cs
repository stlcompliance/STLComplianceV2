using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class AvailabilitySnapshotSettingsEndpoints
{
    public static void MapSupplyArrAvailabilitySnapshotSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/availability-snapshot-settings")
            .WithTags("AvailabilitySnapshotSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAvailabilitySnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetSupplyArrAvailabilitySnapshotSettings");

        group.MapPut("/", async (
            UpsertAvailabilitySnapshotSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAvailabilitySnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertSupplyArrAvailabilitySnapshotSettings");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAvailabilitySnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken));
        })
        .WithName("ListSupplyArrPendingAvailabilitySnapshotCaptures");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAvailabilitySnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListSupplyArrAvailabilitySnapshotRuns");
    }
}
