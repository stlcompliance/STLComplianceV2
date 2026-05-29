using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed record MaintainArrIngestStaffarrPersonSyncPayload(
    Guid TenantId,
    Guid StaffarrPersonId,
    string DisplayName,
    string EmploymentStatus,
    string? PrimarySite,
    string EventType,
    DateTimeOffset OccurredAt,
    string? CorrelationId);

public sealed record MaintainArrStaffarrPersonSyncResult(
    string PersonId,
    string DisplayName,
    bool IdempotentReplay);

public sealed class MaintainArrTechnicianRefSyncClient(
    HttpClient httpClient,
    IOptions<MaintainArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<MaintainArrStaffarrPersonSyncResult?> TrySyncPersonAsync(
        MaintainArrIngestStaffarrPersonSyncPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/staffarr-person-sync");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "maintainarr.technician_ref_sync_failed",
                $"MaintainArr StaffArr person sync failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<MaintainArrStaffarrPersonSyncResult>(cancellationToken);
    }
}
