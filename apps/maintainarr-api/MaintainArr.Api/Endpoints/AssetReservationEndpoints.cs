using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetReservationEndpoints
{
    public static void MapMaintainArrAssetReservationEndpoints(this WebApplication app)
    {
        MapReservationRoutes(
            app.MapGroup("/api/reservations").WithTags("Reservations").RequireAuthorization(),
            string.Empty,
            "/api/reservations");

        MapReservationRoutes(
            app.MapGroup("/api/v1/reservations").WithTags("Reservations").RequireAuthorization(),
            "V1",
            "/api/v1/reservations");
    }

    private static void MapReservationRoutes(RouteGroupBuilder group, string nameSuffix, string routePrefix)
    {
        group.MapGet("/", async (
            Guid? assetId,
            string? status,
            bool? activeOnly,
            int? limit,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReservationsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                assetId,
                status,
                activeOnly,
                limit,
                cancellationToken));
        })
        .WithName($"ListAssetReservations{nameSuffix}");

        group.MapGet("/{reservationId:guid}", async (
            Guid reservationId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReservationsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, reservationId, cancellationToken));
        })
        .WithName($"GetAssetReservation{nameSuffix}");

        group.MapPost("/", async (
            Guid assetId,
            CreateAssetReservationRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReservationsRead(context.User);
            if (assetId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "assetId must be a non-empty GUID." });
            }

            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                assetId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/{created.ReservationId}", created);
        })
        .WithName($"CreateAssetReservation{nameSuffix}");

        MapReservationActionRoutes(group, nameSuffix, "approve", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.ApproveAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "reserve", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.ReserveAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "checkout", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.CheckOutAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "start-use", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.StartUseAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "return", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.ReturnAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "inspect", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.InspectAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "close", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.CloseAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "cancel", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.CancelAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
        MapReservationActionRoutes(group, nameSuffix, "no-show", async (service, tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken) =>
            await service.NoShowAsync(tenantId, actorUserId, actorPersonId, reservationId, request, cancellationToken));
    }

    private static void MapReservationActionRoutes(
        RouteGroupBuilder group,
        string nameSuffix,
        string actionPath,
        Func<AssetReservationService, Guid, Guid, string, Guid, ReservationActionRequest, CancellationToken, Task<AssetReservationResponse>> handler)
    {
        group.MapPost("/{reservationId:guid}/" + actionPath, async (
            Guid reservationId,
            ReservationActionRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReservationsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await handler(
                service,
                tenantId,
                actorUserId,
                actorPersonId,
                reservationId,
                request,
                cancellationToken));
        })
        .WithName($"{HumanizeAction(actionPath)}AssetReservation{nameSuffix}");
    }

    private static string HumanizeAction(string actionPath)
    {
        return actionPath
            .Replace('-', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..])
            .Aggregate(string.Empty, (current, next) => current + next);
    }
}
