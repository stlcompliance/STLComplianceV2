using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceVendorWorkPortalEndpoints
{
    public static void MapMaintainArrMaintenanceVendorWorkPortalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/vendor-portal/work-orders/{workOrderId:guid}")
            .WithTags("VendorPortal");

        group.MapGet("/", async (
            Guid workOrderId,
            string accessCode,
            MaintenanceVendorWorkService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetPortalAsync(workOrderId, accessCode, cancellationToken));
        })
        .WithName("GetMaintenanceVendorWorkPortal");

        group.MapPost("/status", async (
            Guid workOrderId,
            string accessCode,
            UpdateMaintenanceVendorWorkPortalRequest request,
            MaintenanceVendorWorkService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdatePortalAsync(workOrderId, accessCode, request, cancellationToken));
        })
        .WithName("UpdateMaintenanceVendorWorkPortalStatus");
    }
}
