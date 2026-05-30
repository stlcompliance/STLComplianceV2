using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderPartsDemandEndpoints
{
    public static void MapMaintainArrWorkOrderPartsDemandEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/work-orders/{workOrderId:guid}/parts-demand"),
            app.MapGroup("/api/v1/work-orders/{workOrderId:guid}/parts-demand"),
        };

        foreach (var group in groups)
        {
            group.WithTags("WorkOrderPartsDemand").RequireAuthorization();

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
            });

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
            });

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
            });

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
            });
        }

        var partsUsageAlias = app.MapGroup("/api/v1/parts-usage")
            .WithTags("WorkOrderPartsDemand")
            .RequireAuthorization();

        partsUsageAlias.MapGet("/", async (
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
        });

        partsUsageAlias.MapPost("/", async (
            CreateWorkOrderPartsUsageAliasRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderPartsDemandService partsDemandService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var detail = await workOrderService.GetAsync(tenantId, request.WorkOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            var created = await partsDemandService.CreateAsync(
                tenantId,
                actorUserId,
                request.WorkOrderId,
                new CreateWorkOrderPartsDemandLineRequest(
                    request.SupplyarrPartId,
                    request.PartNumber,
                    request.Description,
                    request.QuantityRequested,
                    request.UnitOfMeasure,
                    request.Notes),
                cancellationToken);
            return Results.Created($"/api/v1/parts-usage/{created.DemandLineId}", created);
        });
    }
}
