using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class WorkOrderDiscussionEndpoints
{
    public static void MapMaintainArrWorkOrderDiscussionEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/work-orders/{workOrderId:guid}").WithTags("WorkOrderDiscussion").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/work-orders/{workOrderId:guid}").WithTags("WorkOrderDiscussion").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/comments", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderDiscussionService discussionService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(context.User, detail.CreatedByUserId, detail.AssignedTechnicianPersonId);
            return Results.Ok(await discussionService.ListCommentsAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"ListWorkOrderComments{nameSuffix}");

        group.MapPost("/comments", async (
            Guid workOrderId,
            CreateWorkOrderCommentRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderDiscussionService discussionService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(context.User, detail.CreatedByUserId, detail.AssignedTechnicianPersonId);
            var created = await discussionService.AddCommentAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                workOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/work-orders/{workOrderId}/comments/{created.CommentId}", created);
        })
        .WithName($"CreateWorkOrderComment{nameSuffix}");

        group.MapGet("/timeline", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            WorkOrderService workOrderService,
            WorkOrderDiscussionService discussionService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkOrdersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await workOrderService.GetAsync(tenantId, workOrderId, cancellationToken);
            authorization.RequireWorkOrderAccess(context.User, detail.CreatedByUserId, detail.AssignedTechnicianPersonId);
            return Results.Ok(await discussionService.ListTimelineAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"ListWorkOrderTimeline{nameSuffix}");
    }
}
