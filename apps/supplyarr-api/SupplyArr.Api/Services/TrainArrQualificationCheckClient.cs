using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using SupplyArr.Api.Options;

namespace SupplyArr.Api.Services;

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
    public bool IsReceivingCheckConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.ServiceToken)
        && !string.IsNullOrWhiteSpace(options.Value.ReceivingQualificationKey);

    public string ReceivingQualificationKey => options.Value.ReceivingQualificationKey.Trim();

    public string? ReceivingRulePackKey => string.IsNullOrWhiteSpace(options.Value.ReceivingRulePackKey)
        ? null
        : options.Value.ReceivingRulePackKey.Trim();

    public async Task<TrainArrQualificationCheckResponse?> CheckReceivingAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken = default)
    {
        if (!IsReceivingCheckConfigured)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/integrations/qualification-check");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ServiceToken);
        request.Content = JsonContent.Create(new TrainArrQualificationCheckRequest(
            tenantId,
            staffarrPersonId,
            ReceivingQualificationKey,
            ReceivingRulePackKey,
            context));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "trainarr.receiving_qualification_check_failed",
                $"TrainArr receiving qualification check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TrainArrQualificationCheckResponse>(cancellationToken)
            ?? throw new StlApiException(
                "trainarr.receiving_qualification_check_invalid_response",
                "TrainArr receiving qualification check returned an empty response.",
                502);
    }
}
