namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Evaluates k6 summary exports against engineering-default or custom SLO targets.
/// </summary>
public static class StlLoadTestSloEvaluator
{
    public static StlLoadTestSloEvaluationResult Evaluate(
        string scenarioKey,
        StlLoadTestK6Summary summary,
        StlLoadTestSloTarget? target = null)
    {
        target ??= StlLoadTestSloCatalog.GetByScenarioKey(scenarioKey);

        var violations = new List<string>();
        var p95 = summary.GetP95LatencyMs();
        var errorRate = summary.GetErrorRate();
        var requestCount = summary.GetRequestCount();

        if (!p95.HasValue)
        {
            violations.Add("Missing http_req_duration p(95) in k6 summary.");
        }
        else if (p95.Value > target.P95LatencyMsMax)
        {
            violations.Add(
                $"p95 latency {p95.Value:F1}ms exceeds max {target.P95LatencyMsMax:F1}ms.");
        }

        if (!errorRate.HasValue)
        {
            violations.Add("Missing http_req_failed rate in k6 summary.");
        }
        else if (errorRate.Value > target.ErrorRateMax)
        {
            violations.Add(
                $"error rate {errorRate.Value:P2} exceeds max {target.ErrorRateMax:P2}.");
        }

        if (!requestCount.HasValue)
        {
            violations.Add("Missing http_reqs count in k6 summary.");
        }
        else if (requestCount.Value < target.MinRequestCount)
        {
            violations.Add(
                $"request count {requestCount.Value} below minimum {target.MinRequestCount}.");
        }

        return new StlLoadTestSloEvaluationResult(
            target.ScenarioKey,
            violations.Count == 0,
            violations,
            p95,
            errorRate,
            requestCount);
    }

    public static StlLoadTestSloEvaluationResult EvaluateFile(
        string scenarioKey,
        string summaryExportPath,
        StlLoadTestSloTarget? target = null)
    {
        var summary = StlLoadTestK6Summary.ParseFromFile(summaryExportPath);
        return Evaluate(scenarioKey, summary, target);
    }
}
