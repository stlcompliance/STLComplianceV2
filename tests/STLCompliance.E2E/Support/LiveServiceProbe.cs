using System.Net;
using System.Net.Http.Json;
using STLCompliance.Shared.Health;

namespace STLCompliance.E2E.Support;

/// <summary>
/// Default local docker-compose API base URLs (host-mapped ports).
/// Override with E2E_*_URL environment variables.
/// </summary>
internal sealed record LiveServiceEndpoints(
    Uri NexArr,
    Uri StaffArr,
    Uri TrainArr,
    Uri MaintainArr,
    Uri RoutArr,
    Uri SupplyArr,
    Uri ComplianceCore)
{
    public static LiveServiceEndpoints FromEnvironment()
    {
        static Uri Resolve(string envKey, string defaultUrl) =>
            new(Environment.GetEnvironmentVariable(envKey) ?? defaultUrl, UriKind.Absolute);

        return new LiveServiceEndpoints(
            Resolve("E2E_NEXARR_URL", "http://localhost:5101"),
            Resolve("E2E_STAFFARR_URL", "http://localhost:5102"),
            Resolve("E2E_TRAINARR_URL", "http://localhost:5103"),
            Resolve("E2E_MAINTAINARR_URL", "http://localhost:5104"),
            Resolve("E2E_ROUTARR_URL", "http://localhost:5105"),
            Resolve("E2E_SUPPLYARR_URL", "http://localhost:5106"),
            Resolve("E2E_COMPLIANCECORE_URL", "http://localhost:5107"));
    }

    public IEnumerable<(string Product, Uri BaseUrl)> AllProducts =>
    [
        ("nexarr", NexArr),
        ("staffarr", StaffArr),
        ("trainarr", TrainArr),
        ("maintainarr", MaintainArr),
        ("routarr", RoutArr),
        ("supplyarr", SupplyArr),
        ("compliancecore", ComplianceCore),
    ];
}

internal static class LiveServiceProbe
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);

    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable("E2E_LIVE"), "true", StringComparison.OrdinalIgnoreCase);

    public static async Task<bool> IsReachableAsync(Uri baseUrl, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(ProbeTimeout);

        try
        {
            var response = await client.GetAsync(new Uri(baseUrl, "/health"), cts.Token);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<HealthResponse>(cancellationToken: cts.Token);
            return payload is { Status: "Healthy" };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public static async Task<bool> AreAllProductApisAvailableAsync(LiveServiceEndpoints endpoints)
    {
        foreach (var (_, baseUrl) in endpoints.AllProducts)
        {
            if (!await IsReachableAsync(baseUrl))
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<bool> IsNexArrAvailableAsync(LiveServiceEndpoints endpoints) =>
        await IsReachableAsync(endpoints.NexArr);

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = ProbeTimeout };
        return client;
    }
}
