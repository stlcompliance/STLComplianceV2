using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Options;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformJourneySeedService(
    IHttpClientFactory httpClientFactory,
    IOptions<PlatformProductUrlsOptions> options,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public const string HttpClientName = "PlatformJourneySeedClient";

    private static readonly IReadOnlyList<JourneySeedTargetDefinition> SupportedTargets =
    [
        new("compliancecore", "Compliance Core", "Seed the load-test rule pack, rule content, driver license fact, and dispatch gates.", "/api/load-test-journey/seed"),
        new("trainarr", "TrainArr", "Seed the load-test qualification, assignment, and publication inputs.", "/api/load-test-journey/seed"),
        new("routarr", "RoutArr", "Seed the load-test dispatch trip mirror for routing workflows.", "/api/load-test-journey/seed"),
        new("supplyarr", "SupplyArr", "Seed the load-test demand reference and work order inputs.", "/api/load-test-journey/seed"),
    ];

    public async Task<IReadOnlyList<JourneySeedTargetResponse>> GetTargetsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return SupportedTargets
            .Select(target =>
            {
                var baseUrl = ResolveBaseUrl(target.ProductKey);
                return new JourneySeedTargetResponse(
                    target.ProductKey,
                    target.DisplayName,
                    target.Description,
                    target.SeedPath,
                    baseUrl,
                    !string.IsNullOrWhiteSpace(baseUrl));
            })
            .ToList();
    }

    public async Task<JourneySeedResultResponse> SeedAsync(
        ClaimsPrincipal principal,
        string productKey,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var target = ResolveTarget(productKey);
        if (target is null)
        {
            throw new StlApiException(
                "journey_seed.unknown_product",
                $"Unsupported journey seed target '{productKey}'.",
                404);
        }

        var baseUrl = ResolveBaseUrl(target.ProductKey);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new StlApiException(
                "journey_seed.product_url_missing",
                $"No base URL is configured for {target.DisplayName}.",
                503);
        }

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            throw new StlApiException(
                "journey_seed.authorization_missing",
                "An authorization header is required to seed product data.",
                401);
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var parsedAuthorization))
        {
            throw new StlApiException(
                "journey_seed.authorization_invalid",
                "The authorization header is not in a valid Bearer token format.",
                401);
        }

        var requestAt = DateTimeOffset.UtcNow;
        var client = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}{target.SeedPath}");
            request.Headers.Authorization = parsedAuthorization;

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var success = response.IsSuccessStatusCode;

            await audit.WriteAsync(
                "platform_journey_seed.seed",
                "product",
                target.ProductKey,
                success ? "Success" : "Failed",
                actorUserId: actorUserId,
                reasonCode: ((int)response.StatusCode).ToString(),
                cancellationToken: cancellationToken);

            return new JourneySeedResultResponse(
                target.ProductKey,
                target.DisplayName,
                target.Description,
                target.SeedPath,
                baseUrl,
                true,
                success,
                (int)response.StatusCode,
                string.IsNullOrWhiteSpace(body) ? null : body,
                requestAt);
        }
        catch (HttpRequestException ex)
        {
            await audit.WriteAsync(
                "platform_journey_seed.seed",
                "product",
                target.ProductKey,
                "Failed",
                actorUserId: actorUserId,
                reasonCode: "upstream_unreachable",
                cancellationToken: cancellationToken);

            return new JourneySeedResultResponse(
                target.ProductKey,
                target.DisplayName,
                target.Description,
                target.SeedPath,
                baseUrl,
                true,
                false,
                503,
                ex.Message,
                requestAt);
        }
    }

    private string? ResolveBaseUrl(string productKey)
    {
        var normalizedKey = productKey.Trim().ToLowerInvariant();
        return normalizedKey switch
        {
            "compliancecore" => options.Value.ComplianceCoreBaseUrl,
            "trainarr" => options.Value.TrainArrBaseUrl,
            "routarr" => options.Value.RoutArrBaseUrl,
            "supplyarr" => options.Value.SupplyArrBaseUrl,
            _ => null,
        };
    }

    private static JourneySeedTargetDefinition? ResolveTarget(string productKey)
    {
        var normalizedKey = productKey.Trim().ToLowerInvariant();
        return SupportedTargets.FirstOrDefault(target =>
            string.Equals(target.ProductKey, normalizedKey, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record JourneySeedTargetDefinition(
        string ProductKey,
        string DisplayName,
        string Description,
        string SeedPath);
}
