using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetDowntimeEndpoints
{
    public static void MapMaintainArrAssetDowntimeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/downtime")
            .WithTags("Downtime")
            .RequireAuthorization();

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
        .WithName("ListMaintainArrDowntimeEvents");

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
        .WithName("GetMaintainArrDowntimeEvent");

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
            return Results.Created($"/api/downtime/events/{result.EventId}", result);
        })
        .WithName("CreateMaintainArrManualDowntimeEvent");

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
        .WithName("CloseMaintainArrDowntimeEvent");

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
        .WithName("GetMaintainArrAssetAvailability");

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
        .WithName("GetMaintainArrFleetAvailability");
    }
}
