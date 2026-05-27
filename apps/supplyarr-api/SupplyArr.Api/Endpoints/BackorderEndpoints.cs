using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class BackorderEndpoints
{
    public static void MapSupplyArrBackorderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/backorders").WithTags("Backorders").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? purchaseOrderId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            BackorderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBackorderRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                purchaseOrderId,
                partId,
                cancellationToken));
        })
        .WithName("ListBackorders");

        group.MapGet("/{backorderId:guid}", async (
            Guid backorderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            BackorderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBackorderRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, backorderId, cancellationToken));
        })
        .WithName("GetBackorder");

        group.MapPost("/from-purchase-order-line/{purchaseOrderLineId:guid}", async (
            Guid purchaseOrderLineId,
            CreateBackorderFromPurchaseOrderLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            BackorderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBackorderManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromPurchaseOrderLineAsync(
                tenantId,
                actorUserId,
                purchaseOrderLineId,
                request,
                cancellationToken);
            return Results.Created($"/api/backorders/{created.BackorderId}", created);
        })
        .WithName("CreateBackorderFromPurchaseOrderLine");

        group.MapPost("/{backorderId:guid}/fulfill", async (
            Guid backorderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            BackorderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBackorderManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.FulfillAsync(
                tenantId,
                actorUserId,
                backorderId,
                cancellationToken));
        })
        .WithName("FulfillBackorder");

        group.MapPost("/{backorderId:guid}/cancel", async (
            Guid backorderId,
            CancelBackorderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            BackorderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireBackorderManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                backorderId,
                request,
                cancellationToken));
        })
        .WithName("CancelBackorder");
    }
}
