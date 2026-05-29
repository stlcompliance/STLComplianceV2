using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetReadinessEndpoints
{
    public static void MapMaintainArrAssetReadinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/asset-readiness").WithTags("AssetReadiness").RequireAuthorization();

        group.MapGet("/", async (
            Guid? assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetReadinessService service,
            CancellationToken cancellationToken) =>
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
        })
        .WithName("GetAssetReadiness");
    }
}
