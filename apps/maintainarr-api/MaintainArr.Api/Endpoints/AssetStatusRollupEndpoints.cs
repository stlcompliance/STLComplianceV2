using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetStatusRollupEndpoints
{
    public static void MapMaintainArrAssetStatusRollupEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/asset-status-rollups")
            .WithTags("AssetStatusRollups")
            .RequireAuthorization();

        group.MapGet("/fleet", async (
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetFleetRollupAsync(tenantId, cancellationToken));
        })
        .WithName("GetMaintainArrFleetAssetStatusRollup");

        group.MapGet("/types", async (
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAssetTypeRollupsAsync(tenantId, cancellationToken));
        })
        .WithName("ListMaintainArrAssetTypeStatusRollups");

        group.MapGet("/types/{assetTypeId:guid}", async (
            Guid assetTypeId,
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAssetTypeRollupAsync(tenantId, assetTypeId, cancellationToken));
        })
        .WithName("GetMaintainArrAssetTypeStatusRollup");

        group.MapGet("/classes", async (
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAssetClassRollupsAsync(tenantId, cancellationToken));
        })
        .WithName("ListMaintainArrAssetClassStatusRollups");

        group.MapGet("/sites", async (
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSiteRollupsAsync(tenantId, cancellationToken));
        })
        .WithName("ListMaintainArrSiteStatusRollups");

        group.MapGet("/assets", async (
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAssetRollupsAsync(tenantId, cancellationToken));
        })
        .WithName("ListMaintainArrAssetStatusRollups");

        group.MapGet("/assets/{assetId:guid}", async (
            Guid assetId,
            MaintainArrAuthorizationService authorization,
            AssetStatusRollupService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetStatusRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAssetRollupAsync(tenantId, assetId, cancellationToken));
        })
        .WithName("GetMaintainArrAssetStatusRollup");
    }
}
