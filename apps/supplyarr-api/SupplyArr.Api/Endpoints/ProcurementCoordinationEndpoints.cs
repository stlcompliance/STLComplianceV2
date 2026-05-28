using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementCoordinationEndpoints
{
    public static void MapSupplyArrProcurementCoordinationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/procurement-coordination")
            .WithTags("ProcurementCoordination")
            .RequireAuthorization();

        group.MapGet("/", async (
            string? coordinationStage,
            bool? activeOnly,
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationService coordinationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await coordinationService.GetDashboardAsync(
                tenantId,
                coordinationStage,
                activeOnly,
                cancellationToken));
        })
        .WithName("GetSupplyArrProcurementCoordinationDashboard");

        group.MapGet("/{subjectType}/{subjectId:guid}", async (
            string subjectType,
            Guid subjectId,
            SupplyArrAuthorizationService authorization,
            ProcurementCoordinationService coordinationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementCoordinationRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await coordinationService.GetAsync(
                tenantId,
                subjectType,
                subjectId,
                cancellationToken));
        })
        .WithName("GetSupplyArrProcurementCoordinationDetail");
    }
}
