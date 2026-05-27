using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Contracts;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record TrainArrIntegrationQualificationCheckRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record TrainArrIntegrationQualificationCheckResponse(
    Guid CheckId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string Outcome,
    string ReasonCode,
    string Message);

public sealed class TrainArrQualificationCheckClient(
    HttpClient httpClient,
    IOptions<TrainArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<TrainArrIntegrationQualificationCheckResponse?> CheckAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        string qualificationKey,
        string? rulePackKey,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/routarr-qualification-check");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(new TrainArrIntegrationQualificationCheckRequest(
            tenantId,
            staffarrPersonId,
            qualificationKey,
            rulePackKey,
            null));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.qualification_check_failed",
                $"TrainArr qualification check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TrainArrIntegrationQualificationCheckResponse>(cancellationToken);
    }
}
