using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class StockReservationEndpoints
{
    public static void MapSupplyArrStockReservationEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("StockReservations").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? partId,
            Guid? binId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StockReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                partId,
                binId,
                cancellationToken));
        })
        .WithName($"ListStockReservations{nameSuffix}");

        group.MapGet("/{reservationId:guid}", async (
            Guid reservationId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StockReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, reservationId, cancellationToken));
        })
        .WithName($"GetStockReservation{nameSuffix}");

        group.MapPost("/", async (
            CreateStockReservationRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StockReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/inventory/reservations/{created.ReservationId}", created);
        })
        .WithName($"CreateStockReservation{nameSuffix}");

        group.MapPost("/{reservationId:guid}/release", async (
            Guid reservationId,
            ReleaseStockReservationRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StockReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ReleaseAsync(
                tenantId,
                actorUserId,
                reservationId,
                request,
                cancellationToken));
        })
        .WithName($"ReleaseStockReservation{nameSuffix}");

        group.MapPost("/{reservationId:guid}/fulfill", async (
            Guid reservationId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StockReservationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.FulfillAsync(
                tenantId,
                actorUserId,
                reservationId,
                cancellationToken));
        })
        .WithName($"FulfillStockReservation{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/inventory/reservations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/inventory/reservations"), "V1");
    }
}
