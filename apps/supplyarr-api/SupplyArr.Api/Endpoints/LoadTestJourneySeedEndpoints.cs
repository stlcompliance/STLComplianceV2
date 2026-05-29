using STLCompliance.Shared.Auth;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class LoadTestJourneySeedEndpoints
{
    public static void MapSupplyArrLoadTestJourneySeedEndpoints(this WebApplication app)
    {
        app.MapPost("/api/load-test-journey/seed", async (
            SupplyArrAuthorizationService authorization,
            LoadTestJourneySeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await seedService.EnsureSeededAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("LoadTestJourney")
        .RequireAuthorization()
        .WithName("SeedSupplyArrLoadTestJourney");
    }
}
