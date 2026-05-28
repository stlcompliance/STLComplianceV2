using System.Diagnostics;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.Load.Tests;

/// <summary>
/// Optional live k6 load soak against Render staging APIs with full product-owner SLO thresholds.
/// Skipped unless LOAD_RENDER_STAGING_LIVE=1 and all RENDER_STAGING_*_API_URL values are set.
/// </summary>
[Trait("Category", "Load")]
[Trait("Category", "Live")]
public sealed class RenderStagingLoadSoakLiveTests
{
    public static IEnumerable<object[]> StagingSoakScenarios =>
        StlRenderStagingLoadSoakCatalog.DefaultScenarioKeys.Select(key => new object[] { key });

    [SkippableTheory]
    [MemberData(nameof(StagingSoakScenarios))]
    public async Task K6_scenario_meets_product_owner_staging_soak_slo(string scenarioKey)
    {
        Skip.IfNot(RenderStagingLoadLiveProbe.LiveModeEnabled, "Set LOAD_RENDER_STAGING_LIVE=1 to run Render staging load soak.");
        Skip.IfNot(RenderStagingLoadLiveProbe.IsK6Available(), "k6 is not available on PATH.");

        var endpoints = RenderStagingLoadLiveProbe.ResolveEndpoints();
        Skip.IfNot(await StlRenderStagingLoadTestSupport.AreEndpointsHealthyAsync(endpoints), "One or more Render staging APIs are unavailable.");

        var repoRoot = RenderStagingLoadLiveProbe.ResolveRepoRoot();
        var outputDirectory = Environment.GetEnvironmentVariable(StlRenderStagingLoadTestSupport.OutputDirectoryEnvironmentVariable)
            ?? Path.Combine(Path.GetTempPath(), $"stl-render-staging-load-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);

        var summaryPath = Path.Combine(outputDirectory, $"{scenarioKey}-summary.json");
        var scriptPath = Path.Combine(repoRoot, $"tests/load-k6/scenarios/{scenarioKey}.js");

        try
        {
            Assert.True(File.Exists(scriptPath), $"Missing k6 script: {scriptPath}");

            await RenderStagingLoadLiveProbe.RunK6ScenarioAsync(
                scriptPath,
                summaryPath,
                endpoints,
                StlRenderStagingLoadSoakCatalog.DefaultVirtualUsers,
                StlRenderStagingLoadSoakCatalog.DefaultDuration);

            var result = StlLoadTestSloEvaluator.EvaluateFile(
                scenarioKey,
                summaryPath,
                StlRenderStagingLoadSoakCatalog.ResolveSoakSloTarget(scenarioKey));

            Assert.True(result.Passed, string.Join("; ", result.Violations));
        }
        finally
        {
            if (Directory.Exists(outputDirectory)
                && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(StlRenderStagingLoadTestSupport.OutputDirectoryEnvironmentVariable)))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}

internal static class RenderStagingLoadLiveProbe
{
    public static bool LiveModeEnabled => StlRenderStagingLoadTestSupport.LiveModeEnabled;

    public static IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> ResolveEndpoints() =>
        StlRenderStagingLoadTestSupport.ResolveEndpointsFromEnvironment();

    public static bool IsK6Available()
    {
        try
        {
            return RunProcess("k6", "version").ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static string ResolveRepoRoot()
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

        throw new InvalidOperationException("Could not locate repository root from test base directory.");
    }

    public static async Task RunK6ScenarioAsync(
        string scriptPath,
        string summaryPath,
        IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> endpoints,
        int vus,
        string duration)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "k6",
            Arguments = $"run \"{scriptPath}\" --summary-export \"{summaryPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.Environment["STL_LOAD_VUS"] = vus.ToString();
        startInfo.Environment["STL_LOAD_DURATION"] = duration;
        startInfo.Environment[StlLoadTestAuthDefaults.EmailEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.EmailEnvVar) ?? StlLoadTestAuthDefaults.DemoEmail;
        startInfo.Environment[StlLoadTestAuthDefaults.PasswordEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.PasswordEnvVar) ?? StlLoadTestAuthDefaults.DemoPassword;
        startInfo.Environment[StlLoadTestAuthDefaults.TenantIdEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.TenantIdEnvVar) ?? StlLoadTestAuthDefaults.DemoTenantId;
        startInfo.Environment[StlLoadTestJourneyDefaults.SubjectPersonIdEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestJourneyDefaults.SubjectPersonIdEnvVar)
            ?? StlLoadTestJourneyDefaults.SubjectPersonId;
        startInfo.Environment[StlLoadTestJourneyDefaults.QualificationKeyEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestJourneyDefaults.QualificationKeyEnvVar)
            ?? StlLoadTestJourneyDefaults.QualificationKey;
        startInfo.Environment[StlLoadTestJourneyDefaults.RulePackKeyEnvVar] =
            Environment.GetEnvironmentVariable(StlLoadTestJourneyDefaults.RulePackKeyEnvVar)
            ?? StlLoadTestJourneyDefaults.RulePackKey;
        startInfo.Environment[StlLoadTestSloCatalog.ActiveProfileEnvVar] = StlLoadTestSloCatalog.ProductOwnerProfile;

        foreach (var (envVar, value) in StlRenderStagingLoadTestSupport.BuildK6BaseUrlEnvironment(endpoints))
        {
            startInfo.Environment[envVar] = value;
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start k6.");

        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"k6 failed: {error}");
        }
    }

    private static Process RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {fileName}.");

        process.WaitForExit();
        return process;
    }
}
