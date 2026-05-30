using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetStatusRollupEndpoints
{
    public static void MapMaintainArrAssetStatusRollupEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/asset-status-rollups", Suffix: string.Empty),
            (Route: "/api/v1/asset-status-rollups", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
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
            .WithName($"GetMaintainArrFleetAssetStatusRollup{suffix}");

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
            .WithName($"ListMaintainArrAssetTypeStatusRollups{suffix}");

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
            .WithName($"GetMaintainArrAssetTypeStatusRollup{suffix}");

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
            .WithName($"ListMaintainArrAssetClassStatusRollups{suffix}");

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
            .WithName($"ListMaintainArrSiteStatusRollups{suffix}");

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
            .WithName($"ListMaintainArrAssetStatusRollups{suffix}");

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
            .WithName($"GetMaintainArrAssetStatusRollup{suffix}");
        }
    }
}

