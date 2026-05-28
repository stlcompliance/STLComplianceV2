using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class AssignmentEscalationSettingsEndpoints
{
    public static void MapTrainArrAssignmentEscalationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/assignment-escalation-settings")
            .WithTags("AssignmentEscalationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            AssignmentEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrAssignmentEscalationSettings");

        group.MapPut("/", async (
            UpsertAssignmentEscalationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            AssignmentEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertTrainArrAssignmentEscalationSettings");

        group.MapGet("/pending", async (
            TrainArrAuthorizationService authorization,
            AssignmentEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken));
        })
        .WithName("ListTrainArrPendingAssignmentEscalations");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            AssignmentEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrAssignmentEscalationRuns");

        group.MapGet("/events", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            AssignmentEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentEventsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrAssignmentEscalationEvents");
    }
}
