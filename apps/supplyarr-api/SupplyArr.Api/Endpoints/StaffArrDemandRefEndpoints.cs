using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class StaffArrDemandRefEndpoints
{
    public static void MapSupplyArrStaffArrDemandRefEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/staffarr-demand-refs").WithTags("StaffArrDemandRefs").RequireAuthorization();

        group.MapGet("/", async (
            string? status,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StaffArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName("ListStaffArrDemandRefs");

        group.MapGet("/{demandRefId:guid}", async (
            Guid demandRefId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StaffArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandRefRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, demandRefId, cancellationToken));
        })
        .WithName("GetStaffArrDemandRef");

        group.MapPost("/{demandRefId:guid}/create-purchase-request", async (
            Guid demandRefId,
            CreatePurchaseRequestFromStaffarrDemandRefRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            StaffArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePurchaseRequestCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreatePurchaseRequestFromDemandRefAsync(
                tenantId,
                actorUserId,
                demandRefId,
                request,
                cancellationToken);
            return Results.Created($"/api/purchase-requests/{created.PurchaseRequestId}", created);
        })
        .WithName("CreatePurchaseRequestFromStaffArrDemandRef");
    }
}
