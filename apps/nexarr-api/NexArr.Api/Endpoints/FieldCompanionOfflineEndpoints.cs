using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionOfflineEndpoints
{
    public static void MapFieldCompanionOfflineEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/offline-actions", "/api/v1/mobile/offline-actions", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var sync = group.MapPost("/sync", async (
                SyncFieldCompanionOfflineActionsRequest request,
                FieldCompanionOfflineSyncService service,
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
            });
            if (isCanonical)
            {
                sync.WithName("SyncFieldCompanionOfflineActions");
            }

            var list = group.MapGet("/", async (
                int? limit,
                FieldCompanionOfflineSyncService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.ListRecentAsync(context.User, limit, cancellationToken));
            });
            if (isCanonical)
            {
                list.WithName("ListFieldCompanionOfflineActions");
            }
        });
    }
}
