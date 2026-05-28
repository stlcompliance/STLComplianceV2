using System.Diagnostics;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.Load.Tests;

/// <summary>
/// Optional live k6 load tests against docker-compose APIs.
/// Skipped unless LOAD_LIVE=1 (or E2E_LIVE=1) and k6 is on PATH.
/// </summary>
[Trait("Category", "Load")]
[Trait("Category", "Live")]
public sealed class LoadTestLiveTests
{
    public static IEnumerable<object[]> ProductOwnerLiveScenarios =>
        StlLoadTestLiveScenarioCatalog.All.Select(definition => new object[] { definition.ScenarioKey });

    [SkippableTheory]
    [MemberData(nameof(ProductOwnerLiveScenarios))]
    public async Task K6_scenario_meets_product_owner_live_slo(string scenarioKey)
    {
        Skip.IfNot(LoadLiveProbe.LiveModeEnabled, "Set LOAD_LIVE=1 or E2E_LIVE=1 to run live k6 load tests.");
        Skip.IfNot(LoadLiveProbe.IsK6Available(), "k6 is not available on PATH.");
        Skip.IfNot(await LoadLiveProbe.AreApisHealthyAsync(), "One or more product APIs are unavailable.");

        var definition = StlLoadTestLiveScenarioCatalog.GetByScenarioKey(scenarioKey);
        var repoRoot = LoadLiveProbe.ResolveRepoRoot();
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"stl-load-live-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);

        var summaryPath = Path.Combine(outputDirectory, $"{scenarioKey}-summary.json");
        var scriptPath = Path.Combine(repoRoot, $"tests/load-k6/scenarios/{scenarioKey}.js");

        try
        {
            Assert.True(File.Exists(scriptPath), $"Missing k6 script: {scriptPath}");

            await LoadLiveProbe.RunK6ScenarioAsync(
                scriptPath,
                summaryPath,
                definition.VirtualUsers,
                definition.Duration);

            var result = StlLoadTestSloEvaluator.EvaluateFile(
                scenarioKey,
                summaryPath,
                StlLoadTestLiveScenarioCatalog.ResolveLiveSloTarget(scenarioKey));

            Assert.True(result.Passed, string.Join("; ", result.Violations));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }
}

internal static class LoadLiveProbe
{
    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("LOAD_LIVE"), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable("LOAD_LIVE"), "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "true", StringComparison.OrdinalIgnoreCase);

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

    public static async Task<bool> AreApisHealthyAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        foreach (var (_, baseUrl) in StlLoadTestApiEndpoints.All)
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/health");
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        return true;
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

    public static async Task RunK6ScenarioAsync(string scriptPath, string summaryPath, int vus, string duration)
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
        startInfo.Environment[StlLoadTestAuthDefaults.EmailEnvVar] = StlLoadTestAuthDefaults.DemoEmail;
        startInfo.Environment[StlLoadTestAuthDefaults.PasswordEnvVar] = StlLoadTestAuthDefaults.DemoPassword;
        startInfo.Environment[StlLoadTestAuthDefaults.TenantIdEnvVar] = StlLoadTestAuthDefaults.DemoTenantId;
        startInfo.Environment[StlLoadTestJourneyDefaults.SubjectPersonIdEnvVar] = StlLoadTestJourneyDefaults.SubjectPersonId;
        startInfo.Environment[StlLoadTestJourneyDefaults.QualificationKeyEnvVar] = StlLoadTestJourneyDefaults.QualificationKey;
        startInfo.Environment[StlLoadTestJourneyDefaults.RulePackKeyEnvVar] = StlLoadTestJourneyDefaults.RulePackKey;
        startInfo.Environment[StlLoadTestJourneyDefaults.DriverLicenseFactKeyEnvVar] =
            StlLoadTestJourneyDefaults.DriverLicenseFactKey;
        startInfo.Environment[StlLoadTestSloCatalog.ActiveProfileEnvVar] = StlLoadTestSloCatalog.ProductOwnerProfile;

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
