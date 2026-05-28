using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class EventProcessingSettingsEndpoints
{
    public static void MapTrainArrEventProcessingSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/event-processing-settings")
            .WithTags("EventProcessingSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            TrainArrAuthorizationService authorization,
            EventProcessingSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEventProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetTrainArrEventProcessingSettings");

        group.MapPut("/", async (
            UpsertEventProcessingSettingsRequest request,
            TrainArrAuthorizationService authorization,
            EventProcessingSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEventProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertTrainArrEventProcessingSettings");

        group.MapGet("/events", async (
            int? limit,
            TrainArrAuthorizationService authorization,
            TrainingEventProcessingService processingService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEventProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await processingService.ListRecentAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListTrainArrTrainingDomainEvents");
    }
}
