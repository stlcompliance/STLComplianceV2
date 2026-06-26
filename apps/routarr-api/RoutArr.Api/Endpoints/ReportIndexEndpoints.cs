using RoutArr.Api.Services;

namespace RoutArr.Api.Endpoints;

public static class ReportIndexEndpoints
{
    private static readonly object[] Reports =
    [
        new { Key = "dispatch", Path = "/api/v1/reports/dispatch" },
        new { Key = "dispatch-time-summary", Path = "/api/v1/reports/dispatch/time-summary" },
        new { Key = "routes", Path = "/api/v1/reports/routes" },
        new { Key = "proof-dvir", Path = "/api/v1/reports/proof-dvir" },
        new { Key = "dispatch-overrides", Path = "/api/v1/reports/dispatch-overrides" }
    ];

    public static void MapRoutArrReportIndexEndpoints(this WebApplication app)
    {
        static IResult Get(RoutArrAuthorizationService authorization, HttpContext context)
        {
            authorization.RequireDispatchReportRead(context.User);
            return Results.Ok(new { Reports });
        }

        app.MapGet("/api/reports", Get)
            .WithTags("Reports")
            .RequireAuthorization()
            .WithName("ListRoutArrReportGroups");

        app.MapGet("/api/v1/reports", Get)
            .WithTags("Reports")
            .RequireAuthorization()
            .WithName("ListRoutArrReportGroupsV1");
    }
}
