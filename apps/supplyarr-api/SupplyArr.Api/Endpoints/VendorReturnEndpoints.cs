using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorReturnEndpoints
{
    public static void MapSupplyArrVendorReturnEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/returns").WithTags("Returns").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? vendorPartyId,
            Guid? purchaseOrderId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                vendorPartyId,
                purchaseOrderId,
                partId,
                cancellationToken));
        })
        .WithName("ListVendorReturns");

        group.MapGet("/{returnId:guid}", async (
            Guid returnId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, returnId, cancellationToken));
        })
        .WithName("GetVendorReturn");

        group.MapPost("/from-stock", async (
            CreateVendorReturnFromStockRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
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
            return Results.Created($"/api/returns/{created.ReturnId}", created);
        })
        .WithName("CreateVendorReturnFromStock");

        group.MapPost("/from-purchase-order-line/{purchaseOrderLineId:guid}", async (
            Guid purchaseOrderLineId,
            CreateVendorReturnFromPurchaseOrderLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
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
            return Results.Created($"/api/returns/{created.ReturnId}", created);
        })
        .WithName("CreateVendorReturnFromPurchaseOrderLine");

        group.MapPost("/{returnId:guid}/post", async (
            Guid returnId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
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
        .WithName("PostVendorReturn");

        group.MapPost("/{returnId:guid}/cancel", async (
            Guid returnId,
            CancelVendorReturnRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorReturnService service,
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
        .WithName("CancelVendorReturn");
    }
}
