using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetClassEndpoints
{
    public static void MapMaintainArrAssetClassEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/asset-classes").WithTags("AssetClasses").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/asset-classes").WithTags("AssetClasses").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetClassService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListAssetClasses{nameSuffix}");

        group.MapPost("/", async (
            CreateAssetClassRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetClassService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/asset-classes/{created.AssetClassId}", created);
        })
        .WithName($"CreateAssetClass{nameSuffix}");

        group.MapPut("/{assetClassId:guid}", async (
            Guid assetClassId,
            UpdateAssetClassRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetClassService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, assetClassId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetClass{nameSuffix}");

        group.MapPatch("/{assetClassId:guid}/status", async (
            Guid assetClassId,
            UpdateAssetClassStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetClassService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, assetClassId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetClassStatus{nameSuffix}");
    }
}
