using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record TrainArrRoutarrIncidentRemediationRequest(
    Guid TenantId,
    Guid SourceEventId,
    string EventKind,
    Guid RelatedEntityId,
    Guid CorrelationId,
    RoutArrIntegrationOutboxPayload Payload,
    DateTimeOffset? OccurredAt = null);

public sealed record TrainArrRoutarrIncidentRemediationResponse(
    Guid RemediationId,
    Guid TenantId,
    Guid SourceEventId,
    Guid StaffarrPersonId,
    string Status,
    bool IdempotentReplay);

public sealed class TrainArrIncidentRemediationClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public async Task<TrainArrRoutarrIncidentRemediationResponse> IngestAsync(
        TrainArrRoutarrIncidentRemediationRequest payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "trainarr.service_token_missing",
                "RoutArr TrainArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/routarr-incident-remediations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.routarr_incident_remediation_failed",
                $"TrainArr RoutArr incident remediation ingest failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TrainArrRoutarrIncidentRemediationResponse>(cancellationToken)
            ?? throw new StlApiException(
                "trainarr.routarr_incident_remediation_invalid_response",
                "TrainArr RoutArr incident remediation ingest returned an empty response.",
                502);
    }
}
