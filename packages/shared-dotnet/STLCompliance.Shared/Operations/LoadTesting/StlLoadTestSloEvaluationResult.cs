namespace STLCompliance.Shared.Operations.LoadTesting;

public sealed record StlLoadTestSloEvaluationResult(
    string ScenarioKey,
    bool Passed,
    IReadOnlyList<string> Violations,
    double? ObservedP95LatencyMs,
    double? ObservedErrorRate,
    int? ObservedRequestCount);
