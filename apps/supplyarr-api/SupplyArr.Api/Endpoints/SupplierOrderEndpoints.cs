using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierOrderEndpoints
{
    public static void MapSupplyArrSupplierOrderEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string canonicalRoutePrefix)
        {
            group = group.WithTags("SupplierOrders").RequireAuthorization();

            group.MapGet("/", async (
                string? status,
                Guid? supplierId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderRead(context.User);
                return Results.Ok(await service.ListAsync(
                    context.User.GetTenantId(),
                    status,
                    supplierId,
                    cancellationToken));
            })
            .WithName($"ListSupplierOrders{nameSuffix}");

            group.MapGet("/metadata", (
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service) =>
            {
                authorization.RequireSupplierOrderRead(context.User);
                return Results.Ok(service.GetMetadata());
            })
            .WithName($"GetSupplierOrderMetadata{nameSuffix}");

            group.MapGet("/{supplierOrderId:guid}", async (
                Guid supplierOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderRead(context.User);
                return Results.Ok(await service.GetAsync(
                    context.User.GetTenantId(),
                    supplierOrderId,
                    cancellationToken));
            })
            .WithName($"GetSupplierOrder{nameSuffix}");

            group.MapPost("/", async (
                CreateSupplierOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderCreate(context.User);
                var created = await service.CreateAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken);
                return Results.Created($"{canonicalRoutePrefix}/{created.SupplierOrderId}", created);
            })
            .WithName($"CreateSupplierOrder{nameSuffix}");

            group.MapPatch("/{supplierOrderId:guid}", async (
                Guid supplierOrderId,
                UpdateSupplierOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderUpdate(context.User);
                return Results.Ok(await service.UpdateAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    supplierOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpdateSupplierOrder{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/send-to-supplier", async (
                Guid supplierOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderSend(context.User);
                return Results.Ok(await service.SendToSupplierAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    supplierOrderId,
                    cancellationToken));
            })
            .WithName($"SendSupplierOrder{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/status", async (
                Guid supplierOrderId,
                UpdateSupplierOrderStatusRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderStatusUpdate(context.User);
                return Results.Ok(await service.SubmitStatusAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    supplierOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpdateSupplierOrderStatus{nameSuffix}");

            group.MapGet("/{supplierOrderId:guid}/status-history", async (
                Guid supplierOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderRead(context.User);
                return Results.Ok(await service.ListHistoryAsync(
                    context.User.GetTenantId(),
                    supplierOrderId,
                    cancellationToken));
            })
            .WithName($"GetSupplierOrderStatusHistory{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/magic-link", async (
                Guid supplierOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderSend(context.User);
                return Results.Ok(await service.CreateMagicLinkAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    supplierOrderId,
                    cancellationToken));
            })
            .WithName($"CreateSupplierOrderMagicLink{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/documents", async (
                Guid supplierOrderId,
                RegisterSupplierOrderDocumentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderDocumentUpload(context.User);
                return Results.Ok(await service.RegisterDocumentAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    supplierOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"RegisterSupplierOrderDocument{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/partial-decision", async (
                Guid supplierOrderId,
                CreateSupplierOrderBrokerDecisionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderUpdate(context.User);
                return Results.Ok(await service.CreateBrokerDecisionAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    supplierOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"CreateSupplierOrderPartialDecision{nameSuffix}");

            group.MapPost("/{supplierOrderId:guid}/split", async (
                Guid supplierOrderId,
                SplitSupplierOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderUpdate(context.User);
                return Results.Ok(await service.SplitRemainingAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    supplierOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"SplitSupplierOrder{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-orders"), "SupplierV1", "/api/v1/supplier-orders");
    }
}
