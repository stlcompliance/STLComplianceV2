using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class PricingSnapshotEndpoints
{
    public static void MapSupplyArrPricingSnapshotEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pricing-snapshots").WithTags("PricingSnapshots").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partVendorLinkId,
            Guid? partId,
            Guid? vendorPartyId,
            DateTimeOffset? asOf,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PricingSnapshotService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                partVendorLinkId,
                partId,
                vendorPartyId,
                asOf,
                cancellationToken));
        })
        .WithName("ListPricingSnapshots");

        group.MapGet("/{pricingSnapshotId:guid}", async (
            Guid pricingSnapshotId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PricingSnapshotService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, pricingSnapshotId, cancellationToken));
        })
        .WithName("GetPricingSnapshot");

        group.MapPost("/", async (
            CreatePricingSnapshotRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            PricingSnapshotService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Created($"/api/pricing-snapshots/{created.PricingSnapshotId}", created);
        })
        .WithName("CreatePricingSnapshot");
    }
}
