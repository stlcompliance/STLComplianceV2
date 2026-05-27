using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record MaintainArrIntegrationAssetReadinessResponse(
    Guid AssetId,
    string AssetTag,
    string ReadinessStatus,
    string ReadinessBasis,
    IReadOnlyList<MaintainArrIntegrationAssetReadinessBlockerResponse> Blockers);

public sealed record MaintainArrIntegrationAssetReadinessBlockerResponse(
    string BlockerType,
    string Message,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);

public sealed class MaintainArrAssetReadinessClient(
    HttpClient httpClient,
    IOptions<MaintainArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<MaintainArrIntegrationAssetReadinessResponse?> GetReadinessAsync(
        Guid tenantId,
        string? vehicleRefKey,
        string? assetTag,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        var query = new List<string> { $"tenantId={tenantId}" };
        if (!string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            query.Add($"vehicleRefKey={Uri.EscapeDataString(vehicleRefKey.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(assetTag))
        {
            query.Add($"assetTag={Uri.EscapeDataString(assetTag.Trim())}");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/routarr-asset-readiness?{string.Join('&', query)}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "maintainarr.asset_readiness_check_failed",
                $"MaintainArr asset readiness check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrIntegrationAssetReadinessResponse>(cancellationToken);
    }
}
