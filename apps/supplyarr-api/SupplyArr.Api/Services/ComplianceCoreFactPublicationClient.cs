using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record ComplianceCoreFactPublicationItem(
    string FactKey,
    string ValueType,
    string ScopeKey,
    string? StringValue,
    bool? BooleanValue,
    decimal? NumberValue,
    string? DateValue,
    string SourceEntityType,
    Guid? SourceEntityId,
    string SourceEventKind,
    string IdempotencyKey);

public sealed record ComplianceCoreIngestProductFactsPayload(
    Guid TenantId,
    Guid PublicationId,
    string SourceProduct,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ComplianceCoreFactPublicationItem> Facts);

public sealed record ComplianceCoreIngestProductFactsResult(
    Guid TenantId,
    Guid PublicationId,
    int AcceptedCount,
    int SkippedDuplicateCount);

public sealed class ComplianceCoreFactPublicationClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public async Task<ComplianceCoreIngestProductFactsResult> IngestAsync(
        ComplianceCoreIngestProductFactsPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "compliancecore.service_token_missing",
                "SupplyArr Compliance Core service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/product-facts/ingest");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.fact_publication_failed",
                $"Compliance Core fact ingest failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreIngestProductFactsResult>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.fact_publication_invalid_response",
                "Compliance Core fact ingest returned an empty response.",
                502);
    }
}
