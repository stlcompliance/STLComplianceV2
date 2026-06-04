using System.Text.Json.Nodes;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "Ci")]
[Trait("Area", "Frontend")]
public sealed class StlCiFrontendCatalogTests
{
    [Fact]
    public void Main_ci_frontend_jobs_include_suite_routarr_staffarr_trainarr_maintainarr_supplyarr_compliancecore_loadarr_and_assurarr()
    {
        var jobIds = StlCiFrontendCatalog.MainCiFrontendJobs
            .Select(job => job.JobId)
            .ToArray();

        Assert.Contains("stlcompliancesite", jobIds);
        Assert.Contains("suite-frontend", jobIds);
        Assert.Contains("routarr-frontend", jobIds);
        Assert.Contains("staffarr-frontend", jobIds);
        Assert.Contains("trainarr-frontend", jobIds);
        Assert.Contains("maintainarr-frontend", jobIds);
        Assert.Contains("supplyarr-frontend", jobIds);
        Assert.Contains("compliancecore-frontend", jobIds);
        Assert.Contains("loadarr-frontend", jobIds);
        Assert.Contains("assurarr-frontend", jobIds);
    }

    [Fact]
    public void Gated_product_frontend_jobs_start_with_routarr_staffarr_trainarr_maintainarr_supplyarr_compliancecore_loadarr_then_assurarr()
    {
        Assert.Equal(8, StlCiFrontendCatalog.GatedProductFrontendJobs.Count);
        Assert.Equal(
            StlCiFrontendCatalog.RoutArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[0]);
        Assert.Equal(
            StlCiFrontendCatalog.StaffArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[1]);
        Assert.Equal(
            StlCiFrontendCatalog.TrainArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[2]);
        Assert.Equal(
            StlCiFrontendCatalog.MaintainArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[3]);
        Assert.Equal(
            StlCiFrontendCatalog.SupplyArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[4]);
        Assert.Equal(
            StlCiFrontendCatalog.ComplianceCoreFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[5]);
        Assert.Equal(
            StlCiFrontendCatalog.LoadArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[6]);
        Assert.Equal(
            StlCiFrontendCatalog.AssurArrFrontend,
            StlCiFrontendCatalog.GatedProductFrontendJobs[7]);
    }

    [Fact]
    public void Every_main_ci_frontend_job_has_package_lock_and_build_test_scripts()
    {
        var repoRoot = FindRepoRoot();

        foreach (var job in StlCiFrontendCatalog.MainCiFrontendJobs)
        {
            var packageLockPath = Path.Combine(repoRoot, job.PackageLockRelativePath);
            Assert.True(File.Exists(packageLockPath), $"Missing package-lock for '{job.JobId}' at {packageLockPath}.");

            var packageJsonPath = Path.Combine(repoRoot, job.AppDirectory, "package.json");
            Assert.True(File.Exists(packageJsonPath), $"Missing package.json for '{job.JobId}' at {packageJsonPath}.");

            var packageJson = JsonNode.Parse(File.ReadAllText(packageJsonPath))!.AsObject();
            var scripts = packageJson["scripts"]?.AsObject()
                ?? throw new InvalidOperationException($"Missing scripts in {packageJsonPath}.");

            if (job.RunsBuild)
            {
                Assert.True(scripts.ContainsKey("build"), $"'{job.JobId}' must declare an npm build script.");
            }

            if (job.RunsTest)
            {
                Assert.True(scripts.ContainsKey("test"), $"'{job.JobId}' must declare an npm test script.");
            }
        }
    }

    [Fact]
    public void Main_ci_workflow_declares_every_catalog_frontend_job()
    {
        var repoRoot = FindRepoRoot();
        var workflowPath = Path.Combine(repoRoot, StlCiFrontendCatalog.MainCiWorkflowRelativePath);
        Assert.True(File.Exists(workflowPath), $"Missing main CI workflow at {workflowPath}.");

        var workflow = File.ReadAllText(workflowPath);
        foreach (var job in StlCiFrontendCatalog.MainCiFrontendJobs)
        {
            Assert.Contains($"{job.JobId}:", workflow, StringComparison.Ordinal);
            Assert.Contains($"working-directory: {job.AppDirectory}", workflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Gated_product_frontend_jobs_are_subset_of_main_ci_jobs()
    {
        var mainJobIds = StlCiFrontendCatalog.MainCiFrontendJobs
            .Select(job => job.JobId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var job in StlCiFrontendCatalog.GatedProductFrontendJobs)
        {
            Assert.True(job.IsProductFrontendGate);
            Assert.Contains(job.JobId, mainJobIds);
        }
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
