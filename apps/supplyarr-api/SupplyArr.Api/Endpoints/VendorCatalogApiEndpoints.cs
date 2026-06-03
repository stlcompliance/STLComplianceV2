using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorCatalogApiEndpoints
{
    public static void MapSupplyArrVendorCatalogApiEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/vendor-catalogs")
            .WithTags("VendorCatalogApi")
            .RequireAuthorization();

        group.MapPost("/sync", async (
            VendorCatalogApiSyncRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            VendorCatalogApiService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SyncAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("SyncVendorCatalogApiV1");
    }
}
