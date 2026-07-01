using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

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
                    x.SupplierId,
                    x.UpdatedAt))
                .Concat(pendingPurchaseOrders.Select(x => new ApprovalQueueItemResponse(
                    "purchase_order",
                    x.PurchaseOrderId,
                    x.OrderKey,
                    x.Status,
                    x.SupplierId,
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

        app.MapPost("/api/v1/stock-transactions", async (
            CreateStockTransactionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartStockService stockService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var transactionType = request.TransactionType.Trim().ToLowerInvariant();

            var updated = transactionType switch
            {
                "in" or "increment" or "increase" or "receive" or "receipt" or "adjustment_in" =>
                    await stockService.IncrementOnHandAsync(
                        tenantId,
                        actorUserId,
                        request.PartId,
                        request.BinId,
                        request.Quantity,
                        cancellationToken),
                "out" or "decrement" or "decrease" or "issue" or "consume" or "adjustment_out" =>
                    await stockService.DecrementOnHandAsync(
                        tenantId,
                        actorUserId,
                        request.PartId,
                        request.BinId,
                        request.Quantity,
                        cancellationToken),
                _ => throw new StlApiException(
                    "stock_transactions.invalid_type",
                    "Transaction type must be an inbound or outbound stock movement.",
                    400)
            };

            return Results.Created($"/api/v1/inventory/stock?partId={updated.PartId}&binId={updated.BinId}", updated);
        })
        .WithTags("Inventory")
        .RequireAuthorization()
        .WithName("CreateStockTransactionV1");

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

        app.MapPost("/api/v1/cycle-counts", async (
            CreateCycleCountRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PartStockService stockService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireInventoryManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var counted = await stockService.UpsertAsync(
                tenantId,
                actorUserId,
                new UpsertPartStockLevelRequest(request.PartId, request.BinId, request.QuantityOnHand),
                cancellationToken);

            return Results.Created($"/api/v1/cycle-counts/{counted.StockLevelId}", new CycleCountItemResponse(
                counted.StockLevelId,
                counted.PartId,
                counted.PartKey,
                counted.PartDisplayName,
                counted.BinId,
                counted.BinKey,
                counted.LocationId,
                counted.LocationKey,
                counted.QuantityOnHand,
                counted.QuantityReserved,
                counted.QuantityAvailable,
                counted.UpdatedAt));
        })
        .WithTags("Inventory")
        .RequireAuthorization()
        .WithName("CreateCycleCountV1");
    }
}
