using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class AvailabilitySnapshotEndpoints
{
    public static void MapSupplyArrAvailabilitySnapshotEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("AvailabilitySnapshots").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partVendorLinkId,
            Guid? partId,
            Guid? vendorPartyId,
            DateTimeOffset? asOf,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotService service,
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
        .WithName($"ListAvailabilitySnapshots{nameSuffix}");

        group.MapGet("/{availabilitySnapshotId:guid}", async (
            Guid availabilitySnapshotId,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, availabilitySnapshotId, cancellationToken));
        })
        .WithName($"GetAvailabilitySnapshot{nameSuffix}");

        group.MapPost("/", async (
            CreateAvailabilitySnapshotRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            AvailabilitySnapshotService service,
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
            return Results.Created($"/api/availability-snapshots/{created.AvailabilitySnapshotId}", created);
        })
        .WithName($"CreateAvailabilitySnapshot{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/availability-snapshots"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/availability-snapshots"), "V1");
    }
}
