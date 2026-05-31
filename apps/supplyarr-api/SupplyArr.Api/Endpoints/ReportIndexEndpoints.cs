using SupplyArr.Api.Services;

namespace SupplyArr.Api.Endpoints;

public static class ReportIndexEndpoints
{
    private static readonly object[] Reports =
    [
        new { Key = "dashboard", Path = "/api/v1/dashboard" },
        new { Key = "readiness", Path = "/api/v1/supply-readiness/dashboard" },
        new { Key = "vendors", Path = "/api/v1/reports/vendors" },
        new { Key = "parts-inventory", Path = "/api/v1/reports/parts-inventory" },
        new { Key = "purchasing", Path = "/api/v1/reports/purchasing" },
        new { Key = "compliance", Path = "/api/v1/reports/compliance" }
    ];

    public static void MapSupplyArrReportIndexEndpoints(this WebApplication app)
    {
        static IResult Get(SupplyArrAuthorizationService authorization, HttpContext context)
        {
            authorization.RequireSupplyArrEntitlement(context.User);
            return Results.Ok(new { Reports });
        }

        app.MapGet("/api/reports", Get)
            .WithTags("Reports")
            .RequireAuthorization()
            .WithName("ListSupplyArrReportGroups");

        app.MapGet("/api/v1/reports", Get)
            .WithTags("Reports")
            .RequireAuthorization()
            .WithName("ListSupplyArrReportGroupsV1");
    }
}
