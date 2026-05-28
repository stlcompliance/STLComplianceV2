using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class AssignmentDueReminderSettingsEndpoints
{
    public static void MapTrainArrAssignmentDueReminderSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/assignment-due-reminder-settings")
            .WithTags("AssignmentDueReminderSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            AssignmentDueReminderSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrAssignmentDueReminderSettings");

        group.MapPut("/", async (
            UpsertAssignmentDueReminderSettingsRequest request,
            TrainArrAuthorizationService authorization,
            AssignmentDueReminderSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertTrainArrAssignmentDueReminderSettings");

        group.MapGet("/pending", async (
            TrainArrAuthorizationService authorization,
            AssignmentDueReminderWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken));
        })
        .WithName("ListTrainArrPendingAssignmentDueReminders");

        group.MapGet("/runs", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            AssignmentDueReminderWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrAssignmentDueReminderRuns");
    }
}
