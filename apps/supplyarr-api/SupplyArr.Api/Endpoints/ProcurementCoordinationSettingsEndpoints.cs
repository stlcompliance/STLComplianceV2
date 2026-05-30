using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementCoordinationSettingsEndpoints
{
    public static void MapSupplyArrProcurementCoordinationSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("ProcurementCoordinationSettings").RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName($"GetSupplyArrProcurementCoordinationSettings{nameSuffix}");

        group.MapPut("/", async (
            UpsertProcurementCoordinationSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName($"UpsertSupplyArrProcurementCoordinationSettings{nameSuffix}");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken));
        })
        .WithName($"ListSupplyArrPendingProcurementCoordination{nameSuffix}");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName($"ListSupplyArrProcurementCoordinationRuns{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/procurement-coordination-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/procurement-coordination-settings"), "V1");
    }
}
