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
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            ReceivingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReceivingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, purchaseOrderId, cancellationToken));
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
        }

        var legacyGroup = app.MapGroup("/api/receiving").WithTags("Receiving").RequireAuthorization();
        MapRoutes(legacyGroup, string.Empty);

        var v1Group = app.MapGroup("/api/v1/receiving").WithTags("Receiving").RequireAuthorization();
        MapRoutes(v1Group, "V1");

        var v1ReceiptsGroup = app.MapGroup("/api/v1/receipts").WithTags("Receiving").RequireAuthorization();
        MapRoutes(v1ReceiptsGroup, "V1Receipts");
    }
}
