using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PurchaseOrderEndpoints
{
    public static void MapSupplyArrPurchaseOrderEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
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
        .WithName($"ListPurchaseOrders{nameSuffix}");

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
        .WithName($"GetPurchaseOrder{nameSuffix}");

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
        .WithName($"CreatePurchaseOrderFromPurchaseRequest{nameSuffix}");

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
        .WithName($"UpdatePurchaseOrder{nameSuffix}");

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
        .WithName($"AddPurchaseOrderLine{nameSuffix}");

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
        .WithName($"UpdatePurchaseOrderLine{nameSuffix}");

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
        .WithName($"RemovePurchaseOrderLine{nameSuffix}");

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
        .WithName($"ApprovePurchaseOrder{nameSuffix}");

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
                context.User.GetPersonId(),
                purchaseOrderId,
                cancellationToken));
        })
        .WithName($"IssuePurchaseOrder{nameSuffix}");

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
        .WithName($"CancelPurchaseOrder{nameSuffix}");
        }

        var legacyGroup = app.MapGroup("/api/purchase-orders").WithTags("PurchaseOrders").RequireAuthorization();
        MapRoutes(legacyGroup, string.Empty);

        var v1Group = app.MapGroup("/api/v1/purchase-orders").WithTags("PurchaseOrders").RequireAuthorization();
        MapRoutes(v1Group, "V1");
    }
}
