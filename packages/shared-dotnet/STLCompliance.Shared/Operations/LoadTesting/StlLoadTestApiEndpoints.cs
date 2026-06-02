namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Canonical local docker-compose API base URLs for load-test scenarios.
/// </summary>
public static class StlLoadTestApiEndpoints
{
    public const string NexArr = "http://localhost:5101";
    public const string StaffArr = "http://localhost:5102";
    public const string TrainArr = "http://localhost:5103";
    public const string MaintainArr = "http://localhost:5104";
    public const string RoutArr = "http://localhost:5105";
    public const string SupplyArr = "http://localhost:5106";
    public const string ComplianceCore = "http://localhost:5107";
    public const string LoadArr = "http://localhost:5108";

    public static readonly IReadOnlyList<(string ProductKey, string BaseUrl)> All =
    [
        ("nexarr", NexArr),
        ("staffarr", StaffArr),
        ("trainarr", TrainArr),
        ("maintainarr", MaintainArr),
        ("routarr", RoutArr),
        ("supplyarr", SupplyArr),
        ("compliancecore", ComplianceCore),
        ("loadarr", LoadArr),
    ];
}
