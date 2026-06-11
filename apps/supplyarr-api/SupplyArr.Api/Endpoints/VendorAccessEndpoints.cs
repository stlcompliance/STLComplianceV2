using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class VendorAccessEndpoints
{
    public static void MapSupplyArrVendorAccessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/vendor-access/orders").WithTags("VendorAccess");

        group.MapGet("/{token}", async (
            string token,
            VendorOrderService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetVendorAccessAsync(token, cancellationToken));
        })
        .AllowAnonymous()
        .WithName("GetVendorAccessOrder");

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
        .WithName("UpdateVendorAccessOrderStatus");

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
        .WithName("RegisterVendorAccessOrderDocument");
    }
}
