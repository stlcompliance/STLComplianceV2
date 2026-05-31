using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed record TrainArrQualificationCheckRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string? RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record TrainArrQualificationCheckResponse(
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
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.ServiceToken)
        && !string.IsNullOrWhiteSpace(options.Value.TechnicianQualificationKey);

    public string TechnicianQualificationKey => options.Value.TechnicianQualificationKey.Trim();

    public string? TechnicianRulePackKey => string.IsNullOrWhiteSpace(options.Value.TechnicianRulePackKey)
        ? null
        : options.Value.TechnicianRulePackKey.Trim();

    public async Task<TrainArrQualificationCheckResponse?> CheckTechnicianAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/qualification-check");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ServiceToken);
        request.Content = JsonContent.Create(new TrainArrQualificationCheckRequest(
            tenantId,
            staffarrPersonId,
            TechnicianQualificationKey,
            TechnicianRulePackKey,
            null));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.technician_qualification_check_failed",
                $"TrainArr technician qualification check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TrainArrQualificationCheckResponse>(cancellationToken)
            ?? throw new StlApiException(
                "trainarr.technician_qualification_check_invalid_response",
                "TrainArr technician qualification check returned an empty response.",
                502);
    }
}
