using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ReceivingEndpoints
{
    public static void MapSupplyArrReceivingEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group.MapGet("/", async (
            string? status,
            Guid? purchaseOrderId,
            string? purchaseOrderKey,
            string? packingSlipReference,
            string? invoiceReference,
            string? query,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                purchaseOrderId,
                purchaseOrderKey,
                packingSlipReference,
                invoiceReference,
                query,
                cancellationToken));
        })
        .WithName($"ListReceivingReceipts{nameSuffix}");

        group.MapGet("/{receivingReceiptId:guid}", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, receivingReceiptId, cancellationToken));
        })
        .WithName($"GetReceivingReceipt{nameSuffix}");

        group.MapGet("/{receivingReceiptId:guid}/export-accounting.csv", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            var bytes = await service.BuildAccountingExportCsvAsync(
                tenantId,
                receivingReceiptId,
                cancellationToken);
            return Results.File(
                bytes,
                "text/csv",
                $"supplyarr-receipt-accounting-{receivingReceiptId:D}.csv");
        })
        .WithName($"ExportReceivingReceiptAccountingCsv{nameSuffix}");

        group.MapGet("/by-key/{receiptKey}", async (
            string receiptKey,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetByReceiptKeyAsync(tenantId, receiptKey, cancellationToken));
        })
        .WithName($"GetReceivingReceiptByKey{nameSuffix}");

        group.MapGet("/by-packing-slip/{packingSlipReference}", async (
            string packingSlipReference,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListByPackingSlipReferenceAsync(
                tenantId,
                packingSlipReference,
                cancellationToken));
        })
        .WithName($"ListReceivingReceiptsByPackingSlipReference{nameSuffix}");

        group.MapPost("/from-purchase-order/{purchaseOrderId:guid}", async (
            Guid purchaseOrderId,
            CreateReceivingReceiptFromPurchaseOrderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromPurchaseOrderAsync(
                tenantId,
                actorUserId,
                purchaseOrderId,
                request,
                cancellationToken);
            return Results.Created($"/api/receiving/{created.ReceivingReceiptId}", created);
        })
        .WithName($"CreateReceivingReceiptFromPurchaseOrder{nameSuffix}");

        group.MapPost("/from-purchase-order-key/{purchaseOrderKey}", async (
            string purchaseOrderKey,
            CreateReceivingReceiptFromPurchaseOrderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateFromPurchaseOrderKeyAsync(
                tenantId,
                actorUserId,
                purchaseOrderKey,
                request,
                cancellationToken);
            return Results.Created($"/api/receiving/{created.ReceivingReceiptId}", created);
        })
        .WithName($"CreateReceivingReceiptFromPurchaseOrderKey{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/lines/{lineId:guid}", async (
            Guid receivingReceiptId,
            Guid lineId,
            UpdateReceivingReceiptLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                lineId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptLine{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/lines/{lineId:guid}/tracking", async (
            Guid receivingReceiptId,
            Guid lineId,
            UpdateReceivingReceiptLineTrackingRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineTrackingAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                lineId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptLineTracking{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/lines/{lineId:guid}/condition", async (
            Guid receivingReceiptId,
            Guid lineId,
            UpdateReceivingReceiptLineConditionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineConditionAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                lineId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptLineCondition{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/packing-slip", async (
            Guid receivingReceiptId,
            UpdateReceivingPackingSlipRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdatePackingSlipAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptPackingSlip{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/invoice", async (
            Guid receivingReceiptId,
            UpdateReceivingInvoiceRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateInvoiceAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptInvoice{nameSuffix}");

        group.MapPut("/{receivingReceiptId:guid}/inventory-bin", async (
            Guid receivingReceiptId,
            UpdateReceivingInventoryBinRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateInventoryBinAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateReceivingReceiptInventoryBin{nameSuffix}");

        group.MapPost("/{receivingReceiptId:guid}/post", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId();
            return Results.Ok(await service.PostAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                receivingReceiptId,
                cancellationToken));
        })
        .WithName($"PostReceivingReceipt{nameSuffix}");

        group.MapPost("/{receivingReceiptId:guid}/close", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CloseAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                cancellationToken));
        })
        .WithName($"CloseReceivingReceipt{nameSuffix}");

        group.MapPost("/{receivingReceiptId:guid}/reopen", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ReopenAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                cancellationToken));
        })
        .WithName($"ReopenReceivingReceipt{nameSuffix}");

        group.MapGet("/{receivingReceiptId:guid}/exceptions", async (
            Guid receivingReceiptId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForReceiptAsync(
                tenantId,
                receivingReceiptId,
                cancellationToken));
        })
        .WithName($"ListReceivingExceptions{nameSuffix}");

        group.MapPost("/{receivingReceiptId:guid}/lines/{lineId:guid}/exceptions", async (
            Guid receivingReceiptId,
            Guid lineId,
            CreateReceivingExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                lineId,
                request,
                cancellationToken);
            return Results.Created(
                $"/api/receiving/exceptions/{created.ReceivingExceptionId}",
                created);
        })
        .WithName($"CreateReceivingException{nameSuffix}");

        group.MapPost("/exceptions/{receivingExceptionId:guid}/resolve", async (
            Guid receivingExceptionId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ResolveAsync(
                tenantId,
                actorUserId,
                receivingExceptionId,
                cancellationToken));
        })
        .WithName($"ResolveReceivingException{nameSuffix}");

        group.MapPost("/exceptions/{receivingExceptionId:guid}/cancel", async (
            Guid receivingExceptionId,
            CancelReceivingExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                receivingExceptionId,
                request,
                cancellationToken));
        })
        .WithName($"CancelReceivingException{nameSuffix}");

        group.MapPost("/exceptions/{receivingExceptionId:guid}/reopen", async (
            Guid receivingExceptionId,
            ReopenReceivingExceptionRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingPerform(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ReopenAsync(
                tenantId,
                actorUserId,
                receivingExceptionId,
                request,
                cancellationToken));
        })
        .WithName($"ReopenReceivingException{nameSuffix}");
        }

        var legacyGroup = app.MapGroup("/api/receiving").WithTags("Receiving").RequireAuthorization();
        MapRoutes(legacyGroup, string.Empty);

        var v1Group = app.MapGroup("/api/v1/receiving").WithTags("Receiving").RequireAuthorization();
        MapRoutes(v1Group, "V1");

        var v1ReceiptsGroup = app.MapGroup("/api/v1/receipts").WithTags("Receiving").RequireAuthorization();
        MapRoutes(v1ReceiptsGroup, "V1Receipts");
    }
}
