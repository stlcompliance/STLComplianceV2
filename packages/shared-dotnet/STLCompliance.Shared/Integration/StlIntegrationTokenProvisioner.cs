using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using STLCompliance.Shared.Auth;

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

    public static string ResolveConsumerService(IConfiguration configuration)
    {
        var configured = configuration["STL_SERVICE_NAME"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim();
        }

        var renderName = configuration["RENDER_SERVICE_NAME"]?.Trim();
        if (string.IsNullOrWhiteSpace(renderName))
        {
            return string.Empty;
        }

        // Render slugs append a random suffix (e.g. staffarr-api-58w6); catalog uses blueprint names.
        var knownConsumers = StlIntegrationTokenCatalog.All
            .Select(static profile => profile.ConsumerService)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var consumer in knownConsumers)
        {
            if (string.Equals(renderName, consumer, StringComparison.OrdinalIgnoreCase)
                || renderName.StartsWith(consumer + "-", StringComparison.OrdinalIgnoreCase))
            {
                return consumer;
            }
        }

        return renderName;
    }

    public static IReadOnlyList<string> ExpandConfigurationKeys(string configurationKey)
    {
        var normalizedKey = NormalizeConfigurationKey(configurationKey);
        if (string.Equals(configurationKey, normalizedKey, StringComparison.Ordinal))
        {
            return [configurationKey];
        }

        return [configurationKey, normalizedKey];
    }

    public static string NormalizeConfigurationKey(string configurationKey) =>
        configurationKey.Replace("__", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);

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

        logger?.LogInformation(
            "Resolving integration tokens for consumer {ConsumerService} (render={RenderServiceName}).",
            consumerService,
            configuration["RENDER_SERVICE_NAME"]);

        if (string.Equals(consumerService, "nexarr-api", StringComparison.OrdinalIgnoreCase))
        {
            return ReadExistingTokens(configuration);
        }

        var profiles = StlIntegrationTokenCatalog.ForConsumer(consumerService);
        if (profiles.Count == 0)
        {
            throw new InvalidOperationException(
                $"STL integration token auto-provision has no catalog profiles for consumer '{consumerService}'.");
        }

        if (string.Equals(consumerService, "nexarr-worker", StringComparison.OrdinalIgnoreCase))
        {
            return ProvisionNexArrWorkerTokensLocally(configuration, logger, profiles);
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
            var value = ReadConfigurationValue(configuration, profile.ConfigurationKey);
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
            .Select(key => (Key: key, Value: ReadConfigurationValue(configuration, key)))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key, pair => pair.Value!, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> ReadExistingTokens(
        IConfiguration configuration,
        IReadOnlyList<StlIntegrationTokenProfile> profiles) =>
        profiles
            .Select(profile => (profile.ConfigurationKey, Value: ReadConfigurationValue(configuration, profile.ConfigurationKey)))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.ConfigurationKey, pair => pair.Value!, StringComparer.Ordinal);

    private static string? ReadConfigurationValue(IConfiguration configuration, string configurationKey)
    {
        foreach (var lookupKey in ExpandConfigurationKeys(configurationKey))
        {
            var value = configuration[lookupKey];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string> ProvisionNexArrWorkerTokensLocally(
        IConfiguration configuration,
        ILogger? logger,
        IReadOnlyList<StlIntegrationTokenProfile> profiles)
    {
        if (HasValidProvisionedTokens(configuration, profiles))
        {
            logger?.LogInformation("Using configured integration tokens for nexarr-worker.");
            return ReadExistingTokens(configuration, profiles);
        }

        var options = new StlServiceTokenOptions();
        var issuer = StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options);
        var audience = StlServiceTokenKeyMaterial.ResolveAudience(configuration, options);
        var credentials = StlServiceTokenKeyMaterial.CreateSigningCredentials(configuration, options);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var handler = new JwtSecurityTokenHandler();

        var tokens = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var profile in profiles)
        {
            var tokenId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
                new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
                new(StlServiceTokenClaimTypes.ServiceClientId, Guid.Empty.ToString()),
                new(StlServiceTokenClaimTypes.SourceProduct, profile.SourceProductKey),
                new(StlServiceTokenClaimTypes.AllowedProducts, string.Join(',', profile.AllowedProductKeys)),
                new(StlServiceTokenClaimTypes.TokenId, tokenId.ToString()),
                new(StlServiceTokenClaimTypes.ActionScope, profile.ActionScope)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials);

            tokens[profile.ConfigurationKey] = handler.WriteToken(token);
        }

        logger?.LogInformation(
            "Locally provisioned {Count} integration token(s) for nexarr-worker.",
            tokens.Count);
        return tokens;
    }

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
