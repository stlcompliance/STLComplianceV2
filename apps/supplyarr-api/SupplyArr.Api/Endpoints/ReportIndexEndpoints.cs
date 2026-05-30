namespace SupplyArr.Api.Endpoints;

public static class ReportIndexEndpoints
{
    private static readonly string[] Groups =
    [
        "vendors",
        "parts-inventory",
        "purchasing",
        "compliance"
    ];

    public static void MapSupplyArrReportIndexEndpoints(this WebApplication app)
    {
        static IResult Get() => Results.Ok(Groups);

        app.MapGet("/api/reports", Get)
            .WithTags("Reports")
            .WithName("ListSupplyArrReportGroups");

        app.MapGet("/api/v1/reports", Get)
            .WithTags("Reports")
            .WithName("ListSupplyArrReportGroupsV1");
    }
}

