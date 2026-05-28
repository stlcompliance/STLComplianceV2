using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

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
            var authorization = context.Request.Headers.Authorization.ToString();
            var accessToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization["Bearer ".Length..].Trim()
                : string.Empty;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new StlApiException("auth.unauthorized", "Bearer access token is required.", 401);
            }

            return Results.Ok(await service.SyncAsync(context.User, accessToken, request, cancellationToken));
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
