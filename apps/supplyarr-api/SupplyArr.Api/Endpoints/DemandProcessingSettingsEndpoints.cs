using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class DemandProcessingSettingsEndpoints
{
    public static void MapSupplyArrDemandProcessingSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("DemandProcessingSettings").RequireAuthorization();

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
        .WithName($"GetSupplyArrDemandProcessingSettings{nameSuffix}");

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
        .WithName($"UpsertSupplyArrDemandProcessingSettings{nameSuffix}");

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
        .WithName($"ListSupplyArrPendingDemandProcessing{nameSuffix}");

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
        .WithName($"ListSupplyArrDemandProcessingRuns{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/demand-processing-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/demand-processing-settings"), "V1");
    }
}
