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
