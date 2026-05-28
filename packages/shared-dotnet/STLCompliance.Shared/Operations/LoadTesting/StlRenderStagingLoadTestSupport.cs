using STLCompliance.Shared.Http;
using STLCompliance.Shared.Operations;

namespace STLCompliance.Shared.Operations.LoadTesting;

public static class StlRenderStagingLoadTestSupport
{
    public const string LiveModeEnvironmentVariable = "LOAD_RENDER_STAGING_LIVE";
    public const string OutputDirectoryEnvironmentVariable = "RENDER_STAGING_LOAD_OUTPUT_DIRECTORY";

    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable(LiveModeEnvironmentVariable), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable(LiveModeEnvironmentVariable), "true", StringComparison.OrdinalIgnoreCase);

    public static StlRenderStagingLoadTestEndpointTarget ParseApiUrl(
        string productKey,
        string apiUrl)
    {
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            throw new ArgumentException("API URL is required.", nameof(apiUrl));
        }

        var entry = StlRenderStagingLoadTestCatalog.TryGetEntry(productKey)
            ?? throw new ArgumentException($"Unknown product key '{productKey}'.", nameof(productKey));

        var normalized = StlServiceUrl.NormalizeHttpBaseUrl(apiUrl);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException($"API URL for '{productKey}' is empty after normalization.");
        }

        if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"API URL for '{productKey}' must be an http(s) URL.");
        }

        return new StlRenderStagingLoadTestEndpointTarget(
            entry.ProductKey,
            normalized,
            entry.LoadTestBaseUrlEnvironmentVariable);
    }

    public static IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> ResolveEndpointsFromEnvironment(
        IReadOnlyList<string>? productKeys = null)
    {
        var selected = productKeys is { Count: > 0 }
            ? productKeys
            : StlProductDatabaseCatalog.All;

        var targets = new List<StlRenderStagingLoadTestEndpointTarget>();
        var missing = new List<string>();

        foreach (var productKey in selected)
        {
            var entry = StlRenderStagingLoadTestCatalog.TryGetEntry(productKey)
                ?? throw new InvalidOperationException($"No Render staging load-test catalog entry for '{productKey}'.");

            var apiUrl = Environment.GetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                missing.Add(entry.SourceApiUrlEnvironmentVariable);
                continue;
            }

            targets.Add(ParseApiUrl(entry.ProductKey, apiUrl));
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing staging API URL environment variables: {string.Join(", ", missing)}");
        }

        return targets;
    }

    public static IReadOnlyDictionary<string, string> BuildK6BaseUrlEnvironment(
        IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> targets)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var target in targets)
        {
            map[target.LoadTestBaseUrlEnvironmentVariable] = target.BaseUrl;
        }

        return map;
    }

    public static void ApplyK6Environment(IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> targets)
    {
        foreach (var (envVar, value) in BuildK6BaseUrlEnvironment(targets))
        {
            Environment.SetEnvironmentVariable(envVar, value);
        }

        Environment.SetEnvironmentVariable(
            StlLoadTestSloCatalog.ActiveProfileEnvVar,
            StlLoadTestSloCatalog.ProductOwnerProfile);
    }

    public static async Task<bool> AreEndpointsHealthyAsync(
        IReadOnlyList<StlRenderStagingLoadTestEndpointTarget> targets,
        TimeSpan? timeout = null)
    {
        using var client = new HttpClient { Timeout = timeout ?? TimeSpan.FromSeconds(10) };

        foreach (var target in targets)
        {
            try
            {
                var response = await client.GetAsync($"{target.BaseUrl.TrimEnd('/')}/health");
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        return true;
    }
}
