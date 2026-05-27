using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record StaffArrIngestCertificationLifecyclePayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrGrantPublicationId,
    Guid TrainarrLifecyclePublicationId,
    string LifecycleAction,
    string QualificationKey,
    string QualificationName,
    string Message,
    DateTimeOffset? ExpiresAt);

public sealed class StaffArrCertificationLifecycleClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task IngestLifecycleAsync(
        StaffArrIngestCertificationLifecyclePayload payload,
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/certification-lifecycle");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.certification_lifecycle_failed",
                $"StaffArr certification lifecycle ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
