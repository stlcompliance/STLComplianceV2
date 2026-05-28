namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// TrainArr load-test journey seed conventions for k6 qualification-check scenarios.
/// </summary>
public static class StlTrainArrLoadTestJourneySeedCatalog
{
    public const string SeedEndpointPath = "/api/load-test-journey/seed";

    public const string JourneyDefinitionKey = "load_test_journey_hazmat_endorsement";
    public const string JourneyDefinitionName = "Load Test Journey Hazmat Endorsement";
    public const string JourneyQualificationName = "Hazmat Endorsement";
    public const string JourneyAssignmentReason = "load_test_journey_seed";
    public const string JourneyGrantPublicationMessage =
        "Load-test journey qualification grant mirror for k6 qualification checks.";

    public static Guid SubjectPersonId => Guid.Parse(StlLoadTestJourneyDefaults.SubjectPersonId);
    public static string QualificationKey => StlLoadTestJourneyDefaults.QualificationKey;
}
