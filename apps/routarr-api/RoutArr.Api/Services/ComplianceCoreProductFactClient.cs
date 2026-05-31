using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record ComplianceCoreProductFactPublicationItem(
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

public sealed record ComplianceCoreProductFactIngestRequest(
    Guid TenantId,
    Guid PublicationId,
    string SourceProduct,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ComplianceCoreProductFactPublicationItem> Facts);

public sealed record ComplianceCoreProductFactIngestResponse(
    Guid TenantId,
    Guid PublicationId,
    int AcceptedCount,
    int SkippedDuplicateCount);

public sealed class ComplianceCoreProductFactClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public async Task<ComplianceCoreProductFactIngestResponse> IngestAsync(
        ComplianceCoreProductFactIngestRequest payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "compliancecore.product_facts_not_configured",
                "Compliance Core product fact ingestion is not configured.",
                503);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/product-facts/ingest");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.product_fact_ingest_failed",
                $"Compliance Core product fact ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreProductFactIngestResponse>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.product_fact_ingest_empty_response",
                "Compliance Core product fact ingestion returned an empty response.",
                502);
    }
}
