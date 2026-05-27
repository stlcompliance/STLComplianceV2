using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record StaffArrIngestTrainingBlockerPayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId,
    string QualificationKey,
    string QualificationName,
    string BlockerType,
    string Message,
    DateTimeOffset? ExpiresAt);

public sealed record StaffArrClearTrainingBlockerPayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId);

public sealed class StaffArrTrainingBlockerClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task PublishBlockerAsync(
        StaffArrIngestTrainingBlockerPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "TrainArr StaffArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/training-blockers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.publication_failed",
                $"StaffArr training blocker ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }

    public async Task ClearBlockerAsync(
        StaffArrClearTrainingBlockerPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "TrainArr StaffArr service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/training-blockers/clear");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.publication_clear_failed",
                $"StaffArr training blocker clear failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
