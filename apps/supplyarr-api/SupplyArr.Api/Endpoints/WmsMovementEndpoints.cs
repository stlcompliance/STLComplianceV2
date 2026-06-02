using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class WmsMovementEndpoints
{
    public const string RoutarrShipmentStatusWriteActionScope = "supplyarr.shipments.status.write";

    public static void MapSupplyArrWmsMovementEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("WMS").RequireAuthorization();

            group.MapGet("/stock-ledger", async (
                Guid? partId,
                Guid? binId,
                Guid? locationId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryRead(context.User);
                return Results.Ok(await service.ListLedgerAsync(
                    context.User.GetTenantId(),
                    partId,
                    binId,
                    locationId,
                    cancellationToken));
            })
            .WithName($"ListWmsStockLedger{nameSuffix}");

            group.MapPost("/transfer", async (
                TransferStockRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                return Results.Ok(await service.TransferAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"TransferWmsStock{nameSuffix}");

            group.MapPost("/reserve", async (
                ReserveStockRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                return Results.Ok(await service.ReserveAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"ReserveWmsStock{nameSuffix}");

            group.MapPost("/pick", async (
                PickStockRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                return Results.Ok(await service.PickAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"PickWmsStock{nameSuffix}");

            group.MapPost("/ship", async (
                ShipStockRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                return Results.Ok(await service.ShipAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"ShipWmsStock{nameSuffix}");

            group.MapPost("/cancel", async (
                CancelStockMovementRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                return Results.Ok(await service.CancelAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"CancelWmsStock{nameSuffix}");

            group.MapPost("/outbound-shipments", async (
                CreateOutboundShipmentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireInventoryManage(context.User);
                var created = await service.CreateOutboundShipmentAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/wms/outbound-shipments/{created.ShipmentId}", created);
            })
            .WithName($"CreateWmsOutboundShipment{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/wms"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/wms"), "V1");

        static void MapIntegrationRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("Integrations");

            group.MapPost("/routarr-shipment-status", async (
                RoutArrShipmentStatusUpdateRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                WmsMovementService service,
                CancellationToken cancellationToken) =>
            {
                tokenValidator.ValidateOrThrow(
                    ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                    new ServiceTokenRequirements
                    {
                        ExpectedSourceProduct = "routarr",
                        RequiredTargetProduct = "supplyarr",
                        TenantId = request.TenantId,
                        RequiredActionScope = RoutarrShipmentStatusWriteActionScope,
                    });

                return Results.Ok(await service.UpdateRoutArrStatusAsync(request, cancellationToken));
            })
            .WithName($"UpdateRoutArrShipmentStatus{nameSuffix}");
        }

        MapIntegrationRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapIntegrationRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }
}
