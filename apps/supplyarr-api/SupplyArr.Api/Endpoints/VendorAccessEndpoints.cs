using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class VendorAccessEndpoints
{
    public static void MapSupplyArrVendorAccessEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("VendorAccess");

            group.MapGet("/{token}", async (
                string token,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.GetVendorAccessAsync(token, cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"GetVendorAccessOrder{nameSuffix}");

            group.MapPost("/{token}/status", async (
                string token,
                UpdateVendorOrderStatusRequest request,
                HttpContext context,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers.UserAgent.ToString();
                return Results.Ok(await service.SubmitVendorAccessStatusAsync(
                    token,
                    request,
                    remoteIp,
                    userAgent,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"UpdateVendorAccessOrderStatus{nameSuffix}");

            group.MapPost("/{token}/documents", async (
                string token,
                RegisterVendorOrderDocumentRequest request,
                VendorOrderService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.RegisterVendorAccessDocumentAsync(
                    token,
                    request,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"RegisterVendorAccessOrderDocument{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-access/orders"), "SupplierV1");
        MapRoutes(app.MapGroup("/api/v1/vendor-access/orders"), string.Empty);
    }
}
