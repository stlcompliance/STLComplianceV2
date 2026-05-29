using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderPartsDemandEndpoints
{
    public static void MapMaintainArrWorkOrderPartsDemandEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/work-orders/{workOrderId:guid}/parts-demand")
            .WithTags("WorkOrderPartsDemand")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await partsDemandService.ListAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName("ListWorkOrderPartsDemand");

        group.MapGet("/status-events", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await partsDemandService.ListStatusEventsAsync(
                tenantId,
                workOrderId,
                cancellationToken));
        })
        .WithName("ListWorkOrderPartsDemandStatusEvents");

        group.MapPost("/", async (
            Guid workOrderId,
            CreateWorkOrderPartsDemandLineRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            var created = await partsDemandService.CreateAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{workOrderId}/parts-demand/{created.DemandLineId}", created);
        })
        .WithName("CreateWorkOrderPartsDemandLine");

        group.MapPost("/publish", async (
            Guid workOrderId,
            PublishWorkOrderPartsDemandRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            var published = await partsDemandService.PublishAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Ok(published);
        })
        .WithName("PublishWorkOrderPartsDemand");
    }
}
