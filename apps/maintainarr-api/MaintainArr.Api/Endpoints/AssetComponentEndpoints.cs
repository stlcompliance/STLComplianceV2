using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetComponentEndpoints
{
    public static void MapMaintainArrAssetComponentEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/assets/{assetId:guid}").WithTags("AssetComponents").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/assets/{assetId:guid}").WithTags("AssetComponents").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/components", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assetService,
            AssetInstalledComponentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var asset = await assetService.GetAsync(tenantId, assetId, cancellationToken);
            return Results.Ok(await service.ListAsync(tenantId, asset.AssetId, cancellationToken));
        })
        .WithName($"ListAssetComponents{nameSuffix}");

        group.MapPost("/components", async (
            Guid assetId,
            CreateAssetInstalledComponentRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assetService,
            AssetInstalledComponentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var asset = await assetService.GetAsync(tenantId, assetId, cancellationToken);
            var created = await service.CreateAsync(
                tenantId,
                actorUserId,
                asset.AssetId,
                request,
                cancellationToken);
            return Results.Created($"/api/assets/{assetId}/components/{created.ComponentId}", created);
        })
        .WithName($"CreateAssetComponent{nameSuffix}");

        group.MapPatch("/components/{componentId:guid}", async (
            Guid assetId,
            Guid componentId,
            UpdateAssetInstalledComponentRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assetService,
            AssetInstalledComponentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var asset = await assetService.GetAsync(tenantId, assetId, cancellationToken);
            var updated = await service.UpdateAsync(
                tenantId,
                actorUserId,
                asset.AssetId,
                componentId,
                request,
                cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetComponent{nameSuffix}");
    }
}
