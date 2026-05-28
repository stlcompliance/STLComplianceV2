using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class NotificationSettingsEndpoints
{
    public static void MapTrainArrNotificationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notification-settings")
            .WithTags("NotificationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            TrainingNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrNotificationSettings");

        group.MapPut("/", async (
            UpsertTrainingNotificationSettingsRequest request,
            TrainArrAuthorizationService authorization,
            TrainingNotificationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertTrainArrNotificationSettings");

        group.MapGet("/dispatches", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            TrainingNotificationDispatchService dispatchService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireNotificationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await dispatchService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrNotificationDispatches");
    }
}
