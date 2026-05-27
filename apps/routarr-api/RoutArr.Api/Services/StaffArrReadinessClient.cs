using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record StaffArrIntegrationReadinessResponse(
    Guid PersonId,
    string ReadinessStatus,
    string ReadinessBasis,
    IReadOnlyList<StaffArrIntegrationReadinessBlockerResponse> Blockers);

public sealed record StaffArrIntegrationReadinessBlockerResponse(
    string BlockerSource,
    string BlockerType,
    string Message,
    string? CertificationKey,
    string? QualificationKey);

public sealed class StaffArrReadinessClient(
    HttpClient httpClient,
    IOptions<StaffArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<StaffArrIntegrationReadinessResponse?> GetReadinessAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/integrations/routarr-readiness?tenantId={tenantId}&personId={personId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new StlApiException("staffarr.person_not_found", "StaffArr person was not found.", 404);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "staffarr.readiness_check_failed",
                $"StaffArr readiness check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<StaffArrIntegrationReadinessResponse>(cancellationToken);
    }
}
