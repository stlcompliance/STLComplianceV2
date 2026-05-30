using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class LeadTimeSnapshotSettingsEndpoints
{
    public static void MapSupplyArrLeadTimeSnapshotSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("LeadTimeSnapshotSettings").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireLeadTimeSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrLeadTimeSnapshotSettings{nameSuffix}");

        group.MapPut("/", async (
            UpsertLeadTimeSnapshotSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireLeadTimeSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertSupplyArrLeadTimeSnapshotSettings{nameSuffix}");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireLeadTimeSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken));
        })
        .WithName($"ListSupplyArrPendingLeadTimeSnapshotCaptures{nameSuffix}");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireLeadTimeSnapshotSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrLeadTimeSnapshotRuns{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/lead-time-snapshot-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/lead-time-snapshot-settings"), "V1");
    }
}
