using System.Text.Json;
using System.Text.Json.Serialization;

namespace STLCompliance.Shared.Operations.LoadTesting;

/// <summary>
/// Parsed k6 end-of-test summary export (JSON from --summary-export).
/// </summary>
public sealed class StlLoadTestK6Summary
{
    [JsonPropertyName("metrics")]
    public Dictionary<string, StlLoadTestK6Metric>? Metrics { get; init; }

    public static StlLoadTestK6Summary Parse(string json) =>
        JsonSerializer.Deserialize<StlLoadTestK6Summary>(json, JsonOptions)
        ?? throw new InvalidOperationException("k6 summary JSON deserialized to null.");

    public static StlLoadTestK6Summary ParseFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return Parse(json);
    }

    public double? GetP95LatencyMs() =>
        TryGetMetricValue("http_req_duration", "p(95)");

    public double? GetErrorRate() =>
        TryGetMetricValue("http_req_failed", "rate");

    public int? GetRequestCount()
    {
        var count = TryGetMetricValue("http_reqs", "count");
        return count.HasValue ? (int)Math.Round(count.Value) : null;
    }

    private double? TryGetMetricValue(string metricName, string valueKey)
    {
        if (Metrics is null || !Metrics.TryGetValue(metricName, out var metric))
        {
            return null;
        }

        if (metric.Values is null || !metric.Values.TryGetValue(valueKey, out var element))
        {
            return null;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Number => element.GetDouble(),
            _ => null,
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

public sealed class StlLoadTestK6Metric
{
    [JsonPropertyName("values")]
    public Dictionary<string, JsonElement>? Values { get; init; }
}
