using STLCompliance.Shared.Operations;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "Ci")]
[Trait("Area", "RenderStagingShipGate")]
public sealed class StlRenderStagingShipGateCatalogTests
{
    [Fact]
    public void Api_probes_cover_all_render_staging_load_test_products()
    {
        Assert.Equal(10, StlRenderStagingShipGateCatalog.ApiProbes.Count);
        Assert.Equal(
            StlRenderStagingLoadTestCatalog.All.Count,
            StlRenderStagingShipGateCatalog.ApiProbes.Count);

        foreach (var loadEntry in StlRenderStagingLoadTestCatalog.All)
        {
            var probe = StlRenderStagingShipGateCatalog.TryGetApiProbe(loadEntry.ProductKey);
            Assert.NotNull(probe);
            Assert.Equal(loadEntry.SourceApiUrlEnvironmentVariable, probe!.SourceApiUrlEnvironmentVariable);
            Assert.Equal(loadEntry.RenderApiServiceName, probe.RenderApiServiceName);
        }
    }

    [Fact]
    public void Api_probes_align_with_m13_openapi_product_keys()
    {
        var probeKeys = StlRenderStagingShipGateCatalog.ApiProbes
            .Select(entry => entry.ProductKey)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var productKey in StlM13ShipGateCatalog.OpenApiProductKeys)
        {
            Assert.Contains(productKey, probeKeys);
        }
    }

    [Fact]
    public void Required_staging_api_url_environment_variables_match_api_probes()
    {
        Assert.Equal(
            StlRenderStagingShipGateCatalog.ApiProbes.Count,
            StlRenderStagingShipGateCatalog.RequiredStagingApiUrlEnvironmentVariables.Count);

        foreach (var probe in StlRenderStagingShipGateCatalog.ApiProbes)
        {
            Assert.Contains(probe.SourceApiUrlEnvironmentVariable, StlRenderStagingShipGateCatalog.RequiredStagingApiUrlEnvironmentVariables);
        }
    }

    [Fact]
    public void Optional_static_site_probes_cover_blueprint_static_sites()
    {
        Assert.Equal(12, StlRenderStagingShipGateCatalog.OptionalStaticSiteProbes.Count);

        foreach (var site in StlRenderBlueprintCatalog.StaticSites)
        {
            Assert.NotNull(StlRenderStagingShipGateCatalog.TryGetStaticSiteProbe(site.Name));
        }
    }

    [Fact]
    public void Operator_runbook_and_validate_scripts_exist()
    {
        var repoRoot = FindRepoRoot();

        var runbookPath = Path.Combine(repoRoot, StlRenderStagingShipGateCatalog.OperatorRunbookDocRelativePath);
        Assert.True(File.Exists(runbookPath), $"Missing runbook at {runbookPath}.");

        var ps1Path = Path.Combine(repoRoot, StlRenderStagingShipGateCatalog.ValidateScriptPs1RelativePath);
        Assert.True(File.Exists(ps1Path), $"Missing script at {ps1Path}.");

        var shPath = Path.Combine(repoRoot, StlRenderStagingShipGateCatalog.ValidateScriptShRelativePath);
        Assert.True(File.Exists(shPath), $"Missing script at {shPath}.");
    }

    [Fact]
    public void Main_ci_workflow_runs_render_staging_ship_gate_catalog_checks()
    {
        var repoRoot = FindRepoRoot();
        var workflowPath = Path.Combine(repoRoot, ".github/workflows/ci.yml");
        Assert.True(File.Exists(workflowPath), $"Missing CI workflow at {workflowPath}.");

        var workflow = File.ReadAllText(workflowPath);
        Assert.Contains("Render staging ship gate catalog checks", workflow, StringComparison.Ordinal);
        Assert.Contains(StlRenderStagingShipGateCatalog.LocalCatalogCiFilter, workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void Ship_gate_staging_render_workflow_exists()
    {
        var repoRoot = FindRepoRoot();
        var workflowPath = Path.Combine(repoRoot, ".github/workflows", StlRenderStagingShipGateCatalog.GitHubWorkflowFileName);
        Assert.True(File.Exists(workflowPath), $"Missing workflow at {workflowPath}.");

        var workflow = File.ReadAllText(workflowPath);
        Assert.Contains(StlRenderStagingShipGateCatalog.GitHubWorkflowDisplayName, workflow, StringComparison.Ordinal);
        Assert.Contains("RENDER_STAGING_NEXARR_API_URL", workflow, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "STLCompliance.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test output directory.");
    }
}
