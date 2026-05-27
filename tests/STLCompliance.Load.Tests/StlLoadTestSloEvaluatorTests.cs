using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.Load.Tests;

[Trait("Category", "Load")]
public sealed class StlLoadTestSloCatalogTests
{
    [Fact]
    public void EngineeringDefaults_includes_seven_scenarios()
    {
        Assert.Equal(7, StlLoadTestSloCatalog.EngineeringDefaults.Count);
        Assert.Contains(
            StlLoadTestSloCatalog.EngineeringDefaults,
            s => s.ScenarioKey == StlLoadTestSloCatalog.ApiHealthLivenessKey);
    }

    [Fact]
    public void ProductOwnerTargets_includes_seven_scenarios()
    {
        Assert.Equal(7, StlLoadTestSloCatalog.ProductOwnerTargets.Count);
        Assert.Contains(
            StlLoadTestSloCatalog.ProductOwnerTargets,
            s => s.ScenarioKey == StlLoadTestSloCatalog.TrainArrQualificationCheckKey);
        Assert.Contains(
            StlLoadTestSloCatalog.ProductOwnerTargets,
            s => s.ScenarioKey == StlLoadTestSloCatalog.RoutArrDispatchWorkflowGateKey);
    }

    [Fact]
    public void GetByScenarioKey_returns_product_owner_ready_target_by_default()
    {
        var previous = Environment.GetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar, null);
            var target = StlLoadTestSloCatalog.GetByScenarioKey(StlLoadTestSloCatalog.ApiHealthReadyKey);
            Assert.Equal(1500, target.P95LatencyMsMax);
        }
        finally
        {
            Environment.SetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar, previous);
        }
    }

    [Fact]
    public void GetByScenarioKey_returns_engineering_target_when_profile_set()
    {
        var previous = Environment.GetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar);
        try
        {
            Environment.SetEnvironmentVariable(
                StlLoadTestSloCatalog.ActiveProfileEnvVar,
                StlLoadTestSloCatalog.EngineeringDefaultsProfile);
            var target = StlLoadTestSloCatalog.GetByScenarioKey(StlLoadTestSloCatalog.ApiHealthReadyKey);
            Assert.Equal(2000, target.P95LatencyMsMax);
        }
        finally
        {
            Environment.SetEnvironmentVariable(StlLoadTestSloCatalog.ActiveProfileEnvVar, previous);
        }
    }

    [Fact]
    public void GetByScenarioKey_returns_journey_scenario()
    {
        var target = StlLoadTestSloCatalog.GetByScenarioKey(
            StlLoadTestSloCatalog.TrainArrQualificationCheckKey);
        Assert.Equal(10000, target.P95LatencyMsMax);
    }

    [Fact]
    public void GetByScenarioKey_throws_for_unknown_scenario()
    {
        Assert.Throws<KeyNotFoundException>(() => StlLoadTestSloCatalog.GetByScenarioKey("missing"));
    }
}

[Trait("Category", "Load")]
public sealed class StlLoadTestJourneyDefaultsTests
{
    [Fact]
    public void Journey_defaults_match_demo_platform_seeder()
    {
        Assert.Equal("22222222-2222-2222-2222-222222222201", StlLoadTestJourneyDefaults.SubjectPersonId);
        Assert.Equal("hazmat_endorsement", StlLoadTestJourneyDefaults.QualificationKey);
        Assert.Equal("driver_qualification", StlLoadTestJourneyDefaults.RulePackKey);
    }
}

[Trait("Category", "Load")]
public sealed class StlLoadTestAuthDefaultsTests
{
    [Fact]
    public void Demo_credentials_match_platform_seeder()
    {
        Assert.Equal("admin@demo.stl", StlLoadTestAuthDefaults.DemoEmail);
        Assert.Equal("ChangeMe!Demo2026", StlLoadTestAuthDefaults.DemoPassword);
        Assert.Equal("11111111-1111-1111-1111-111111111101", StlLoadTestAuthDefaults.DemoTenantId);
    }
}

