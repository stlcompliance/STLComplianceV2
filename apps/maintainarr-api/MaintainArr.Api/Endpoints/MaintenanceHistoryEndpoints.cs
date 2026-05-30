using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceHistoryEndpoints
{
    public static void MapMaintainArrMaintenanceHistoryEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/maintenance-history").WithTags("MaintenanceHistory").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/maintenance-history").WithTags("MaintenanceHistory").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid assetId,
            int? page,
            int? pageSize,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenanceHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceHistoryRead(context.User);
            if (assetId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "assetId is required." });
            }

            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                assetId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName($"ListMaintenanceHistory{nameSuffix}");

        group.MapGet("/summary", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenanceHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceHistoryRead(context.User);
            if (assetId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "assetId is required." });
            }

            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, assetId, cancellationToken));
        })
        .WithName($"GetMaintenanceHistorySummary{nameSuffix}");
    }
}
