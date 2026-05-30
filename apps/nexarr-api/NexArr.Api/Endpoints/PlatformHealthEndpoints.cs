using NexArr.Api.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NexArr.Api.Endpoints;

public static class PlatformHealthEndpoints
{
    public static void MapPlatformHealthEndpoints(this WebApplication app)
    {
        static async Task<IResult> PlatformHealthEndpoint(
            PlatformHealthService service,
            CancellationToken cancellationToken)
        {
            var report = await service.GetAggregateHealthAsync(cancellationToken);
            return report.Status.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase)
                ? Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable)
                : Results.Ok(report);
        }

        app.MapGet("/api/platform/health", PlatformHealthEndpoint)
        .WithName("GetPlatformHealth")
        .WithTags("PlatformHealth")
        .AllowAnonymous();

        app.MapGet("/api/v1/system/status", PlatformHealthEndpoint)
            .WithName("GetSystemStatusV1")
            .WithTags("PlatformHealth")
            .AllowAnonymous();

        app.MapGet("/ready", async (IServiceProvider services) =>
        {
            var healthService = services.GetRequiredService<HealthCheckService>();
            var report = await healthService.CheckHealthAsync();
            return report.Status == HealthStatus.Healthy
                ? Results.Ok(new { status = "Healthy" })
                : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("ReadyShortcut")
        .WithTags("PlatformHealth")
        .AllowAnonymous();
    }
}
