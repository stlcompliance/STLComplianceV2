using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class ComplianceCoreAssetReadinessGateClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<ComplianceCoreProductGateResponse?> CheckAssetReadinessAsync(
        Guid tenantId,
        Guid assetId,
        string assetTag,
        string assetName,
        IReadOnlyDictionary<string, string> context,
        CancellationToken cancellationToken = default)
    {
        var currentOptions = options.Value;
        if (string.IsNullOrWhiteSpace(currentOptions.ServiceToken))
        {
            return null;
        }

        var actionKey = string.IsNullOrWhiteSpace(currentOptions.AssetReadinessActionKey)
            ? "can-dispatch-asset"
            : currentOptions.AssetReadinessActionKey.Trim().ToLowerInvariant();

        var displayLabel = string.IsNullOrWhiteSpace(assetTag)
            ? assetName
            : $"{assetTag} {assetName}".Trim();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/gates/{actionKey}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentOptions.ServiceToken);
        request.Content = JsonContent.Create(new ComplianceCoreProductGateCompatibilityRequest(
            tenantId,
            string.IsNullOrWhiteSpace(currentOptions.AssetReadinessActivityContextKey)
                ? "asset_readiness"
                : currentOptions.AssetReadinessActivityContextKey.Trim().ToLowerInvariant(),
            [
                new("asset", assetId.ToString("D"), "maintainarr", string.IsNullOrWhiteSpace(displayLabel) ? null : displayLabel)
            ],
            context,
            EmitFindings: currentOptions.EmitAssetReadinessFindings,
            WorkflowKey: string.IsNullOrWhiteSpace(currentOptions.AssetReadinessWorkflowKey)
                ? null
                : currentOptions.AssetReadinessWorkflowKey.Trim().ToLowerInvariant()));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.asset_readiness_gate_failed",
                $"Compliance Core asset-readiness gate failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreProductGateResponse>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.asset_readiness_gate_invalid_response",
                "Compliance Core asset-readiness gate returned an empty response.",
                502);
    }
}
