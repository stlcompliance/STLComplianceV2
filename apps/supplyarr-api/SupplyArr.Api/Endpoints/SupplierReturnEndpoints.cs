using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierReturnEndpoints
{
    public static void MapSupplyArrSupplierReturnEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string canonicalRoutePrefix)
        {
        group = group.WithTags("SupplierReturns").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? supplierId,
            Guid? purchaseOrderId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                supplierId,
                purchaseOrderId,
                partId,
                cancellationToken));
        })
        .WithName($"ListSupplierReturns{nameSuffix}");

        group.MapGet("/{returnId:guid}", async (
            Guid returnId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, returnId, cancellationToken));
        })
        .WithName($"GetSupplierReturn{nameSuffix}");

        group.MapPost("/from-stock", async (
            CreateSupplierReturnFromStockRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromStockAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"{canonicalRoutePrefix}/{created.ReturnId}", created);
        })
        .WithName($"CreateSupplierReturnFromStock{nameSuffix}");

        group.MapPost("/from-purchase-order-line/{purchaseOrderLineId:guid}", async (
            Guid purchaseOrderLineId,
            CreateSupplierReturnFromPurchaseOrderLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromPurchaseOrderLineAsync(
                tenantId,
                actorUserId,
                purchaseOrderLineId,
                request,
                cancellationToken);
            return Results.Created($"{canonicalRoutePrefix}/{created.ReturnId}", created);
        })
        .WithName($"CreateSupplierReturnFromPurchaseOrderLine{nameSuffix}");

        group.MapPost("/{returnId:guid}/post", async (
            Guid returnId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.PostAsync(
                tenantId,
                actorUserId,
                returnId,
                cancellationToken));
        })
        .WithName($"PostSupplierReturn{nameSuffix}");

        group.MapPost("/{returnId:guid}/cancel", async (
            Guid returnId,
            CancelSupplierReturnRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                returnId,
                request,
                cancellationToken));
        })
        .WithName($"CancelSupplierReturn{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/returns"), "V1", "/api/v1/returns");
    }
}
