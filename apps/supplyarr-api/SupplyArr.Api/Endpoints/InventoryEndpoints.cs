using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class InventoryEndpoints
{
    public static void MapSupplyArrInventoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory").RequireAuthorization();

        group.MapGet("/locations", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListLocationsAsync(tenantId, cancellationToken));
        })
        .WithName("ListInventoryLocations");

        group.MapGet("/locations/{locationId:guid}", async (
            Guid locationId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetLocationAsync(tenantId, locationId, cancellationToken));
        })
        .WithName("GetInventoryLocation");

        group.MapPost("/locations", async (
            CreateInventoryLocationRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateLocationAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/inventory/locations/{created.LocationId}", created);
        })
        .WithName("CreateInventoryLocation");

        group.MapPut("/locations/{locationId:guid}", async (
            Guid locationId,
            UpdateInventoryLocationRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLocationAsync(
                tenantId,
                actorUserId,
                locationId,
                request,
                cancellationToken));
        })
        .WithName("UpdateInventoryLocation");

        group.MapPatch("/locations/{locationId:guid}/status", async (
            Guid locationId,
            UpdateInventoryLocationStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLocationStatusAsync(
                tenantId,
                actorUserId,
                locationId,
                request,
                cancellationToken));
        })
        .WithName("UpdateInventoryLocationStatus");

        group.MapGet("/locations/{locationId:guid}/bins", async (
            Guid locationId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListBinsAsync(tenantId, locationId, cancellationToken));
        })
        .WithName("ListInventoryBins");

        group.MapPost("/locations/{locationId:guid}/bins", async (
            Guid locationId,
            CreateInventoryBinRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateBinAsync(
                tenantId,
                actorUserId,
                locationId,
                request,
                cancellationToken);
            return Results.Created($"/api/inventory/bins/{created.BinId}", created);
        })
        .WithName("CreateInventoryBin");

        group.MapPut("/bins/{binId:guid}", async (
            Guid binId,
            UpdateInventoryBinRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateBinAsync(
                tenantId,
                actorUserId,
                binId,
                request,
                cancellationToken));
        })
        .WithName("UpdateInventoryBin");

        group.MapPatch("/bins/{binId:guid}/status", async (
            Guid binId,
            UpdateInventoryBinStatusRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            InventoryLocationService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateBinStatusAsync(
                tenantId,
                actorUserId,
                binId,
                request,
                cancellationToken));
        })
        .WithName("UpdateInventoryBinStatus");

        group.MapGet("/stock", async (
            Guid? locationId,
            Guid? binId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartStockService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                locationId,
                binId,
                partId,
                cancellationToken));
        })
        .WithName("ListPartStockLevels");

        group.MapPost("/stock", async (
            UpsertPartStockLevelRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartStockService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var upserted = await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Ok(upserted);
        })
        .WithName("UpsertPartStockLevel");
    }
}
