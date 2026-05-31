using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetReadinessEndpoints
{
    public static void MapMaintainArrAssetReadinessEndpoints(this WebApplication app)
    {
        static async Task<IResult> HandleAssetReadinessAsync(
            Guid? assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReadinessService service,
            CancellationToken cancellationToken)
        {
            authorization.RequireAssetReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();

            if (assetId is { } id)
            {
                if (id == Guid.Empty)
                {
                    return Results.BadRequest(new { error = "assetId must be a non-empty GUID when provided." });
                }

                return Results.Ok(await service.GetAsync(
                    tenantId,
                    id,
                    cancellationToken,
                    context.User.GetUserId()));
            }

            return Results.Ok(await service.ListFleetAsync(tenantId, cancellationToken));
        }

        var legacyGroup = app.MapGroup("/api/asset-readiness").WithTags("AssetReadiness").RequireAuthorization();
        legacyGroup.MapGet("/", HandleAssetReadinessAsync).WithName("GetAssetReadiness");
        legacyGroup.MapGet("/assets/{assetId:guid}", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReadinessService service,
            CancellationToken cancellationToken) =>
        {
            return await HandleAssetReadinessAsync(
                assetId,
                context,
                authorization,
                service,
                cancellationToken);
        }).WithName("GetAssetReadinessByAssetId");

        // v1 alias for documented MaintainArr readiness contract.
        var v1Group = app.MapGroup("/api/v1/readiness").WithTags("AssetReadiness").RequireAuthorization();
        v1Group.MapGet("/", HandleAssetReadinessAsync).WithName("GetAssetReadinessV1");
        v1Group.MapGet("/assets/{assetId:guid}", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReadinessService service,
            CancellationToken cancellationToken) =>
        {
            return await HandleAssetReadinessAsync(
                assetId,
                context,
                authorization,
                service,
                cancellationToken);
        }).WithName("GetAssetReadinessV1ByAssetId");
    }
}
