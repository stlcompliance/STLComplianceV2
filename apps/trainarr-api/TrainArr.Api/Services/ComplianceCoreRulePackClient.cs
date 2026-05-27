using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record ComplianceCoreRulePackLookupPayload(
    Guid TenantId,
    IReadOnlyList<string> RulePackKeys);

public sealed record ComplianceCoreRulePackLookupItem(
    string RulePackKey,
    string Label,
    string Description,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    int VersionNumber,
    string Status,
    bool IsActive);

public sealed class ComplianceCoreRulePackClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public async Task<IReadOnlyList<ComplianceCoreRulePackLookupItem>> LookupAsync(
        ComplianceCoreRulePackLookupPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return Array.Empty<ComplianceCoreRulePackLookupItem>();
        }

        if (payload.RulePackKeys.Count == 0)
        {
            return Array.Empty<ComplianceCoreRulePackLookupItem>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/rule-packs/lookup");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.rule_pack_lookup_failed",
                $"Compliance Core rule pack lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<ComplianceCoreRulePackLookupItem>>(
            cancellationToken);
        return results ?? Array.Empty<ComplianceCoreRulePackLookupItem>();
    }
}
