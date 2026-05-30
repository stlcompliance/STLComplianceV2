using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class WorkflowAliasEndpoints
{
    public static void MapSupplyArrWorkflowAliasEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/bootstrap", async (
            MeService service,
            SupplyArrAuthorizationService authorization,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuthenticated(context.User);
            return Results.Ok(await service.GetSessionBootstrapAsync(context.User, cancellationToken));
        })
        .WithTags("Session")
        .RequireAuthorization()
        .WithName("SupplyArrBootstrapV1");

        app.MapGet("/api/v1/approvals", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService purchaseRequestService,
            PurchaseOrderService purchaseOrderService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();

            var pendingPurchaseRequests = await purchaseRequestService.ListAsync(
                tenantId,
                "pending_approval",
                cancellationToken);
            var pendingPurchaseOrders = await purchaseOrderService.ListAsync(
                tenantId,
                "pending_approval",
                cancellationToken);

            var queue = pendingPurchaseRequests
                .Select(x => new ApprovalQueueItemResponse(
                    "purchase_request",
                    x.PurchaseRequestId,
                    x.RequestKey,
                    x.Status,
                    x.VendorPartyId,
                    x.UpdatedAt))
                .Concat(pendingPurchaseOrders.Select(x => new ApprovalQueueItemResponse(
                    "purchase_order",
                    x.PurchaseOrderId,
                    x.OrderKey,
                    x.Status,
                    x.VendorPartyId,
                    x.UpdatedAt)))
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();

            return Results.Ok(queue);
        })
        .WithTags("Approvals")
        .RequireAuthorization()
        .WithName("ListApprovalsV1");

        app.MapGet("/api/v1/stock-transactions", async (
            int? limit,
            string? cursor,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            AuditHistoryService auditHistoryService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var audit = await auditHistoryService.ListAsync(
                tenantId,
                limit,
                cursor,
                "stock",
                null,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);

            var items = audit.Items
                .Select(x => new StockTransactionItemResponse(
                    x.Id,
                    x.Action,
                    x.TargetId,
                    x.Result,
                    x.OccurredAt))
                .ToList();
            return Results.Ok(items);
        })
        .WithTags("Inventory")
        .RequireAuthorization()
        .WithName("ListStockTransactionsV1");

        app.MapGet("/api/v1/cycle-counts", async (
            Guid? locationId,
            Guid? binId,
            Guid? partId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartStockService stockService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var stock = await stockService.ListAsync(tenantId, locationId, binId, partId, cancellationToken);
            var items = stock
                .Select(x => new CycleCountItemResponse(
                    x.StockLevelId,
                    x.PartId,
                    x.PartKey,
                    x.PartDisplayName,
                    x.BinId,
                    x.BinKey,
                    x.LocationId,
                    x.LocationKey,
                    x.QuantityOnHand,
                    x.QuantityReserved,
                    x.QuantityAvailable,
                    x.UpdatedAt))
                .ToList();
            return Results.Ok(items);
        })
        .WithTags("Inventory")
        .RequireAuthorization()
        .WithName("ListCycleCountsV1");
    }
}

