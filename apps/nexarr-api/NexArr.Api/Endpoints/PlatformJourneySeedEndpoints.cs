using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformJourneySeedEndpoints
{
    public static void MapPlatformJourneySeedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/journey-seeds")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            PlatformJourneySeedService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetTargetsAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformJourneySeedTargets");

        group.MapPost("/{productKey}", async (
            string productKey,
            HttpContext context,
            PlatformJourneySeedService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.SeedAsync(
                context.User,
                productKey,
                context.Request.Headers.Authorization.ToString(),
                cancellationToken));
        })
        .WithName("SeedPlatformJourney");
    }
}
