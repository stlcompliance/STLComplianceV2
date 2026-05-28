using STLCompliance.Shared.Auth;
using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class LoadTestJourneySeedEndpoints
{
    public static void MapRoutArrLoadTestJourneySeedEndpoints(this WebApplication app)
    {
        app.MapPost("/api/load-test-journey/seed", async (
            RoutArrAuthorizationService authorization,
            LoadTestJourneySeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsCreate(context.User);
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await seedService.EnsureSeededAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("LoadTestJourney")
        .RequireAuthorization()
        .WithName("SeedRoutArrLoadTestJourney");

        app.MapGet("/api/load-test-journey/trip", async (
            RoutArrAuthorizationService authorization,
            LoadTestJourneySeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await seedService.GetMirrorTripAsync(tenantId, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("LoadTestJourney")
        .RequireAuthorization()
        .WithName("GetRoutArrLoadTestJourneyTrip");
    }
}
