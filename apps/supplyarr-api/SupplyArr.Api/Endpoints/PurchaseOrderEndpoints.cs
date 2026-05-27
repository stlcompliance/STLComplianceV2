using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PurchaseOrderEndpoints
{
    public static void MapSupplyArrPurchaseOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/purchase-orders").WithTags("PurchaseOrders").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName("ListPurchaseOrders");

        group.MapGet("/{purchaseOrderId:guid}", async (
            Guid purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, purchaseOrderId, cancellationToken));
        })
        .WithName("GetPurchaseOrder");

        group.MapPost("/from-purchase-request/{purchaseRequestId:guid}", async (
            Guid purchaseRequestId,
            CreatePurchaseOrderFromPurchaseRequestRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromPurchaseRequestAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken);
            return Results.Created($"/api/purchase-orders/{created.PurchaseOrderId}", created);
        })
        .WithName("CreatePurchaseOrderFromPurchaseRequest");

        group.MapPut("/{purchaseOrderId:guid}", async (
            Guid purchaseOrderId,
            UpdatePurchaseOrderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                request,
                cancellationToken));
        })
        .WithName("UpdatePurchaseOrder");

        group.MapPost("/{purchaseOrderId:guid}/lines", async (
            Guid purchaseOrderId,
            AddPurchaseOrderLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.AddLineAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                request,
                cancellationToken));
        })
        .WithName("AddPurchaseOrderLine");

        group.MapPut("/{purchaseOrderId:guid}/lines/{lineId:guid}", async (
            Guid purchaseOrderId,
            Guid lineId,
            UpdatePurchaseOrderLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                lineId,
                request,
                cancellationToken));
        })
        .WithName("UpdatePurchaseOrderLine");

        group.MapDelete("/{purchaseOrderId:guid}/lines/{lineId:guid}", async (
            Guid purchaseOrderId,
            Guid lineId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RemoveLineAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                lineId,
                cancellationToken));
        })
        .WithName("RemovePurchaseOrderLine");

        group.MapPost("/{purchaseOrderId:guid}/approve", async (
            Guid purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ApproveAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                cancellationToken));
        })
        .WithName("ApprovePurchaseOrder");

        group.MapPost("/{purchaseOrderId:guid}/issue", async (
            Guid purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.IssueAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                cancellationToken));
        })
        .WithName("IssuePurchaseOrder");

        group.MapPost("/{purchaseOrderId:guid}/cancel", async (
            Guid purchaseOrderId,
            CancelPurchaseOrderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseOrderService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseOrderCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                request,
                cancellationToken));
        })
        .WithName("CancelPurchaseOrder");
    }
}
