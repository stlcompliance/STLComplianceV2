using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Http;

namespace NexArr.Api.Services;

public sealed class PlatformHealthService(
    IHttpClientFactory httpClientFactory,
    IOptions<PlatformProductUrlsOptions> options)
{
    public const string HttpClientName = "PlatformHealthProbe";

    private static readonly (string ProductKey, Func<PlatformProductUrlsOptions, string> ResolveUrl)[] Products =
    [
        ("staffarr", o => o.StaffArrBaseUrl),
        ("trainarr", o => o.TrainArrBaseUrl),
        ("maintainarr", o => o.MaintainArrBaseUrl),
        ("routarr", o => o.RoutArrBaseUrl),
        ("supplyarr", o => o.SupplyArrBaseUrl),
        ("compliancecore", o => o.ComplianceCoreBaseUrl),
        ("recordarr", o => o.RecordArrBaseUrl),
    ];

    public async Task<PlatformHealthResponse> GetAggregateHealthAsync(CancellationToken cancellationToken = default)
    {
        var urlOptions = options.Value;
        var probes = await Task.WhenAll(
            Products.Select(product => ProbeProductAsync(product.ProductKey, product.ResolveUrl(urlOptions), cancellationToken)));

        var configured = probes.Where(p => p.Status != "NotConfigured").ToList();
        var status = configured.Count == 0
            ? "Degraded"
            : configured.All(p => string.Equals(p.Status, "Healthy", StringComparison.OrdinalIgnoreCase))
                ? "Healthy"
                : configured.Any(p => string.Equals(p.Status, "Healthy", StringComparison.OrdinalIgnoreCase))
                    ? "Degraded"
                    : "Unhealthy";

        return new PlatformHealthResponse(status, DateTimeOffset.UtcNow, probes);
    }

    private async Task<ProductHealthProbeResult> ProbeProductAsync(
        string productKey,
        string? rawBaseUrl,
        CancellationToken cancellationToken)
    {
        var baseUrl = StlServiceUrl.NormalizeHttpBaseUrl(rawBaseUrl);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new ProductHealthProbeResult(
                productKey,
                "NotConfigured",
                null,
                null,
                "not_configured",
                $"{productKey} API URL is not configured.",
                null);
        }

        var readyUrl = $"{baseUrl.TrimEnd('/')}/health/ready";
        var client = httpClientFactory.CreateClient(HttpClientName);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await client.GetAsync(readyUrl, cancellationToken);
            stopwatch.Stop();
            var latencyMs = stopwatch.Elapsed.TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ProductHealthProbeResult(
                    productKey,
                    "Unhealthy",
                    readyUrl,
                    latencyMs,
                    $"upstream_{(int)response.StatusCode}",
                    string.IsNullOrWhiteSpace(body) ? "Ready probe returned a non-success status." : body,
                    null);
            }

            var detail = await response.Content.ReadFromJsonAsync<HealthResponse>(cancellationToken);
            var downstreamStatus = detail?.Status ?? "Unknown";
            var mappedStatus = MapDownstreamStatus(downstreamStatus);

            return new ProductHealthProbeResult(
                productKey,
                mappedStatus,
                readyUrl,
                latencyMs,
                mappedStatus == "Healthy" ? null : "downstream_not_healthy",
                mappedStatus == "Healthy" ? null : $"Downstream reported status '{downstreamStatus}'.",
                detail);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return Unreachable(productKey, readyUrl, stopwatch.Elapsed.TotalMilliseconds, "probe_timeout", "Ready probe timed out.");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return Unreachable(productKey, readyUrl, stopwatch.Elapsed.TotalMilliseconds, "upstream_unreachable", ex.Message);
        }
    }

    private static string MapDownstreamStatus(string downstreamStatus) =>
        downstreamStatus.Equals("Healthy", StringComparison.OrdinalIgnoreCase)
            ? "Healthy"
            : downstreamStatus.Equals("Degraded", StringComparison.OrdinalIgnoreCase)
                ? "Degraded"
                : "Unhealthy";

    private static ProductHealthProbeResult Unreachable(
        string productKey,
        string readyUrl,
        double latencyMs,
        string errorCode,
        string errorMessage) =>
        new(productKey, "Unreachable", readyUrl, latencyMs, errorCode, errorMessage, null);
}
