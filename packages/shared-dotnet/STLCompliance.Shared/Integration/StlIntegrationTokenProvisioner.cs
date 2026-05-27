using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace STLCompliance.Shared.Integration;

public static class StlIntegrationTokenProvisioner
{
    public const string AutoProvisionConfigurationKey = "STL_INTEGRATION_TOKEN_AUTO_PROVISION";
    public const string BootstrapSecretConfigurationKey = "STL_INTEGRATION_BOOTSTRAP_SECRET";
    public const string NexArrBaseUrlConfigurationKey = "NexArr__BaseUrl";

    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(3);
    private const int MaxAttempts = 40;

    public static bool IsAutoProvisionEnabled(IConfiguration configuration) =>
        string.Equals(
            configuration[AutoProvisionConfigurationKey],
            "true",
            StringComparison.OrdinalIgnoreCase);

    public static string ResolveConsumerService(IConfiguration configuration) =>
        configuration["RENDER_SERVICE_NAME"]
        ?? configuration["STL_SERVICE_NAME"]
        ?? string.Empty;

    public static IReadOnlyDictionary<string, string> ProvisionSynchronously(
        IConfiguration configuration,
        ILogger? logger = null)
    {
        if (!IsAutoProvisionEnabled(configuration))
        {
            return ReadExistingTokens(configuration);
        }

        var consumerService = ResolveConsumerService(configuration);
        if (string.IsNullOrWhiteSpace(consumerService))
        {
            throw new InvalidOperationException(
                "STL integration token auto-provision is enabled but RENDER_SERVICE_NAME is not configured.");
        }

        if (string.Equals(consumerService, "nexarr-api", StringComparison.OrdinalIgnoreCase))
        {
            return ReadExistingTokens(configuration);
        }

        var profiles = StlIntegrationTokenCatalog.ForConsumer(consumerService);
        if (profiles.Count == 0)
        {
            return ReadExistingTokens(configuration);
        }

        if (HasValidProvisionedTokens(configuration, profiles))
        {
            logger?.LogInformation(
                "Using configured integration tokens for {ConsumerService}.",
                consumerService);
            return ReadExistingTokens(configuration, profiles);
        }

        var nexArrBaseUrl = configuration[NexArrBaseUrlConfigurationKey]
            ?? configuration["NexArr:BaseUrl"];
        var bootstrapSecret = configuration[BootstrapSecretConfigurationKey];

        if (string.IsNullOrWhiteSpace(nexArrBaseUrl))
        {
            throw new InvalidOperationException(
                "STL integration token auto-provision requires NexArr__BaseUrl.");
        }

        if (string.IsNullOrWhiteSpace(bootstrapSecret))
        {
            throw new InvalidOperationException(
                "STL integration token auto-provision requires STL_INTEGRATION_BOOTSTRAP_SECRET.");
        }

        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(nexArrBaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        Exception? lastError = null;
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"/api/internal/integration-tokens?consumer={Uri.EscapeDataString(consumerService)}");
                request.Headers.Add("X-Integration-Bootstrap-Secret", bootstrapSecret);

                using var response = httpClient.Send(request);
                if (!response.IsSuccessStatusCode)
                {
                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    lastError = new InvalidOperationException(
                        $"NexArr integration token provision failed ({(int)response.StatusCode}): {body}");
                }
                else
                {
                    var payload = response.Content
                        .ReadFromJsonAsync<IntegrationTokenProvisionResponse>()
                        .GetAwaiter()
                        .GetResult();

                    if (payload?.Tokens is null || payload.Tokens.Count == 0)
                    {
                        lastError = new InvalidOperationException(
                            $"NexArr returned no integration tokens for {consumerService}.");
                    }
                    else
                    {
                        logger?.LogInformation(
                            "Provisioned {Count} integration token(s) for {ConsumerService} from NexArr.",
                            payload.Tokens.Count,
                            consumerService);
                        return payload.Tokens;
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                lastError = ex;
            }

            if (attempt < MaxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(InitialRetryDelay.TotalSeconds + attempt, 30));
                logger?.LogWarning(
                    lastError,
                    "Integration token provision attempt {Attempt}/{MaxAttempts} failed for {ConsumerService}; retrying in {DelaySeconds}s.",
                    attempt,
                    MaxAttempts,
                    consumerService,
                    delay.TotalSeconds);
                Thread.Sleep(delay);
            }
        }

        throw new InvalidOperationException(
            $"Unable to provision integration tokens for {consumerService} from NexArr after {MaxAttempts} attempts.",
            lastError);
    }

    private static bool HasValidProvisionedTokens(
        IConfiguration configuration,
        IReadOnlyList<StlIntegrationTokenProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            var value = configuration[profile.ConfigurationKey];
            if (!IsLikelyServiceTokenJwt(value))
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyDictionary<string, string> ReadExistingTokens(IConfiguration configuration) =>
        StlIntegrationTokenCatalog.All
            .Select(p => p.ConfigurationKey)
            .Distinct(StringComparer.Ordinal)
            .Where(key => !string.IsNullOrWhiteSpace(configuration[key]))
            .ToDictionary(key => key, key => configuration[key]!, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> ReadExistingTokens(
        IConfiguration configuration,
        IReadOnlyList<StlIntegrationTokenProfile> profiles) =>
        profiles
            .Where(p => !string.IsNullOrWhiteSpace(configuration[p.ConfigurationKey]))
            .ToDictionary(p => p.ConfigurationKey, p => configuration[p.ConfigurationKey]!, StringComparer.Ordinal);

    public static bool IsLikelyServiceTokenJwt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('.');
        return parts.Length == 3
            && parts.All(part => part.Length > 0);
    }

    private sealed record IntegrationTokenProvisionResponse(
        [property: JsonPropertyName("tokens")] IReadOnlyDictionary<string, string> Tokens);
}
