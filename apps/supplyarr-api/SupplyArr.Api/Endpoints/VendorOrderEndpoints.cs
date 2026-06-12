using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorOrderEndpoints
{
    public static void MapSupplyArrVendorOrderEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorOrders").RequireAuthorization();

            group.MapGet("/", async (
                string? status,
                Guid? vendorId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderRead(context.User);
                return Results.Ok(await service.ListAsync(
                    context.User.GetTenantId(),
                    status,
                    vendorId,
                    cancellationToken));
            })
            .WithName($"ListVendorOrders{nameSuffix}");

            group.MapGet("/metadata", (
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service) =>
            {
                authorization.RequireVendorOrderRead(context.User);
                return Results.Ok(service.GetMetadata());
            })
            .WithName($"GetVendorOrderMetadata{nameSuffix}");

            group.MapGet("/{vendorOrderId:guid}", async (
                Guid vendorOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderRead(context.User);
                return Results.Ok(await service.GetAsync(
                    context.User.GetTenantId(),
                    vendorOrderId,
                    cancellationToken));
            })
            .WithName($"GetVendorOrder{nameSuffix}");

            group.MapPost("/", async (
                CreateVendorOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderCreate(context.User);
                var created = await service.CreateAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/vendor-orders/{created.VendorOrderId}", created);
            })
            .WithName($"CreateVendorOrder{nameSuffix}");

            group.MapPatch("/{vendorOrderId:guid}", async (
                Guid vendorOrderId,
                UpdateVendorOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderUpdate(context.User);
                return Results.Ok(await service.UpdateAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    vendorOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpdateVendorOrder{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/send-to-vendor", async (
                Guid vendorOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderSend(context.User);
                return Results.Ok(await service.SendToVendorAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    vendorOrderId,
                    cancellationToken));
            })
            .WithName($"SendVendorOrder{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/status", async (
                Guid vendorOrderId,
                UpdateVendorOrderStatusRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderStatusUpdate(context.User);
                return Results.Ok(await service.SubmitStatusAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    vendorOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"UpdateVendorOrderStatus{nameSuffix}");

            group.MapGet("/{vendorOrderId:guid}/status-history", async (
                Guid vendorOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderRead(context.User);
                return Results.Ok(await service.ListHistoryAsync(
                    context.User.GetTenantId(),
                    vendorOrderId,
                    cancellationToken));
            })
            .WithName($"GetVendorOrderStatusHistory{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/magic-link", async (
                Guid vendorOrderId,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderSend(context.User);
                return Results.Ok(await service.CreateMagicLinkAsync(
                    context.User.GetTenantId(),
                    context.User.GetPersonId().ToString(),
                    context.User.GetUserId(),
                    vendorOrderId,
                    cancellationToken));
            })
            .WithName($"CreateVendorOrderMagicLink{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/documents", async (
                Guid vendorOrderId,
                RegisterVendorOrderDocumentRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderDocumentUpload(context.User);
                return Results.Ok(await service.RegisterDocumentAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    vendorOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"RegisterVendorOrderDocument{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/partial-decision", async (
                Guid vendorOrderId,
                CreateVendorOrderBrokerDecisionRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderUpdate(context.User);
                return Results.Ok(await service.CreateBrokerDecisionAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    vendorOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"CreateVendorOrderPartialDecision{nameSuffix}");

            group.MapPost("/{vendorOrderId:guid}/split", async (
                Guid vendorOrderId,
                SplitVendorOrderRequest request,
                HttpContext context,
                SupplyArrAuthorizationService authorization,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderUpdate(context.User);
                return Results.Ok(await service.SplitRemainingAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    context.User.GetPersonId().ToString(),
                    vendorOrderId,
                    request,
                    cancellationToken));
            })
            .WithName($"SplitVendorOrder{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/vendor-orders"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/vendor-orders"), "V1");
    }
}
