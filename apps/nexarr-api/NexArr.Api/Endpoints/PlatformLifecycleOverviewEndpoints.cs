using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformLifecycleOverviewEndpoints
{
    public static void MapPlatformLifecycleOverviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/platform-lifecycle")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/overview", async (
            HttpContext context,
            PlatformLifecycleOverviewService overviewService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await overviewService.GetOverviewAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformLifecycleOverview");
    }
}
