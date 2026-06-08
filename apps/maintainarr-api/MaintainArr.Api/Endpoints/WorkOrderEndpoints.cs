using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderEndpoints
{
    public static void MapMaintainArrWorkOrderEndpoints(this WebApplication app)
    {
        MapWorkOrderRoutes(app.MapGroup("/api/work-orders").WithTags("WorkOrders").RequireAuthorization(), string.Empty, "/api/work-orders");
        MapWorkOrderRoutes(app.MapGroup("/api/v1/work-orders").WithTags("WorkOrders").RequireAuthorization(), "V1", "/api/v1/work-orders");

        MapDefectWorkOrderRoutes(app.MapGroup("/api/defects").WithTags("Defects").RequireAuthorization(), string.Empty, "/api/work-orders");
        MapDefectWorkOrderRoutes(app.MapGroup("/api/v1/defects").WithTags("Defects").RequireAuthorization(), "V1", "/api/v1/work-orders");
    }

    private static void MapWorkOrderRoutes(RouteGroupBuilder group, string nameSuffix, string workOrderRoutePrefix)
    {
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
        .WithName($"ListWorkOrders{nameSuffix}");

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
        .WithName($"GetWorkOrder{nameSuffix}");

        group.MapPost("/drafts", async (
            CreateWorkOrderRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateDraftAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"{workOrderRoutePrefix}/{created.WorkOrderId}", created);
        })
        .WithName($"CreateWorkOrderDraft{nameSuffix}");

        group.MapPatch("/{workOrderId:guid}/draft", async (
            Guid workOrderId,
            CreateWorkOrderRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            var updated = await service.UpdateDraftAsync(tenantId, actorUserId, workOrderId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateWorkOrderDraft{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/validate", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            return Results.Ok(await service.ValidateDraftAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"ValidateWorkOrderDraft{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/preview", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            return Results.Ok(await service.PreviewDraftAsync(tenantId, workOrderId, actorPersonId, cancellationToken));
        })
        .WithName($"PreviewWorkOrderDraft{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/duplicates", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            return Results.Ok(await service.CheckDuplicateDraftAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"CheckWorkOrderDraftDuplicates{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/open", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            var updated = await service.OpenDraftAsync(tenantId, actorUserId, workOrderId, actorPersonId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"OpenWorkOrderDraft{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/schedule", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            var updated = await service.ScheduleDraftAsync(tenantId, actorUserId, workOrderId, actorPersonId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"ScheduleWorkOrderDraft{nameSuffix}");

        group.MapPost("/{workOrderId:guid}/start", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var existing = await service.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(
                context.User,
                existing.CreatedByUserId,
                existing.AssignedTechnicianPersonId);
            var updated = await service.StartDraftAsync(tenantId, actorUserId, workOrderId, actorPersonId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"StartWorkOrderDraft{nameSuffix}");

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
            return Results.Created($"{workOrderRoutePrefix}/{created.WorkOrderId}", created);
        })
        .WithName($"CreateWorkOrder{nameSuffix}");

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
        .WithName($"UpdateWorkOrder{nameSuffix}");

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
        .WithName($"UpdateWorkOrderStatus{nameSuffix}");
    }

    private static void MapDefectWorkOrderRoutes(RouteGroupBuilder defectGroup, string nameSuffix, string workOrderRoutePrefix)
    {
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
            authorization.RequireDefectAccess(context.User, defect.ReportedByPersonId, defect.ReportedByUserId);
            var created = await workOrderService.CreateFromDefectAsync(
                tenantId,
                actorUserId,
                defectId,
                request,
                cancellationToken);
            return Results.Created($"{workOrderRoutePrefix}/{created.WorkOrderId}", created);
        })
        .WithName($"CreateWorkOrderFromDefect{nameSuffix}");
    }
}
