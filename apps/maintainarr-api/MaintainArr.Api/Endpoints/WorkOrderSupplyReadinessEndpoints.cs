using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderSupplyReadinessEndpoints
{
    public static void MapMaintainArrWorkOrderSupplyReadinessEndpoints(this WebApplication app)
    {
        MapRoute(app.MapGroup("/api/work-orders").WithTags("WorkOrderSupplyReadiness").RequireAuthorization(), string.Empty);
        MapRoute(app.MapGroup("/api/v1/work-orders").WithTags("WorkOrderSupplyReadiness").RequireAuthorization(), "V1");
    }

    private static void MapRoute(RouteGroupBuilder group, string routeSuffix)
    {
        group.MapGet("/{workOrderId:guid}/supply-readiness", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderSupplyReadinessService supplyReadinessService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await supplyReadinessService.GetAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"GetWorkOrderSupplyReadiness{routeSuffix}");
    }
}
