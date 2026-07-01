using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class SupplierAccessEndpoints
{
    public static void MapSupplyArrSupplierAccessEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
            group = group.WithTags("SupplierAccess");

            group.MapGet("/{token}", async (
                string token,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.GetSupplierAccessAsync(token, cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"GetSupplierAccessOrder{nameSuffix}");

            group.MapPost("/{token}/status", async (
                string token,
                UpdateSupplierOrderStatusRequest request,
                HttpContext context,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers.UserAgent.ToString();
                return Results.Ok(await service.SubmitSupplierAccessStatusAsync(
                    token,
                    request,
                    remoteIp,
                    userAgent,
                    cancellationToken));
                })
            .AllowAnonymous()
            .WithName($"UpdateSupplierAccessOrderStatus{nameSuffix}");

            group.MapPost("/{token}/documents", async (
                string token,
                RegisterSupplierOrderDocumentRequest request,
                SupplierOrderService service,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(await service.RegisterSupplierAccessDocumentAsync(
                    token,
                    request,
                    cancellationToken));
            })
            .AllowAnonymous()
            .WithName($"RegisterSupplierAccessOrderDocument{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/v1/supplier-access/orders"), "SupplierV1");
    }
}
