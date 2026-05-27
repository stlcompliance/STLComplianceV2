using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class AssetDispatchabilityEndpoints
{
    public static void MapRoutArrAssetDispatchabilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/asset-dispatchability")
            .WithTags("AssetDispatchability")
            .RequireAuthorization();

        group.MapPost("/check", async (
            AssetDispatchabilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            AssetDispatchabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await service.CheckAsync(
                tenantId,
                actorUserId,
                request.VehicleRefKey,
                request.AssetTag,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CheckAssetDispatchability");
    }
}
