using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class SupplyReadinessEndpoints
{
    public static void MapSupplyArrSupplyReadinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/supply-readiness")
            .WithTags("SupplyReadiness")
            .RequireAuthorization();

        group.MapGet("/dashboard", async (
            SupplyArrAuthorizationService authorization,
            SupplyReadinessService service,
            ISupplyArrAuditService audit,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSupplyReadinessRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var dashboard = await service.GetDashboardAsync(tenantId, cancellationToken);
            await audit.WriteAsync(
                "supplyarr.supply_readiness.dashboard",
                tenantId,
                actorUserId,
                "supply_readiness_dashboard",
                null,
                "success",
                cancellationToken: cancellationToken);
            return Results.Ok(dashboard);
        })
        .WithName("GetSupplyArrSupplyReadinessDashboard");
    }
}
