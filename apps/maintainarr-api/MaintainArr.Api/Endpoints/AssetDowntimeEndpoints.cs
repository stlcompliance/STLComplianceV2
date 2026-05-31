using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetDowntimeEndpoints
{
    public static void MapMaintainArrAssetDowntimeEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/downtime", Suffix: string.Empty),
            (Route: "/api/v1/downtime", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("Downtime")
                .RequireAuthorization();

            group.MapGet("/", (
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireDowntimeRead(context.User);
                return Results.Ok(new
                {
                    items = new[]
                    {
                        new { key = "events", path = $"{route}/events" },
                        new { key = "asset-availability", path = $"{route}/availability/assets/{{assetId}}" },
                        new { key = "fleet-availability", path = $"{route}/availability/fleet" },
                    }
                });
            })
            .WithName($"GetMaintainArrDowntimeIndex{suffix}");

            group.MapGet("/events", async (
                Guid? assetId,
                bool? activeOnly,
                int? limit,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListEventsAsync(
                    tenantId,
                    assetId,
                    activeOnly,
                    limit,
                    cancellationToken));
            })
            .WithName($"ListMaintainArrDowntimeEvents{suffix}");

            group.MapGet("/events/{eventId:guid}", async (
                Guid eventId,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetEventAsync(tenantId, eventId, cancellationToken));
            })
            .WithName($"GetMaintainArrDowntimeEvent{suffix}");

            group.MapPost("/events", async (
                CreateManualDowntimeEventRequest request,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var result = await service.CreateManualEventAsync(
                    tenantId,
                    actorUserId,
                    request,
                    cancellationToken);
                return Results.Created($"{route}/events/{result.EventId}", result);
            })
            .WithName($"CreateMaintainArrManualDowntimeEvent{suffix}");

            group.MapPost("/events/{eventId:guid}/close", async (
                Guid eventId,
                CloseDowntimeEventRequest request,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.CloseEventAsync(
                    tenantId,
                    eventId,
                    actorUserId,
                    request,
                    cancellationToken));
            })
            .WithName($"CloseMaintainArrDowntimeEvent{suffix}");

            group.MapGet("/availability/assets/{assetId:guid}", async (
                Guid assetId,
                DateTimeOffset? periodStart,
                DateTimeOffset? periodEnd,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetAssetAvailabilityAsync(
                    tenantId,
                    assetId,
                    periodStart,
                    periodEnd,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrAssetAvailability{suffix}");

            group.MapGet("/availability/fleet", async (
                DateTimeOffset? periodStart,
                DateTimeOffset? periodEnd,
                MaintainArrAuthorizationService authorization,
                AssetDowntimeService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireDowntimeRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetFleetAvailabilityAsync(
                    tenantId,
                    periodStart,
                    periodEnd,
                    cancellationToken));
            })
            .WithName($"GetMaintainArrFleetAvailability{suffix}");
        }
    }
}
