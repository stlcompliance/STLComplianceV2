using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetTypeEndpoints
{
    public static void MapMaintainArrAssetTypeEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/asset-types").WithTags("AssetTypes").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/asset-types").WithTags("AssetTypes").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetTypeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListAssetTypes{nameSuffix}");

        group.MapPost("/", async (
            CreateAssetTypeRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetTypeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/asset-types/{created.AssetTypeId}", created);
        })
        .WithName($"CreateAssetType{nameSuffix}");

        group.MapPut("/{assetTypeId:guid}", async (
            Guid assetTypeId,
            UpdateAssetTypeRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetTypeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, assetTypeId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetType{nameSuffix}");

        group.MapPatch("/{assetTypeId:guid}/status", async (
            Guid assetTypeId,
            UpdateAssetTypeStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetTypeService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, assetTypeId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetTypeStatus{nameSuffix}");
    }
}
