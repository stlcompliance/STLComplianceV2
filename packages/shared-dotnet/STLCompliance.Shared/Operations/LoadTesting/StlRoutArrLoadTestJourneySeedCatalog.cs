namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// RoutArr load-test journey seed conventions for k6 dispatch workflow gate scenarios.
/// </summary>
public static class StlRoutArrLoadTestJourneySeedCatalog
{
    public const string SeedEndpointPath = "/api/load-test-journey/seed";

    public const string JourneyTripTitle = "Load Test Journey Dispatch Trip";
    public const string JourneyTripDescription =
        "Idempotent load-test journey trip mirror for k6 dispatch workflow gate checks.";

    public static Guid SubjectPersonId => Guid.Parse(StlLoadTestJourneyDefaults.SubjectPersonId);
}
