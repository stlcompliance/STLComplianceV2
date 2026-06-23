using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;

namespace StaffArr.Api.Services;

public sealed class NexArrLoginEnableClient(
    HttpClient httpClient,
    IOptions<NexArrClientOptions> options)
{
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(options.Value.BaseUrl)
        && !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<NexArrLoginEnableResult> RequestLoginEnableAsync(
        Guid tenantId,
        Guid personId,
        Guid externalUserId,
        Guid? requestedByUserId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return NexArrLoginEnableResult.Pending("NexArr login enable integration is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/person-login-enable");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Value.ServiceToken);
        request.Content = JsonContent.Create(new NexArrLoginEnablePayload(
            tenantId,
            personId,
            externalUserId,
            reason,
            requestedByUserId));

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return NexArrLoginEnableResult.Requested("NexArr login enable request accepted.");
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return NexArrLoginEnableResult.Pending($"NexArr login enable request returned {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex)
        {
            return NexArrLoginEnableResult.Pending($"NexArr login enable request failed: {ex.Message}");
        }
    }

    private sealed record NexArrLoginEnablePayload(
        Guid TenantId,
        Guid StaffarrPersonId,
        Guid ExternalUserId,
        string? Reason,
        Guid? RequestedByUserId);
}

public sealed record NexArrLoginEnableResult(
    string Outcome,
    string Detail)
{
    public static NexArrLoginEnableResult Requested(string detail) =>
        new("requested", detail);

    public static NexArrLoginEnableResult Pending(string detail) =>
        new("pending", detail);
}
