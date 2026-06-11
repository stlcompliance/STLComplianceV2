using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class VendorOrderSettingsEndpoints
{
    public static void MapSupplyArrVendorOrderSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorOrderSettings").RequireAuthorization();

            group.MapGet("/", async (
                SupplyArrAuthorizationService authorization,
                VendorOrderSettingsService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderSettingsManage(context.User);
                return Results.Ok(await service.GetAsync(context.User.GetTenantId(), cancellationToken));
            })
            .WithName($"GetVendorOrderSettings{nameSuffix}");

            group.MapPut("/", async (
                UpsertVendorOrderSettingsRequest request,
                SupplyArrAuthorizationService authorization,
                VendorOrderSettingsService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireVendorOrderSettingsManage(context.User);
                return Results.Ok(await service.UpsertAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"UpsertVendorOrderSettings{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/vendor-order-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/vendor-order-settings"), "V1");
    }
}
