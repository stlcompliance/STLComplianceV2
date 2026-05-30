using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class EmergencyPurchaseEndpoints
{
    public static void MapSupplyArrEmergencyPurchaseEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("EmergencyPurchases").RequireAuthorization();

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
            .WithName($"ListEmergencyPurchases{nameSuffix}");

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
            .WithName($"ListPendingEmergencyPurchases{nameSuffix}");

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
            .WithName($"GetEmergencyPurchase{nameSuffix}");

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
            .WithName($"CreateEmergencyPurchase{nameSuffix}");

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
            .WithName($"ExpeditedSubmitEmergencyPurchase{nameSuffix}");

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
            .WithName($"ManagerOverrideApproveEmergencyPurchase{nameSuffix}");

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
            .WithName($"IssueEmergencyPurchaseOrder{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/emergency-purchases"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/emergency-purchases"), "V1");
    }
}
