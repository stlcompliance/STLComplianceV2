using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class LeadTimeSnapshotEndpoints
{
    public static void MapSupplyArrLeadTimeSnapshotEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/lead-time-snapshots").WithTags("LeadTimeSnapshots").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partVendorLinkId,
            Guid? partId,
            Guid? vendorPartyId,
            DateTimeOffset? asOf,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotService service,
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
        .WithName("ListLeadTimeSnapshots");

        group.MapGet("/{leadTimeSnapshotId:guid}", async (
            Guid leadTimeSnapshotId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, leadTimeSnapshotId, cancellationToken));
        })
        .WithName("GetLeadTimeSnapshot");

        group.MapPost("/", async (
            CreateLeadTimeSnapshotRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            LeadTimeSnapshotService service,
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
            return Results.Created($"/api/lead-time-snapshots/{created.LeadTimeSnapshotId}", created);
        })
        .WithName("CreateLeadTimeSnapshot");
    }
}
