using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class OperatorDashboardEndpoints
{
    public static void MapComplianceCoreOperatorDashboardEndpoints(this WebApplication app)
    {
        var dashboards = app.MapGroup("/api/dashboards")
            .WithTags("Dashboards")
            .RequireAuthorization();

        dashboards.MapGet("/operator", async (
            ComplianceCoreAuthorizationService authorization,
            OperatorDashboardService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireOperatorDashboardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var response = await service.GetOperatorDashboardAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetComplianceCoreOperatorDashboard");
    }
}
