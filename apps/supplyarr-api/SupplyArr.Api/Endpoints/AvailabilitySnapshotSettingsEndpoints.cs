using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class AvailabilitySnapshotSettingsEndpoints
{
    public static void MapSupplyArrAvailabilitySnapshotSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("AvailabilitySnapshotSettings").RequireAuthorization();

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
        .WithName($"GetSupplyArrAvailabilitySnapshotSettings{nameSuffix}");

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
        .WithName($"UpsertSupplyArrAvailabilitySnapshotSettings{nameSuffix}");

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
        .WithName($"ListSupplyArrPendingAvailabilitySnapshotCaptures{nameSuffix}");

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
        .WithName($"ListSupplyArrAvailabilitySnapshotRuns{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/availability-snapshot-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/availability-snapshot-settings"), "V1");
    }
}
