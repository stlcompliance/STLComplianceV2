using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class PersonalTrainingDashboardEndpoints
{
    public static void MapTrainArrPersonalTrainingDashboardEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/me/training", Suffix: string.Empty),
            (Route: "/api/v1/me/training", Suffix: "V1"),
        };

        foreach (var (route, suffix) in routes)
        {
            var group = app.MapGroup(route)
                .WithTags("Me")
                .RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                TrainArrAuthorizationService authorization,
                PersonalTrainingDashboardService dashboardService,
                CancellationToken cancellationToken) =>
            {
                var staffarrPersonId = context.User.GetPersonId();
                authorization.RequireAssignmentsRead(context.User, staffarrPersonId);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await dashboardService.GetAsync(
                    tenantId,
                    staffarrPersonId,
                    cancellationToken));
            })
            .WithName($"GetPersonalTrainingDashboard{suffix}");
        }
    }
}
