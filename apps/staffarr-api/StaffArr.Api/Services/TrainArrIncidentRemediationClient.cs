using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed record TrainArrIngestIncidentRemediationPayload(
    Guid TenantId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Severity,
    string Title,
    string Description,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReportedAt);

public sealed record TrainArrIncidentRemediationResult(
    Guid RemediationId,
    Guid TenantId,
    Guid StaffarrIncidentId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Status,
    DateTimeOffset CreatedAt);

public sealed class TrainArrIncidentRemediationClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public async Task<TrainArrIncidentRemediationResult> IngestRemediationAsync(
        TrainArrIngestIncidentRemediationPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "trainarr.service_token_missing",
                "StaffArr TrainArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/incident-remediations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.incident_routing_failed",
                $"TrainArr incident remediation intake failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<TrainArrIncidentRemediationResult>(cancellationToken);
        if (result is null)
        {
            throw new StlApiException(
                "trainarr.incident_routing_failed",
                "TrainArr incident remediation intake returned an empty response.",
                502);
        }

        return result;
    }
}
