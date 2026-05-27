namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Live k6 probe settings for nightly M13 load validation against docker-compose.
/// Uses product-owner latency/error SLOs with reduced minimum request counts for short smoke runs.
/// </summary>
public sealed record StlLoadTestLiveScenarioDefinition(
    string ScenarioKey,
    int VirtualUsers,
    string Duration,
    int LiveMinRequestCount);

public static class StlLoadTestLiveScenarioCatalog
{
    public static readonly IReadOnlyList<StlLoadTestLiveScenarioDefinition> All =
    [
        new(StlLoadTestSloCatalog.ApiHealthLivenessKey, 2, "10s", 20),
        new(StlLoadTestSloCatalog.ApiHealthReadyKey, 2, "10s", 20),
        new(StlLoadTestSloCatalog.NexArrPlatformHealthKey, 2, "10s", 10),
        new(StlLoadTestSloCatalog.NexArrAuthMeKey, 2, "10s", 15),
        new(StlLoadTestSloCatalog.ProductAuthHandoffMeKey, 2, "15s", 6),
        new(StlLoadTestSloCatalog.TrainArrQualificationCheckKey, 2, "15s", 5),
        new(StlLoadTestSloCatalog.RoutArrDispatchWorkflowGateKey, 2, "15s", 4),
    ];

    public static StlLoadTestLiveScenarioDefinition GetByScenarioKey(string scenarioKey) =>
        All.FirstOrDefault(s => s.ScenarioKey.Equals(scenarioKey, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown live load-test scenario '{scenarioKey}'.");

    public static StlLoadTestSloTarget ResolveLiveSloTarget(string scenarioKey)
    {
        var definition = GetByScenarioKey(scenarioKey);
        var target = StlLoadTestSloCatalog.GetByScenarioKey(scenarioKey);
        return target with
        {
            MinRequestCount = Math.Min(target.MinRequestCount, definition.LiveMinRequestCount),
        };
    }
}
