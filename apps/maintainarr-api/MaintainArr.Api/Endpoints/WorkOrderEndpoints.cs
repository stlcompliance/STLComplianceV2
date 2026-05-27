using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderEndpoints
{
    public static void MapMaintainArrWorkOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/work-orders").WithTags("WorkOrders").RequireAuthorization();

        group.MapGet("/", async (
            Guid? assetId,
            Guid? defectId,
            string? status,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllWorkOrders(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await service.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                assetId,
                defectId,
                status,
                cancellationToken));
        })
        .WithName("ListWorkOrders");

        group.MapGet("/{workOrderId:guid}", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(detail);
        })
        .WithName("GetWorkOrder");

        group.MapPost("/", async (
            CreateWorkOrderRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/work-orders/{created.WorkOrderId}", created);
        })
        .WithName("CreateWorkOrder");

        group.MapPatch("/{workOrderId:guid}", async (
            Guid workOrderId,
            UpdateWorkOrderRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            var updated = await service.UpdateAsync(tenantId, actorUserId, workOrderId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateWorkOrder");

        group.MapPatch("/{workOrderId:guid}/status", async (
            Guid workOrderId,
            UpdateWorkOrderStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);

            var canCloseAny = authorization.CanCloseAnyWorkOrder(context.User);
            if (string.Equals(request.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
            {
                authorization.RequireWorkOrdersClose(context.User);
            }

            var updated = await service.UpdateStatusAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                canCloseAny,
                actorPersonId,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdateWorkOrderStatus");

        var defectGroup = app.MapGroup("/api/defects").WithTags("Defects").RequireAuthorization();

        defectGroup.MapPost("/{defectId:guid}/work-orders", async (
            Guid defectId,
            CreateWorkOrderFromDefectRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            DefectService defectService,
            WorkOrderService workOrderService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var defect = await defectService.GetAsync(tenantId, defectId, cancellationToken);
            authorization.RequireDefectAccess(context.User, defect.ReportedByUserId);
            var created = await workOrderService.CreateFromDefectAsync(
                tenantId,
                actorUserId,
                defectId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{created.WorkOrderId}", created);
        })
        .WithName("CreateWorkOrderFromDefect");
    }
}
