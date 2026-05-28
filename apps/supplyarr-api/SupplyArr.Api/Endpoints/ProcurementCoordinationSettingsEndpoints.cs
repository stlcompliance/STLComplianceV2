using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementCoordinationSettingsEndpoints
{
    public static void MapSupplyArrProcurementCoordinationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/procurement-coordination-settings")
            .WithTags("ProcurementCoordinationSettings")
            .RequireAuthorization();

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
        .WithName("GetSupplyArrProcurementCoordinationSettings");

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
        .WithName("UpsertSupplyArrProcurementCoordinationSettings");

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
        .WithName("ListSupplyArrPendingProcurementCoordination");

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
        .WithName("ListSupplyArrProcurementCoordinationRuns");
    }
}
