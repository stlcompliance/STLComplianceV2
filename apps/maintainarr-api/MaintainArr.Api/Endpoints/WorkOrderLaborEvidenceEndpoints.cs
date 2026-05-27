using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderLaborEvidenceEndpoints
{
    public static void MapMaintainArrWorkOrderLaborEvidenceEndpoints(this WebApplication app)
    {
        var tasks = app.MapGroup("/api/work-orders/{workOrderId:guid}/tasks")
            .WithTags("WorkOrderTasks")
            .RequireAuthorization();

        tasks.MapGet("/", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await laborEvidenceService.ListTasksAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName("ListWorkOrderTasks");

        tasks.MapPost("/", async (
            Guid workOrderId,
            CreateWorkOrderTaskLineRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
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
            var created = await laborEvidenceService.CreateTaskAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{workOrderId}/tasks/{created.TaskLineId}", created);
        })
        .WithName("CreateWorkOrderTask");

        var labor = app.MapGroup("/api/work-orders/{workOrderId:guid}/labor")
            .WithTags("WorkOrderLabor")
            .RequireAuthorization();

        labor.MapGet("/", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await laborEvidenceService.ListLaborAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName("ListWorkOrderLabor");

        labor.MapPost("/", async (
            Guid workOrderId,
            CreateWorkOrderLaborEntryRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
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
            var created = await laborEvidenceService.LogLaborAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{workOrderId}/labor/{created.LaborEntryId}", created);
        })
        .WithName("LogWorkOrderLabor");

        var evidence = app.MapGroup("/api/work-orders/{workOrderId:guid}/evidence")
            .WithTags("WorkOrderEvidence")
            .RequireAuthorization();

        evidence.MapGet("/", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                detail.CreatedByUserId,
                detail.AssignedTechnicianPersonId);
            return Results.Ok(await laborEvidenceService.ListEvidenceAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName("ListWorkOrderEvidence");

        evidence.MapPost("/", async (
            Guid workOrderId,
            CreateWorkOrderEvidenceRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderLaborEvidenceService laborEvidenceService,
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
            var created = await laborEvidenceService.UploadEvidenceAsync(
                tenantId,
                actorUserId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{workOrderId}/evidence/{created.EvidenceId}", created);
        })
        .WithName("UploadWorkOrderEvidence");
    }
}
