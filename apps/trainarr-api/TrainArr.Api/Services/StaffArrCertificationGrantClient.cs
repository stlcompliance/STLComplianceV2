using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record StaffArrIngestCertificationGrantPayload(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrPublicationId,
    Guid TrainarrAssignmentId,
    string QualificationKey,
    string QualificationName,
    string TrainingDefinitionName,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    string? Notes);

public sealed class StaffArrCertificationGrantClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public async Task IngestGrantAsync(
        StaffArrIngestCertificationGrantPayload payload,
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

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/certification-grants");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.certification_grant_failed",
                $"StaffArr certification grant ingestion failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }
    }
}
