using STLCompliance.Shared.Operations;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.Load.Tests;

[Trait("Category", "Load")]
public sealed class StlRenderStagingLoadTestSupportTests
{
    [Fact]
    public void Catalog_includes_seven_render_api_entries()
    {
        Assert.Equal(7, StlRenderStagingLoadTestCatalog.All.Count);
        Assert.Contains(
            StlRenderStagingLoadTestCatalog.All,
            entry => entry.ProductKey == StlProductDatabaseCatalog.NexArr
                && entry.SourceApiUrlEnvironmentVariable == "RENDER_STAGING_NEXARR_API_URL"
                && entry.LoadTestBaseUrlEnvironmentVariable == "STL_NEXARR_BASE_URL");
    }

    [Fact]
    public void Soak_catalog_covers_all_product_owner_scenarios()
    {
        Assert.Equal(7, StlRenderStagingLoadSoakCatalog.DefaultScenarioKeys.Count);
        Assert.Equal(
            StlLoadTestSloCatalog.ProductOwnerTargets.Count,
            StlRenderStagingLoadSoakCatalog.DefaultScenarioKeys.Count);
    }

    [Fact]
    public void Journey_seed_catalog_matches_load_test_defaults()
    {
        Assert.Equal(StlLoadTestJourneyDefaults.RulePackKey, StlLoadTestJourneySeedCatalog.RulePackKey);
        Assert.Equal("/api/load-test-journey/seed", StlLoadTestJourneySeedCatalog.SeedEndpointPath);
        Assert.Equal("driver_license_valid", StlLoadTestJourneySeedCatalog.DriverLicenseValidFactKey);
    }

    [Fact]
    public void Schedule_catalog_lists_seven_required_staging_api_url_env_vars()
    {
        Assert.Equal(7, StlRenderStagingLoadSoakScheduleCatalog.RequiredStagingApiUrlEnvironmentVariables.Count);
        Assert.Equal(
            StlRenderStagingLoadTestCatalog.All.Count,
            StlRenderStagingLoadSoakScheduleCatalog.RequiredStagingApiUrlEnvironmentVariables.Count);
        Assert.Equal("0 7 * * 0", StlRenderStagingLoadSoakScheduleCatalog.WeeklyCronUtc);
    }

    [Fact]
    public void AreStagingApiUrlsConfigured_returns_false_when_any_url_missing()
    {
        var previous = new Dictionary<string, string?>();
        try
        {
            foreach (var environmentVariable in StlRenderStagingLoadSoakScheduleCatalog.RequiredStagingApiUrlEnvironmentVariables)
            {
                previous[environmentVariable] = Environment.GetEnvironmentVariable(environmentVariable);
                Environment.SetEnvironmentVariable(environmentVariable, null);
            }

            Assert.False(StlRenderStagingLoadSoakScheduleCatalog.AreStagingApiUrlsConfigured());
            Assert.Contains(
                "RENDER_STAGING_NEXARR_API_URL",
                StlRenderStagingLoadSoakScheduleCatalog.GetMissingStagingApiUrlEnvironmentVariables());
        }
        finally
        {
            foreach (var (environmentVariable, value) in previous)
            {
                Environment.SetEnvironmentVariable(environmentVariable, value);
            }
        }
    }

    [Fact]
    public void AreStagingApiUrlsConfigured_returns_true_when_all_urls_present()
    {
        var previous = new Dictionary<string, string?>();
        try
        {
            foreach (var entry in StlRenderStagingLoadTestCatalog.All)
            {
                previous[entry.SourceApiUrlEnvironmentVariable] =
                    Environment.GetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable);
                Environment.SetEnvironmentVariable(
                    entry.SourceApiUrlEnvironmentVariable,
                    $"https://{entry.RenderApiServiceName}.onrender.com");
            }

            Assert.True(StlRenderStagingLoadSoakScheduleCatalog.AreStagingApiUrlsConfigured());
            Assert.Empty(StlRenderStagingLoadSoakScheduleCatalog.GetMissingStagingApiUrlEnvironmentVariables());
        }
        finally
        {
            foreach (var (environmentVariable, value) in previous)
            {
                Environment.SetEnvironmentVariable(environmentVariable, value);
            }
        }
    }

    [Fact]
    public void ResolveEndpointsFromEnvironment_maps_render_urls_to_k6_env()
    {
        var previous = new Dictionary<string, string?>();
        try
        {
            foreach (var entry in StlRenderStagingLoadTestCatalog.All)
            {
                previous[entry.SourceApiUrlEnvironmentVariable] =
                    Environment.GetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable);
                Environment.SetEnvironmentVariable(
                    entry.SourceApiUrlEnvironmentVariable,
                    $"https://{entry.RenderApiServiceName}.onrender.com/");
            }

            var targets = StlRenderStagingLoadTestSupport.ResolveEndpointsFromEnvironment();
            Assert.Equal(7, targets.Count);

            var nexarr = targets.Single(t => t.ProductKey == StlProductDatabaseCatalog.NexArr);
            Assert.Equal("https://nexarr-api.onrender.com", nexarr.BaseUrl);
            Assert.Equal("STL_NEXARR_BASE_URL", nexarr.LoadTestBaseUrlEnvironmentVariable);

            var map = StlRenderStagingLoadTestSupport.BuildK6BaseUrlEnvironment(targets);
            Assert.Equal("https://compliancecore-api.onrender.com", map["STL_COMPLIANCECORE_BASE_URL"]);
        }
        finally
        {
            foreach (var (envVar, value) in previous)
            {
                Environment.SetEnvironmentVariable(envVar, value);
            }
        }
    }

    [Fact]
    public void ResolveEndpointsFromEnvironment_throws_when_api_url_missing()
    {
        var previous = new Dictionary<string, string?>();
        try
        {
            foreach (var entry in StlRenderStagingLoadTestCatalog.All)
            {
                previous[entry.SourceApiUrlEnvironmentVariable] =
                    Environment.GetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable);
                Environment.SetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable, null);
            }

            var exception = Assert.Throws<InvalidOperationException>(
                () => StlRenderStagingLoadTestSupport.ResolveEndpointsFromEnvironment());

            Assert.Contains("RENDER_STAGING_NEXARR_API_URL", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            foreach (var (envVar, value) in previous)
            {
                Environment.SetEnvironmentVariable(envVar, value);
            }
        }
    }

    [Fact]
    public void ApplyK6Environment_sets_product_owner_profile()
    {
        var previousProfile = Environment.GetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar);
        var previousNexarr = Environment.GetEnvironmentVariable("STL_NEXARR_BASE_URL");
        try
        {
            var targets = new[]
            {
                StlRenderStagingLoadTestSupport.ParseApiUrl(
                    StlProductDatabaseCatalog.NexArr,
                    "https://nexarr-api.onrender.com"),
            };

            StlRenderStagingLoadTestSupport.ApplyK6Environment(targets);

            Assert.Equal("https://nexarr-api.onrender.com", Environment.GetEnvironmentVariable("STL_NEXARR_BASE_URL"));
            Assert.Equal(
                StlLoadTestSloCatalog.ProductOwnerProfile,
                Environment.GetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar));
        }
        finally
        {
            Environment.SetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar, previousProfile);
            Environment.SetEnvironmentVariable("STL_NEXARR_BASE_URL", previousNexarr);
        }
    }
}
