using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenanceHistoryEndpoints
{
    public static void MapMaintainArrMaintenanceHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/maintenance-history").WithTags("MaintenanceHistory").RequireAuthorization();

        group.MapGet("/", async (
            Guid assetId,
            int? page,
            int? pageSize,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintenanceHistoryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMaintenanceHistoryRead(context.User);
            if (assetId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "assetId is required." });
            }

            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                assetId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("ListMaintenanceHistory");
    }
}
