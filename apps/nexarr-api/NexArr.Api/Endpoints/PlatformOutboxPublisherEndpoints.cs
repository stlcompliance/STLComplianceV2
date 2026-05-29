using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformOutboxPublisherEndpoints
{
    public static void MapPlatformOutboxPublisherEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/platform-outbox")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("/settings", async (
            HttpContext context,
            PlatformOutboxPublisherSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformOutboxPublisherSettings");

        group.MapPut("/settings", async (
            UpsertPlatformOutboxPublisherSettingsRequest request,
            HttpContext context,
            PlatformOutboxPublisherSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformOutboxPublisherSettings");

        group.MapGet("/status", async (
            HttpContext context,
            PlatformOutboxPublisherSettingsService settingsService,
            PlatformOutboxPublisherWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.GetStatusAsync(cancellationToken));
        })
        .WithName("GetPlatformOutboxPublisherStatus");

        group.MapGet("/events", async (
            int? limit,
            HttpContext context,
            PlatformOutboxPublisherSettingsService settingsService,
            PlatformOutboxPublisherWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListRecentEventsAsync(limit, cancellationToken));
        })
        .WithName("ListPlatformOutboxEvents");

        group.MapGet("/runs", async (
            int? limit,
            HttpContext context,
            PlatformOutboxPublisherSettingsService settingsService,
            PlatformOutboxPublisherWorkerService workerService,
            CancellationToken cancellationToken) =>
        {
            await settingsService.GetAsync(context.User, cancellationToken);
            return Results.Ok(await workerService.ListRecentRunsAsync(limit, cancellationToken));
        })
        .WithName("ListPlatformOutboxPublisherRuns");
    }
}
