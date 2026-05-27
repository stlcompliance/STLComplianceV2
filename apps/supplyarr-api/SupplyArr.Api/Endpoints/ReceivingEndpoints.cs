using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ReceivingEndpoints
{
    public static void MapSupplyArrReceivingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/receiving").WithTags("Receiving").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, purchaseOrderId, cancellationToken));
        })
        .WithName("ListReceivingReceipts");

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
        .WithName("GetReceivingReceipt");

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
        .WithName("CreateReceivingReceiptFromPurchaseOrder");

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
        .WithName("UpdateReceivingReceiptLine");

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
            return Results.Ok(await service.PostAsync(
                tenantId,
                actorUserId,
                receivingReceiptId,
                cancellationToken));
        })
        .WithName("PostReceivingReceipt");

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
        .WithName("ListReceivingExceptions");

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
        .WithName("CreateReceivingException");

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
        .WithName("ResolveReceivingException");
    }
}
