namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// GitHub Actions schedule and secret conventions for weekly Render staging load soak.
/// </summary>
public static class StlRenderStagingLoadSoakScheduleCatalog
{
    public const string GitHubWorkflowFileName = "load-staging-render.yml";
    public const string GitHubWorkflowDisplayName = "Load Staging Render";

    /// <summary>Sunday 07:00 UTC — after daily docker-compose nightly (06:00 UTC).</summary>
    public const string WeeklyCronUtc = "0 7 * * 0";

    public static readonly IReadOnlyList<string> RequiredStagingApiUrlEnvironmentVariables =
        StlRenderStagingLoadTestCatalog.All
            .Select(entry => entry.SourceApiUrlEnvironmentVariable)
            .ToList();

    public static readonly IReadOnlyList<string> OptionalCredentialEnvironmentVariables =
    [
        StlLoadTestAuthDefaults.EmailEnvVar,
        StlLoadTestAuthDefaults.PasswordEnvVar,
        StlLoadTestAuthDefaults.TenantIdEnvVar,
        StlLoadTestJourneyDefaults.SubjectPersonIdEnvVar,
        StlLoadTestJourneyDefaults.QualificationKeyEnvVar,
        StlLoadTestJourneyDefaults.RulePackKeyEnvVar,
        StlLoadTestJourneyDefaults.JourneyTripIdEnvVar,
        StlLoadTestJourneyDefaults.JourneyRulePackIdEnvVar,
        StlLoadTestJourneyDefaults.DriverLicenseFactKeyEnvVar,
    ];

    public static IReadOnlyList<string> GetMissingStagingApiUrlEnvironmentVariables()
    {
        var missing = new List<string>();
        foreach (var environmentVariable in RequiredStagingApiUrlEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentVariable)))
            {
                missing.Add(environmentVariable);
            }
        }

        return missing;
    }

    public static bool AreStagingApiUrlsConfigured() =>
        GetMissingStagingApiUrlEnvironmentVariables().Count == 0;
}
