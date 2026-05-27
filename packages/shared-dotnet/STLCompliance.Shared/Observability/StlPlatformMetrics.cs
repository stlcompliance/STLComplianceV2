using System.Diagnostics.Metrics;

namespace STLCompliance.Shared.Observability;

public sealed class StlPlatformMetrics
{
    public const string MeterName = "STLCompliance.Platform";

    private readonly Counter<long> _healthRequests;

    public StlPlatformMetrics()
    {
        var meter = new Meter(MeterName);
        _healthRequests = meter.CreateCounter<long>(
            "stl.health.requests",
            unit: "{request}",
            description: "Count of platform health and readiness probe requests.");
    }

    public void RecordHealthRequest(string endpoint, string productKey) =>
        _healthRequests.Add(
            1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("product", productKey));
}
