using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformHealthEndpoints
{
    public static void MapPlatformHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/api/platform/health", async (
            PlatformHealthService service,
            CancellationToken cancellationToken) =>
        {
            var report = await service.GetAggregateHealthAsync(cancellationToken);
            return report.Status.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase)
                ? Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable)
                : Results.Ok(report);
        })
        .WithName("GetPlatformHealth")
        .WithTags("PlatformHealth")
        .AllowAnonymous();
    }
}
