using STLCompliance.Shared.Auth;
using TrainArr.Api.Services;

namespace TrainArr.Api.Endpoints;

public static class LoadTestJourneySeedEndpoints
{
    public static void MapTrainArrLoadTestJourneySeedEndpoints(this WebApplication app)
    {
        app.MapPost("/api/load-test-journey/seed", async (
            TrainArrAuthorizationService authorization,
            LoadTestJourneySeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTrainingDefinitionsManage(context.User);
            authorization.RequireQualificationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await seedService.EnsureSeededAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("LoadTestJourney")
        .RequireAuthorization()
        .WithName("SeedTrainArrLoadTestJourney");
    }
}
