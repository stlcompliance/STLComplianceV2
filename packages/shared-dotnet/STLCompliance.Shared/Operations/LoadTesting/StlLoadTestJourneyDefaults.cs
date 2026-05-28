namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Demo journey inputs for cross-product k6 load-test scenarios.
/// </summary>
public static class StlLoadTestJourneyDefaults
{
    public const string SubjectPersonId = "22222222-2222-2222-2222-222222222201";
    public const string QualificationKey = "hazmat_endorsement";
    public const string RulePackKey = "driver_qualification";

    public const string SubjectPersonIdEnvVar = "STL_LOAD_SUBJECT_PERSON_ID";
    public const string QualificationKeyEnvVar = "STL_LOAD_QUALIFICATION_KEY";
    public const string RulePackKeyEnvVar = "STL_LOAD_RULE_PACK_KEY";
    public const string JourneyTripIdEnvVar = "STL_LOAD_JOURNEY_TRIP_ID";
}
