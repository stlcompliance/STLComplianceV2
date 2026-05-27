using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record ComplianceCoreCitationLookupPayload(
    Guid TenantId,
    IReadOnlyList<Guid> CitationIds);

public sealed record ComplianceCoreCitationLookupItem(
    Guid CitationId,
    string CitationKey,
    int VersionNumber,
    string Label,
    string SourceReference,
    string Description,
    string RegulatoryProgramKey,
    string? RulePackKey,
    bool IsActive);

public sealed class ComplianceCoreCitationClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public async Task<IReadOnlyList<ComplianceCoreCitationLookupItem>> LookupAsync(
        ComplianceCoreCitationLookupPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return Array.Empty<ComplianceCoreCitationLookupItem>();
        }

        if (payload.CitationIds.Count == 0)
        {
            return Array.Empty<ComplianceCoreCitationLookupItem>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/citations/lookup");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.citation_lookup_failed",
                $"Compliance Core citation lookup failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<ComplianceCoreCitationLookupItem>>(
            cancellationToken);
        return results ?? Array.Empty<ComplianceCoreCitationLookupItem>();
    }
}
