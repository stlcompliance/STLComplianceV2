using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplierOrderSettingsEndpoints
{
    public static void MapSupplyArrSupplierOrderSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("SupplierOrderSettings").RequireAuthorization();

            group.MapGet("/", async (
                SupplyArrAuthorizationService authorization,
                SupplierOrderSettingsService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderSettingsManage(context.User);
                return Results.Ok(await service.GetAsync(context.User.GetTenantId(), cancellationToken));
            })
            .WithName($"GetSupplierOrderSettings{nameSuffix}");

            group.MapPut("/", async (
                UpsertSupplierOrderSettingsRequest request,
                SupplyArrAuthorizationService authorization,
                SupplierOrderSettingsService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireSupplierOrderSettingsManage(context.User);
                return Results.Ok(await service.UpsertAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"UpsertSupplierOrderSettings{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-order-settings"), "SupplierV1");
    }
}
