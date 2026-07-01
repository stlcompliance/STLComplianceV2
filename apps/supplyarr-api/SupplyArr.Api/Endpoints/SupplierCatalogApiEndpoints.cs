using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierCatalogApiEndpoints
{
    public static void MapSupplyArrSupplierCatalogApiEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/supplier-catalogs")
            .WithTags("SupplierCatalogApi")
            .RequireAuthorization();

        group.MapPost("/sync", async (
            SupplierCatalogApiSyncRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierCatalogApiService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePartsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.SyncAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("SyncSupplierCatalogApiV1");
    }
}
