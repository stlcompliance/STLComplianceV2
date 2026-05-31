using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record MaintainArrRoutarrEventIngestRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    string RelatedEntityType,
    Guid RelatedEntityId,
    Guid CorrelationId,
    RoutArrIntegrationOutboxPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record MaintainArrRoutarrEventIngestResponse(
    Guid InboundEventId,
    string Outcome,
    Guid? DefectId,
    bool IdempotentReplay);

public sealed class MaintainArrRoutarrEventClient(
    HttpClient httpClient,
    IOptions<MaintainArrClientOptions> options)
{
    public async Task<MaintainArrRoutarrEventIngestResponse> IngestAsync(
        MaintainArrRoutarrEventIngestRequest payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "maintainarr.routarr_events_not_configured",
                "MaintainArr RoutArr event ingestion is not configured.",
                503);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/routarr-events");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "maintainarr.routarr_event_ingest_failed",
                $"MaintainArr RoutArr event ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrRoutarrEventIngestResponse>(cancellationToken)
            ?? throw new StlApiException(
                "maintainarr.routarr_event_ingest_empty_response",
                "MaintainArr RoutArr event ingestion returned an empty response.",
                502);
    }
}
