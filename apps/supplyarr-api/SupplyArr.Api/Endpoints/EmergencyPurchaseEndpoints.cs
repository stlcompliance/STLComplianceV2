using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class EmergencyPurchaseEndpoints
{
    public static void MapSupplyArrEmergencyPurchaseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/emergency-purchases")
            .WithTags("EmergencyPurchases")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName("ListEmergencyPurchases");

        group.MapGet("/pending", async (
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseOverrideApprove(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPendingOverrideAsync(tenantId, cancellationToken));
        })
        .WithName("ListPendingEmergencyPurchases");

        group.MapGet("/{purchaseRequestId:guid}", async (
            Guid purchaseRequestId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, purchaseRequestId, cancellationToken));
        })
        .WithName("GetEmergencyPurchase");

        group.MapPost("/", async (
            CreateEmergencyPurchaseRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/emergency-purchases/{created.PurchaseRequestId}", created);
        })
        .WithName("CreateEmergencyPurchase");

        group.MapPost("/{purchaseRequestId:guid}/expedited-submit", async (
            Guid purchaseRequestId,
            ExpeditedSubmitEmergencyPurchaseRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseExpedite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ExpeditedSubmitAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName("ExpeditedSubmitEmergencyPurchase");

        group.MapPost("/{purchaseRequestId:guid}/manager-override-approve", async (
            Guid purchaseRequestId,
            ManagerOverrideApproveEmergencyPurchaseRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseOverrideApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ManagerOverrideApproveAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName("ManagerOverrideApproveEmergencyPurchase");

        group.MapPost("/{purchaseRequestId:guid}/issue-purchase-order", async (
            Guid purchaseRequestId,
            IssueEmergencyPurchaseOrderRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            EmergencyPurchaseService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEmergencyPurchaseIssueOrder(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.IssuePurchaseOrderAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName("IssueEmergencyPurchaseOrder");
    }
}
