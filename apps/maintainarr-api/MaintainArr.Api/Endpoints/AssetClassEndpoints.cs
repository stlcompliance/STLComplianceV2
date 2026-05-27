using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetClassEndpoints
{
    public static void MapMaintainArrAssetClassEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/asset-classes").WithTags("AssetClasses").RequireAuthorization();

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
        .WithName("ListAssetClasses");

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
        .WithName("CreateAssetClass");

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
        .WithName("UpdateAssetClass");

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
        .WithName("UpdateAssetClassStatus");
    }
}
