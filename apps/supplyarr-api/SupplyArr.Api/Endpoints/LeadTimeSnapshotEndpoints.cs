using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class LeadTimeSnapshotEndpoints
{
    public static void MapSupplyArrLeadTimeSnapshotEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("LeadTimeSnapshots").RequireAuthorization();

        group.MapGet("/", async (
            Guid? partSupplierLinkId,
            Guid? partId,
            Guid? supplierId,
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
                partSupplierLinkId,
                partId,
                supplierId,
                asOf,
                cancellationToken));
        })
        .WithName($"ListLeadTimeSnapshots{nameSuffix}");

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
        .WithName($"GetLeadTimeSnapshot{nameSuffix}");

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
        .WithName($"CreateLeadTimeSnapshot{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/lead-time-snapshots"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/lead-time-snapshots"), "V1");
    }
}
