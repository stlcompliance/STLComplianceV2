using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PurchaseRequestEndpoints
{
    public static void MapSupplyArrPurchaseRequestEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName($"ListPurchaseRequests{nameSuffix}");

        group.MapGet("/{purchaseRequestId:guid}", async (
            Guid purchaseRequestId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, purchaseRequestId, cancellationToken));
        })
        .WithName($"GetPurchaseRequest{nameSuffix}");

        group.MapPost("/", async (
            CreatePurchaseRequestRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/purchase-requests/{created.PurchaseRequestId}", created);
        })
        .WithName($"CreatePurchaseRequest{nameSuffix}");

        group.MapPut("/{purchaseRequestId:guid}", async (
            Guid purchaseRequestId,
            UpdatePurchaseRequestRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName($"UpdatePurchaseRequest{nameSuffix}");

        group.MapPost("/{purchaseRequestId:guid}/lines", async (
            Guid purchaseRequestId,
            AddPurchaseRequestLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.AddLineAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName($"AddPurchaseRequestLine{nameSuffix}");

        group.MapPut("/{purchaseRequestId:guid}/lines/{lineId:guid}", async (
            Guid purchaseRequestId,
            Guid lineId,
            UpdatePurchaseRequestLineRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateLineAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                lineId,
                request,
                cancellationToken));
        })
        .WithName($"UpdatePurchaseRequestLine{nameSuffix}");

        group.MapDelete("/{purchaseRequestId:guid}/lines/{lineId:guid}", async (
            Guid purchaseRequestId,
            Guid lineId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RemoveLineAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                lineId,
                cancellationToken));
        })
        .WithName($"RemovePurchaseRequestLine{nameSuffix}");

        group.MapPost("/{purchaseRequestId:guid}/submit", async (
            Guid purchaseRequestId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SubmitAsync(
                tenantId,
                actorUserId,
                context.User.GetPersonId(),
                purchaseRequestId,
                cancellationToken));
        })
        .WithName($"SubmitPurchaseRequest{nameSuffix}");

        group.MapPost("/{purchaseRequestId:guid}/approve", async (
            Guid purchaseRequestId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.ApproveAsync(
                tenantId,
                actorUserId,
                context.User.GetPersonId(),
                purchaseRequestId,
                cancellationToken));
        })
        .WithName($"ApprovePurchaseRequest{nameSuffix}");

        group.MapPost("/{purchaseRequestId:guid}/reject", async (
            Guid purchaseRequestId,
            RejectPurchaseRequestRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PurchaseRequestService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestApprove(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RejectAsync(
                tenantId,
                actorUserId,
                purchaseRequestId,
                request,
                cancellationToken));
        })
        .WithName($"RejectPurchaseRequest{nameSuffix}");
        }

        var legacyGroup = app.MapGroup("/api/purchase-requests").WithTags("PurchaseRequests").RequireAuthorization();
        MapRoutes(legacyGroup, string.Empty);

        var v1Group = app.MapGroup("/api/v1/purchase-requests").WithTags("PurchaseRequests").RequireAuthorization();
        MapRoutes(v1Group, "V1");
    }
}
