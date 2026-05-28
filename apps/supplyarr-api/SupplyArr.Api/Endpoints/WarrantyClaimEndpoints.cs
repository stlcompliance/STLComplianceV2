using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class WarrantyClaimEndpoints
{
    public static void MapSupplyArrWarrantyClaimEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/warranty-claims")
            .WithTags("WarrantyClaims")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            Guid? vendorPartyId,
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
                vendorPartyId,
                partId,
                purchaseOrderId,
                cancellationToken));
        })
        .WithName("ListWarrantyClaims");

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
        .WithName("GetWarrantyClaim");

        group.MapPost("/", async (
            CreateWarrantyClaimRequest request,
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
        .WithName("CreateWarrantyClaim");

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
        .WithName("UpdateWarrantyClaim");

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
        .WithName("SubmitWarrantyClaim");

        group.MapPost("/{warrantyClaimId:guid}/record-vendor-response", async (
            Guid warrantyClaimId,
            RecordWarrantyClaimVendorResponseRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            WarrantyClaimService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReturnManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.RecordVendorResponseAsync(
                tenantId,
                actorUserId,
                warrantyClaimId,
                request,
                cancellationToken));
        })
        .WithName("RecordWarrantyClaimVendorResponse");

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
        .WithName("CloseWarrantyClaim");

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
        .WithName("DenyWarrantyClaim");

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
        .WithName("CancelWarrantyClaim");
    }
}
