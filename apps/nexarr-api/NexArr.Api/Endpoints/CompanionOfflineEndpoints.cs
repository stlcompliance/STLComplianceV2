using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionOfflineEndpoints
{
    public static void MapCompanionOfflineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/offline-actions")
            .WithTags("CompanionOffline")
            .RequireAuthorization();

        group.MapPost("/sync", async (
            SyncCompanionOfflineActionsRequest request,
            CompanionOfflineSyncService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.SyncAsync(context.User, request, cancellationToken));
        })
        .WithName("SyncCompanionOfflineActions");

        group.MapGet("/", async (
            int? limit,
            CompanionOfflineSyncService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListRecentAsync(context.User, limit, cancellationToken));
        })
        .WithName("ListCompanionOfflineActions");
    }
}
