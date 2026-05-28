using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformServiceTokenCleanupEndpoints
{
    public static void MapPlatformServiceTokenCleanupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/service-token-cleanup")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/settings", async (
            HttpContext context,
            ServiceTokenCleanupSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformServiceTokenCleanupSettings");

        group.MapPut("/settings", async (
            UpsertServiceTokenCleanupSettingsRequest request,
            HttpContext context,
            ServiceTokenCleanupSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformServiceTokenCleanupSettings");

        group.MapGet("/runs", async (
            int? limit,
            HttpContext context,
            ServiceTokenCleanupSettingsService settingsService,
            ServiceTokenCleanupWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListRecentRunsAsync(limit, cancellationToken));
        })
        .WithName("ListPlatformServiceTokenCleanupRuns");
    }
}
