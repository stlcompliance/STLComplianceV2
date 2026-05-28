using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class PersonTrainingHistoryEndpoints
{
    public static void MapTrainArrPersonTrainingHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/person-training-history")
            .WithTags("PersonTrainingHistory")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid staffarrPersonId,
            int? limit,
            TrainArrAuthorizationService authorization,
            PersonTrainingHistoryService historyService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonTrainingHistoryRead(context.User, staffarrPersonId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await historyService.GetForPersonAsync(
                tenantId,
                staffarrPersonId,
                limit,
                cancellationToken));
        })
        .WithName("GetTrainArrPersonTrainingHistory");

        app.MapGet("/api/people/{staffarrPersonId:guid}/training-history", async (
            Guid staffarrPersonId,
            int? limit,
            TrainArrAuthorizationService authorization,
            PersonTrainingHistoryService historyService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePersonTrainingHistoryRead(context.User, staffarrPersonId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await historyService.GetForPersonAsync(
                tenantId,
                staffarrPersonId,
                limit,
                cancellationToken));
        })
        .WithTags("PersonTrainingHistory")
        .RequireAuthorization()
        .WithName("GetTrainArrPersonTrainingHistoryByPersonRoute");
    }
}
