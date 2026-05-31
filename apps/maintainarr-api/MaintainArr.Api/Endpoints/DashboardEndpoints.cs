using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapMaintainArrDashboardEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/dashboard", Suffix: string.Empty),
            (Route: "/api/v1/dashboard", Suffix: "V1"),
            (Route: "/api/command-center", Suffix: "CommandCenter"),
            (Route: "/api/v1/command-center", Suffix: "CommandCenterV1"),
        };

        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("Dashboard")
                .RequireAuthorization();

            group.MapGet("/", async (
                DashboardService dashboardService,
                MaintainArrAuthorizationService authorization,
                IMaintainArrAuditService audit,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireExecutiveReportRead(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var dashboard = await dashboardService.GetAsync(tenantId, cancellationToken);
                await audit.WriteAsync(
                    "maintainarr.dashboard.read",
                    tenantId,
                    actorUserId,
                    "maintainarr_dashboard",
                    null,
                    "success",
                    cancellationToken: cancellationToken);
                return Results.Ok(dashboard);
            })
            .WithName($"GetMaintainArrDashboard{suffix}");
        }
    }
}
