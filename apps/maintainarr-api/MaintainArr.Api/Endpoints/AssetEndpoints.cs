using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetEndpoints
{
    public static void MapMaintainArrAssetEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/assets").WithTags("Assets").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/assets").WithTags("Assets").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName($"ListAssets{nameSuffix}");

        group.MapGet("/{assetId:guid}", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, assetId, cancellationToken));
        })
        .WithName($"GetAsset{nameSuffix}");

        group.MapPost("/", async (
            CreateAssetRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/assets/{created.AssetId}", created);
        })
        .WithName($"CreateAsset{nameSuffix}");

        group.MapPut("/{assetId:guid}", async (
            Guid assetId,
            UpdateAssetRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, assetId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAsset{nameSuffix}");

        group.MapPatch("/{assetId:guid}/lifecycle-status", async (
            Guid assetId,
            UpdateAssetLifecycleStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateLifecycleStatusAsync(tenantId, actorUserId, assetId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpdateAssetLifecycleStatus{nameSuffix}");
    }
}
