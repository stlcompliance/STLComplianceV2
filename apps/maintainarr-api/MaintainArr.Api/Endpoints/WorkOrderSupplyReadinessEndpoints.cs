using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderSupplyReadinessEndpoints
{
    public static void MapMaintainArrWorkOrderSupplyReadinessEndpoints(this WebApplication app)
    {
        app.MapGet("/api/work-orders/{workOrderId:guid}/supply-readiness", async (
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
        .WithTags("WorkOrderSupplyReadiness")
        .RequireAuthorization()
        .WithName("GetWorkOrderSupplyReadiness");
    }
}
