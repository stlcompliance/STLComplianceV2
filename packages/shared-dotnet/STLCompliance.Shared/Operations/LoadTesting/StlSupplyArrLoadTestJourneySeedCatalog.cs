namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// SupplyArr load-test journey seed conventions for k6 procurement and Playwright demand-processing smokes.
/// </summary>
public static class StlSupplyArrLoadTestJourneySeedCatalog
{
    public const string SeedEndpointPath = "/api/load-test-journey/seed";

    public const string JourneyDemandRefTitle = "Load Test Journey Demand Processing Ref";
    public const string JourneyWorkOrderNumber = "WO-LTJ-DP-100";
    public const string JourneyPartKey = "LTJ-DP-PART";

    public const string JourneyDemandRefIdEnvVar = "STL_LOAD_JOURNEY_DEMAND_REF_ID";
}
