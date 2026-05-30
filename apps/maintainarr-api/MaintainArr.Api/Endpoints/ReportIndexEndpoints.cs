using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class ReportIndexEndpoints
{
    public static void MapMaintainArrReportIndexEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/reports", Suffix: string.Empty),
            (Route: "/api/v1/reports", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("Reports")
                .RequireAuthorization();

            group.MapGet("/", (
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireAssetsRead(context.User);
                return Results.Ok(new
                {
                    Reports = new[]
                    {
                        new { Key = "maintenance", Path = "/api/v1/reports/maintenance" },
                        new { Key = "executive", Path = "/api/v1/reports/executive" },
                        new { Key = "compliance", Path = "/api/v1/reports/compliance" }
                    }
                });
            })
            .WithName($"ListMaintainArrReportsIndex{suffix}");
        }
    }
}
