using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed record StaffArrProductIncidentIngestRequest(
    Guid TenantId,
    string SourceProduct,
    Guid SourceIncidentId,
    string? SourceEventKind,
    Guid PersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    string? SourceReferenceKey = null);

public sealed record StaffArrProductIncidentIngestResponse(
    Guid IncidentId,
    Guid PersonId,
    string SourceProduct,
    Guid SourceIncidentId,
    string Status,
    bool IdempotentReplay);

public sealed class StaffArrProductIncidentClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<StaffArrProductIncidentIngestResponse> IngestAsync(
        StaffArrProductIncidentIngestRequest payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "SupplyArr StaffArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/product-incidents");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.product_incident_ingest_failed",
                $"StaffArr product incident ingest failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrProductIncidentIngestResponse>(cancellationToken)
            ?? throw new StlApiException(
                "staffarr.product_incident_ingest_invalid_response",
                "StaffArr product incident ingest returned an empty response.",
                502);
    }
}
