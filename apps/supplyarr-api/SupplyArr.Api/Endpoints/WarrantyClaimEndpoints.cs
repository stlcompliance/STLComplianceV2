using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class WarrantyClaimEndpoints
{
    public static void MapSupplyArrWarrantyClaimEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("WarrantyClaims").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? supplierId,
            Guid? partId,
            Guid? purchaseOrderId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                status,
                supplierId,
                partId,
                purchaseOrderId,
                cancellationToken));
        })
        .WithName($"ListWarrantyClaims{nameSuffix}");

        group.MapGet("/{warrantyClaimId:guid}", async (
            Guid warrantyClaimId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, warrantyClaimId, cancellationToken));
        })
        .WithName($"GetWarrantyClaim{nameSuffix}");

        group.MapPost("/", async (
            CreateSupplierWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName($"CreateWarrantyClaim{nameSuffix}");

        group.MapPut("/{warrantyClaimId:guid}", async (
            Guid warrantyClaimId,
            UpdateWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"UpdateWarrantyClaim{nameSuffix}");

        group.MapPost("/{warrantyClaimId:guid}/submit", async (
            Guid warrantyClaimId,
            SubmitWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SubmitAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"SubmitWarrantyClaim{nameSuffix}");

        group.MapPost("/{warrantyClaimId:guid}/record-supplier-response", async (
            Guid warrantyClaimId,
            RecordWarrantyClaimSupplierResponseRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RecordSupplierResponseAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"RecordWarrantyClaimSupplierResponse{nameSuffix}");

        group.MapPost("/{warrantyClaimId:guid}/close", async (
            Guid warrantyClaimId,
            CloseWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CloseAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"CloseWarrantyClaim{nameSuffix}");

        group.MapPost("/{warrantyClaimId:guid}/deny", async (
            Guid warrantyClaimId,
            DenyWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.DenyAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"DenyWarrantyClaim{nameSuffix}");

        group.MapPost("/{warrantyClaimId:guid}/cancel", async (
            Guid warrantyClaimId,
            CancelWarrantyClaimRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CancelAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName($"CancelWarrantyClaim{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/warranty-claims"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/warranty-claims"), "V1");
    }
}
