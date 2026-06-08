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

        group.MapGet("/{assetId:guid}/telematics-ingestion", async (
            Guid assetId,
            int? limit,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetTelematicsIngestionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, assetId, limit, cancellationToken));
        })
        .WithName($"ListAssetTelematicsIngestion{nameSuffix}");

        if (nameSuffix == "V1")
        {
            group.MapGet("/search", async (
                string? query,
                string? status,
                string? siteRef,
                int? limit,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                AssetService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.SearchAsync(
                    tenantId,
                    query,
                    status,
                    siteRef,
                    limit ?? 25,
                    cancellationToken));
            })
            .WithName("SearchAssetsV1");
        }

        if (nameSuffix == "V1")
        {
            group.MapPost("/", async (
                AssetUpsertV1Request request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                AssetService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.CreateV1Async(
                    tenantId,
                    actorUserId,
                    context.User.GetPersonId().ToString("D"),
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/assets/{created.AssetId}", created);
            })
            .WithName("CreateAssetV1");
        }
        else
        {
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
        }

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

        if (nameSuffix == "V1")
        {
            group.MapPatch("/{assetId:guid}", async (
                Guid assetId,
                AssetUpsertV1Request request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                AssetService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetsManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateV1Async(
                    tenantId,
                    actorUserId,
                    context.User.GetPersonId().ToString("D"),
                    assetId,
                    request,
                    cancellationToken);
                return Results.Ok(updated);
            })
            .WithName("UpdateAssetControlledV1");

            group.MapGet("/{assetId:guid}/field-context", async (
                Guid assetId,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                AssetService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetFieldContextAsync(tenantId, assetId, cancellationToken));
            })
            .WithName("GetAssetFieldContextV1");
        }

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
