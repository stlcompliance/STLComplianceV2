using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class DemandProcessingSettingsEndpoints
{
    public static void MapSupplyArrDemandProcessingSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/demand-processing-settings")
            .WithTags("DemandProcessingSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            DemandProcessingSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetSupplyArrDemandProcessingSettings");

        group.MapPut("/", async (
            UpsertDemandProcessingSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            DemandProcessingSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertSupplyArrDemandProcessingSettings");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            DemandProcessingWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, null, cancellationToken));
        })
        .WithName("ListSupplyArrPendingDemandProcessing");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            DemandProcessingWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListSupplyArrDemandProcessingRuns");
    }
}
