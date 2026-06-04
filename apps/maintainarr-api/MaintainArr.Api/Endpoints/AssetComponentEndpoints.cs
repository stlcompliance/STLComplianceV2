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
    }
}
