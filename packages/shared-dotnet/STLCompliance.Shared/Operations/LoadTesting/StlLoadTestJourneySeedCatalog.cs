namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Compliance Core load-test journey seed conventions for k6 TrainArr and RoutArr scenarios.
/// </summary>
public static class StlLoadTestJourneySeedCatalog
{
    public const string SeedEndpointPath = "/api/load-test-journey/seed";

    public const string DriverLicenseValidFactKey = "driver_license_valid";
    public const string DriverLicenseFactSourceKey = "load_test_journey_license_flag";

    public const string GoverningBodyKey = "dot";
    public const string JurisdictionKey = "us_federal";
    public const string RegulatoryProgramKey = "fmcsa_driver_compliance";

    public const string RulePackLabel = "Driver Qualification Rules";
    public const string RulePackDescription = "Load-test journey rule pack for TrainArr qualification checks and RoutArr dispatch gates.";

    public static string RulePackKey => StlLoadTestJourneyDefaults.RulePackKey;
    public static string QualificationKey => StlLoadTestJourneyDefaults.QualificationKey;
}
