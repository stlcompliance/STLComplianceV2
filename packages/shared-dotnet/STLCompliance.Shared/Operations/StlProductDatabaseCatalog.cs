namespace STLCompliance.Shared.Operations;

/// <summary>
/// Canonical PostgreSQL database names for each product API (local docker-compose and Render V1).
/// </summary>
public static class StlProductDatabaseCatalog
{
    public const string NexArr = "nexarr";
    public const string StaffArr = "staffarr";
    public const string TrainArr = "trainarr";
    public const string MaintainArr = "maintainarr";
    public const string RoutArr = "routarr";
    public const string SupplyArr = "supplyarr";
    public const string ComplianceCore = "compliancecore";
    public const string LoadArr = "loadarr";
    public const string RecordArr = "recordarr";
    public const string ReportArr = "reportarr";
    public const string AssurArr = "assurarr";

    public static readonly IReadOnlyList<string> All =
    [
        NexArr,
        StaffArr,
        TrainArr,
        MaintainArr,
        RoutArr,
        SupplyArr,
        ComplianceCore,
        LoadArr,
        RecordArr,
        ReportArr,
        AssurArr,
    ];

    public static bool IsKnownProductDatabase(string databaseName) =>
        All.Contains(databaseName, StringComparer.OrdinalIgnoreCase);
}
