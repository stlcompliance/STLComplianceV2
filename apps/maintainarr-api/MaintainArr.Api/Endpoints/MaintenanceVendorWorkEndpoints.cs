using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceVendorWorkEndpoints
{
    public static void MapMaintainArrMaintenanceVendorWorkEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/work-orders/{workOrderId:guid}/vendor-work").WithTags("VendorWork").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/work-orders/{workOrderId:guid}/vendor-work").WithTags("VendorWork").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid workOrderId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenanceVendorWorkService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVendorWorkRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, workOrderId, cancellationToken));
        })
        .WithName($"ListMaintenanceVendorWork{nameSuffix}");

        group.MapPost("/", async (
            Guid workOrderId,
            UpsertMaintenanceVendorWorkRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenanceVendorWorkService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVendorWorkManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpsertAsync(tenantId, actorUserId, workOrderId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName($"UpsertMaintenanceVendorWork{nameSuffix}");
    }
}
