using Microsoft.Extensions.Options;
using StaffArr.Api.Options;

namespace StaffArr.Api.Services;

public sealed class NexArrLoginDisableClient(
    HttpClient httpClient,
    IOptions<NexArrClientOptions> options)
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.BaseUrl)
        && !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<NexArrLoginDisableResult> TryRequestLoginDisableAsync(
        Guid tenantId,
        Guid personId,
        Guid? externalUserId,
        Guid? requestedByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (externalUserId is null)
        {
            return NexArrLoginDisableResult.Skipped("Person has no linked platform login account.");
        }

        if (!IsConfigured)
        {
            return NexArrLoginDisableResult.Skipped("NexArr login disable integration is not configured.");
        }

        var serviceToken = options.Value.ServiceToken;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/internal/person-login-disable");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(new NexArrLoginDisablePayload(
            tenantId,
            personId,
            externalUserId.Value,
            reason,
            requestedByUserId));

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return NexArrLoginDisableResult.Requested("NexArr login disable request accepted.");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return NexArrLoginDisableResult.Pending(
                $"NexArr login disable request returned {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex)
        {
            return NexArrLoginDisableResult.Pending($"NexArr login disable request failed: {ex.Message}");
        }
    }

    private sealed record NexArrLoginDisablePayload(
        Guid TenantId,
        Guid StaffarrPersonId,
        Guid ExternalUserId,
        string Reason,
        Guid? RequestedByUserId);
}

public sealed record NexArrLoginDisableResult(
    string Outcome,
    string Detail)
{
    public static NexArrLoginDisableResult Requested(string detail) =>
        new("requested", detail);

    public static NexArrLoginDisableResult Skipped(string detail) =>
        new("skipped", detail);

    public static NexArrLoginDisableResult Pending(string detail) =>
        new("pending", detail);
}
