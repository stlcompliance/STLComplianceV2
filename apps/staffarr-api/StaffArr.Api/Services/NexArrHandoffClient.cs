using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using StaffArr.Api.Contracts;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class NexArrHandoffClient(
    HttpClient httpClient,
    IConfiguration configuration)
{
    public async Task<NexArrHandoffRedeemedResponse> RedeemHandoffAsync(
        string handoffCode,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = configuration["Handoff__ServiceToken"] ?? configuration["Handoff:ServiceToken"];
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "handoff.service_token_missing",
                "StaffArr handoff service token is not configured.",
                500);
        }

        using var response = await httpClient.PostAsJsonAsync(
            "/api/launch/handoff/redeem",
            new NexArrRedeemHandoffRequest(handoffCode.Trim(), serviceToken),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "handoff.redeem_failed",
                "NexArr could not redeem the handoff code.",
                (int)response.StatusCode,
                new { upstream = body });
        }

        var redeemed = await response.Content.ReadFromJsonAsync<NexArrHandoffRedeemedResponse>(cancellationToken);
        if (redeemed is null)
        {
            throw new StlApiException("handoff.redeem_invalid_response", "NexArr returned an empty handoff response.", 502);
        }

        return redeemed;
    }
}
