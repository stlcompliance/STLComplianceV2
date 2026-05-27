namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Service-level objective thresholds for a load-test scenario.
/// Engineering defaults until product owners publish official SLO targets.
/// </summary>
public sealed record StlLoadTestSloTarget(
    string ScenarioKey,
    string DisplayName,
    double P95LatencyMsMax,
    double ErrorRateMax,
    int MinRequestCount,
    string Notes);
