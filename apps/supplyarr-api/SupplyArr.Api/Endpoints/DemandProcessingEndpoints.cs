using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class DemandProcessingEndpoints
{
    public static void MapSupplyArrDemandProcessingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/demand-processing")
            .WithTags("DemandProcessing")
            .RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDashboardAsync(tenantId, cancellationToken));
        })
        .WithName("GetSupplyArrDemandProcessingDashboard");

        group.MapGet("/{demandRefId:guid}", async (
            Guid demandRefId,
            SupplyArrAuthorizationService authorization,
            DemandProcessingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDemandProcessingRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDetailAsync(tenantId, demandRefId, cancellationToken));
        })
        .WithName("GetSupplyArrDemandProcessingDetail");
    }
}
