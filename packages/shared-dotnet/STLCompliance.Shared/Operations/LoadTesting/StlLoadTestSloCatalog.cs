namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Engineering-default SLO targets for M13 load-test scenarios.
/// Replace values when product owners publish official SLO definitions.
/// </summary>
public static class StlLoadTestSloCatalog
{
    public const string ApiHealthLivenessKey = "api-health-liveness";
    public const string ApiHealthReadyKey = "api-health-ready";
    public const string NexArrPlatformHealthKey = "nexarr-platform-health";

    public static readonly IReadOnlyList<StlLoadTestSloTarget> EngineeringDefaults =
    [
        new(
            ApiHealthLivenessKey,
            "All product APIs /health liveness",
            P95LatencyMsMax: 500,
            ErrorRateMax: 0.01,
            MinRequestCount: 50,
            Notes: "Engineering placeholder — lightweight liveness probes against all seven APIs."),
        new(
            ApiHealthReadyKey,
            "All product APIs /health/ready readiness",
            P95LatencyMsMax: 2000,
            ErrorRateMax: 0.02,
            MinRequestCount: 50,
            Notes: "Engineering placeholder — readiness includes database checks."),
        new(
            NexArrPlatformHealthKey,
            "NexArr GET /api/platform/health aggregation",
            P95LatencyMsMax: 5000,
            ErrorRateMax: 0.05,
            MinRequestCount: 20,
            Notes: "Engineering placeholder — aggregates downstream /health/ready probes."),
    ];

    public static StlLoadTestSloTarget GetByScenarioKey(string scenarioKey) =>
        EngineeringDefaults.FirstOrDefault(s => s.ScenarioKey.Equals(scenarioKey, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown load-test scenario '{scenarioKey}'.");
}