[Trait("Category", "Load")]
public sealed class StlLoadTestApiEndpointsTests
{
    [Fact]
    public void All_includes_seven_product_apis()
    {
        Assert.Equal(7, StlLoadTestApiEndpoints.All.Count);
    }
}

[Trait("Category", "Load")]
public sealed class StlLoadTestSloEvaluatorTests
{
    [Fact]
    public void Evaluate_passes_when_summary_meets_slo()
    {
        var summary = StlLoadTestK6Summary.Parse(
            """
            {
              "metrics": {
                "http_req_duration": { "values": { "p(95)": 120.5 } },
                "http_req_failed": { "values": { "rate": 0.0 } },
                "http_reqs": { "values": { "count": 100 } }
              }
            }
            """);

        var result = StlLoadTestSloEvaluator.Evaluate(
            StlLoadTestSloCatalog.ApiHealthLivenessKey,
            summary);

        Assert.True(result.Passed);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Evaluate_fails_when_p95_exceeds_slo()
    {
        var summary = StlLoadTestK6Summary.Parse(
            """
            {
              "metrics": {
                "http_req_duration": { "values": { "p(95)": 900.0 } },
                "http_req_failed": { "values": { "rate": 0.0 } },
                "http_reqs": { "values": { "count": 100 } }
              }
            }
            """);

        var result = StlLoadTestSloEvaluator.Evaluate(
            StlLoadTestSloCatalog.ApiHealthLivenessKey,
            summary);

        Assert.False(result.Passed);
        Assert.Contains(result.Violations, v => v.Contains("p95 latency", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_fails_when_error_rate_exceeds_slo()
    {
        var summary = StlLoadTestK6Summary.Parse(
            """
            {
              "metrics": {
                "http_req_duration": { "values": { "p(95)": 100.0 } },
                "http_req_failed": { "values": { "rate": 0.05 } },
                "http_reqs": { "values": { "count": 100 } }
              }
            }
            """);

        var result = StlLoadTestSloEvaluator.Evaluate(
            StlLoadTestSloCatalog.ApiHealthLivenessKey,
            summary);

        Assert.False(result.Passed);
        Assert.Contains(result.Violations, v => v.Contains("error rate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_fails_when_request_count_below_minimum()
    {
        var summary = StlLoadTestK6Summary.Parse(
            """
            {
              "metrics": {
                "http_req_duration": { "values": { "p(95)": 100.0 } },
                "http_req_failed": { "values": { "rate": 0.0 } },
                "http_reqs": { "values": { "count": 10 } }
              }
            }
            """);

        var result = StlLoadTestSloEvaluator.Evaluate(
            StlLoadTestSloCatalog.ApiHealthLivenessKey,
            summary);

        Assert.False(result.Passed);
        Assert.Contains(result.Violations, v => v.Contains("request count", StringComparison.OrdinalIgnoreCase));
    }
}

[Trait("Category", "Load")]
public sealed class LoadTestHarnessSupportTests
{
    [SkippableFact]
    public void Evaluate_summary_file_from_environment()
    {
        var scenarioKey = Environment.GetEnvironmentVariable("LOAD_SCENARIO_KEY");
        var summaryPath = Environment.GetEnvironmentVariable("LOAD_SUMMARY_PATH");

        Skip.If(
            string.IsNullOrWhiteSpace(scenarioKey) || string.IsNullOrWhiteSpace(summaryPath),
            "Set LOAD_SCENARIO_KEY and LOAD_SUMMARY_PATH to validate a k6 summary export.");

        Skip.IfNot(File.Exists(summaryPath!), $"Summary file not found: {summaryPath}");

        var result = StlLoadTestSloEvaluator.EvaluateFile(scenarioKey!, summaryPath!);
        Assert.True(result.Passed, string.Join("; ", result.Violations));
    }
}
