using System.Net;
using System.Net.Http.Json;
using STLCompliance.Shared.Health;
using STLCompliance.Shared.Http;
using STLCompliance.Shared.Operations.LoadTesting;

namespace STLCompliance.Shared.Operations;

public sealed record StlRenderStagingShipGateApiTarget(
    string ProductKey,
    Uri BaseUrl,
    string SourceApiUrlEnvironmentVariable,
    string E2eApiUrlEnvironmentVariable);

public sealed record StlRenderStagingShipGateStaticSiteTarget(
    string SiteName,
    Uri BaseUrl,
    string SourceUrlEnvironmentVariable);

public static class StlRenderStagingShipGateSupport
{
    public static bool LiveModeEnabled =>
        string.Equals(Environment.GetEnvironmentVariable(StlRenderStagingShipGateCatalog.LiveModeEnvironmentVariable), "1", StringComparison.Ordinal)
        || string.Equals(Environment.GetEnvironmentVariable(StlRenderStagingShipGateCatalog.LiveModeEnvironmentVariable), "true", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> GetMissingStagingApiUrlEnvironmentVariables()
    {
        var missing = new List<string>();
        foreach (var environmentVariable in StlRenderStagingShipGateCatalog.RequiredStagingApiUrlEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentVariable)))
            {
                missing.Add(environmentVariable);
            }
        }

        return missing;
    }

    public static bool AreStagingApiUrlsConfigured() =>
        GetMissingStagingApiUrlEnvironmentVariables().Count == 0;

    public static IReadOnlyList<StlRenderStagingShipGateApiTarget> ResolveApiTargetsFromEnvironment()
    {
        var targets = new List<StlRenderStagingShipGateApiTarget>();
        var missing = new List<string>();

        foreach (var entry in StlRenderStagingShipGateCatalog.ApiProbes)
        {
            var raw = Environment.GetEnvironmentVariable(entry.SourceApiUrlEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(raw))
            {
                missing.Add(entry.SourceApiUrlEnvironmentVariable);
                continue;
            }

            var normalized = StlServiceUrl.NormalizeHttpBaseUrl(raw);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new InvalidOperationException(
                    $"API URL for '{entry.ProductKey}' is empty after normalization ({entry.SourceApiUrlEnvironmentVariable}).");
            }

            targets.Add(new StlRenderStagingShipGateApiTarget(
                entry.ProductKey,
                new Uri(normalized, UriKind.Absolute),
                entry.SourceApiUrlEnvironmentVariable,
                entry.E2eApiUrlEnvironmentVariable));
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing staging API URL environment variables: {string.Join(", ", missing)}");
        }

        return targets;
    }

    public static IReadOnlyList<StlRenderStagingShipGateStaticSiteTarget> ResolveConfiguredStaticSiteTargetsFromEnvironment()
    {
        var targets = new List<StlRenderStagingShipGateStaticSiteTarget>();

        foreach (var entry in StlRenderStagingShipGateCatalog.OptionalStaticSiteProbes)
        {
            var raw = Environment.GetEnvironmentVariable(entry.SourceUrlEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var normalized = StlServiceUrl.NormalizeHttpBaseUrl(raw);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            targets.Add(new StlRenderStagingShipGateStaticSiteTarget(
                entry.SiteName,
                new Uri(normalized, UriKind.Absolute),
                entry.SourceUrlEnvironmentVariable));
        }

        return targets;
    }

    public static void ApplyStagingApiUrlsToE2eEnvironment(IReadOnlyList<StlRenderStagingShipGateApiTarget>? targets = null)
    {
        var resolved = targets ?? ResolveApiTargetsFromEnvironment();
        foreach (var target in resolved)
        {
            Environment.SetEnvironmentVariable(target.E2eApiUrlEnvironmentVariable, target.BaseUrl.ToString().TrimEnd('/'));
        }

        Environment.SetEnvironmentVariable("E2E_LIVE", "1");
    }

    public static async Task<bool> AreAllApisHealthyAsync(
        IReadOnlyList<StlRenderStagingShipGateApiTarget> targets,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) =>
        await ProbeAllApisAsync(targets, "/health", timeout, cancellationToken);

    public static async Task<bool> AreAllApisReadyAsync(
        IReadOnlyList<StlRenderStagingShipGateApiTarget> targets,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default) =>
        await ProbeAllApisAsync(targets, StlRenderBlueprintCatalog.ApiHealthCheckPath, timeout, cancellationToken);

    public static async Task<IReadOnlyList<string>> GetUnhealthyApiMessagesAsync(
        IReadOnlyList<StlRenderStagingShipGateApiTarget> targets,
        string healthPath,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        using var client = CreateClient(timeout);

        foreach (var target in targets)
        {
            var probeUrl = new Uri(target.BaseUrl, healthPath);
            try
            {
                var response = await client.GetAsync(probeUrl, cancellationToken);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    errors.Add($"{target.ProductKey} {probeUrl} returned {(int)response.StatusCode}");
                    continue;
                }

                var payload = await response.Content.ReadFromJsonAsync<HealthResponse>(cancellationToken: cancellationToken);
                if (payload is null || !string.Equals(payload.Status, "Healthy", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"{target.ProductKey} {probeUrl} status is '{payload?.Status ?? "null"}'");
                    continue;
                }

                if (!string.Equals(payload.Product, target.ProductKey, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"{target.ProductKey} {probeUrl} product mismatch '{payload.Product}'");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                errors.Add($"{target.ProductKey} {probeUrl} unreachable: {ex.Message}");
            }
        }

        return errors;
    }

    public static async Task<IReadOnlyList<string>> GetUnreachableStaticSiteMessagesAsync(
        IReadOnlyList<StlRenderStagingShipGateStaticSiteTarget> targets,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        using var client = CreateClient(timeout);

        foreach (var target in targets)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, target.BaseUrl);
                var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    errors.Add($"{target.SiteName} {target.BaseUrl} returned {(int)response.StatusCode}");
                    continue;
                }

                foreach (var headerName in StlRenderBlueprintCatalog.StaticSecurityHeaderNames)
                {
                    if (!response.Headers.Contains(headerName))
                    {
                        errors.Add($"{target.SiteName} {target.BaseUrl} missing response header '{headerName}'");
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                errors.Add($"{target.SiteName} {target.BaseUrl} unreachable: {ex.Message}");
            }
        }

        return errors;
    }

    public static (string Email, string Password, Guid TenantId) ResolveDemoCredentials()
    {
        var email = Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.EmailEnvVar)
            ?? StlLoadTestAuthDefaults.DemoEmail;
        var password = Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.PasswordEnvVar)
            ?? StlLoadTestAuthDefaults.DemoPassword;
        var tenantRaw = Environment.GetEnvironmentVariable(StlLoadTestAuthDefaults.TenantIdEnvVar)
            ?? StlLoadTestAuthDefaults.DemoTenantId;

        if (!Guid.TryParse(tenantRaw, out var tenantId))
        {
            throw new InvalidOperationException(
                $"Invalid tenant id in {StlLoadTestAuthDefaults.TenantIdEnvVar}: '{tenantRaw}'.");
        }

        return (email, password, tenantId);
    }

    private static async Task<bool> ProbeAllApisAsync(
        IReadOnlyList<StlRenderStagingShipGateApiTarget> targets,
        string healthPath,
        TimeSpan? timeout,
        CancellationToken cancellationToken)
    {
        var errors = await GetUnhealthyApiMessagesAsync(targets, healthPath, timeout, cancellationToken);
        return errors.Count == 0;
    }

    private static HttpClient CreateClient(TimeSpan? timeout) =>
        new() { Timeout = timeout ?? TimeSpan.FromSeconds(15) };
}
