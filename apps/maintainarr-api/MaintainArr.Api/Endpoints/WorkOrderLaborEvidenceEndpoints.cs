using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderLaborEvidenceEndpoints
{
    public static void MapMaintainArrWorkOrderLaborEvidenceEndpoints(this WebApplication app)
    {
        MapTaskRoutes(app.MapGroup("/api/work-orders/{workOrderId:guid}/tasks").WithTags("WorkOrderTasks").RequireAuthorization(), string.Empty);
        MapTaskRoutes(app.MapGroup("/api/v1/work-orders/{workOrderId:guid}/tasks").WithTags("WorkOrderTasks").RequireAuthorization(), "V1");

        MapLaborRoutes(app.MapGroup("/api/work-orders/{workOrderId:guid}/labor").WithTags("WorkOrderLabor").RequireAuthorization(), string.Empty);
        MapLaborRoutes(app.MapGroup("/api/v1/work-orders/{workOrderId:guid}/labor").WithTags("WorkOrderLabor").RequireAuthorization(), "V1");

        MapEvidenceRoutes(app.MapGroup("/api/work-orders/{workOrderId:guid}/evidence").WithTags("WorkOrderEvidence").RequireAuthorization(), string.Empty);
        MapEvidenceRoutes(app.MapGroup("/api/v1/work-orders/{workOrderId:guid}/evidence").WithTags("WorkOrderEvidence").RequireAuthorization(), "V1");
    }

    private static void MapTaskRoutes(RouteGroupBuilder tasks, string nameSuffix)
    {
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
        .WithName($"ListWorkOrderTasks{nameSuffix}");

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
        .WithName($"CreateWorkOrderTask{nameSuffix}");
    }

    private static void MapLaborRoutes(RouteGroupBuilder labor, string nameSuffix)
    {
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
        .WithName($"ListWorkOrderLabor{nameSuffix}");

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
        .WithName($"LogWorkOrderLabor{nameSuffix}");
    }

    private static void MapEvidenceRoutes(RouteGroupBuilder evidence, string nameSuffix)
    {
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
        .WithName($"ListWorkOrderEvidence{nameSuffix}");

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
        .WithName($"UploadWorkOrderEvidence{nameSuffix}");
    }
}
