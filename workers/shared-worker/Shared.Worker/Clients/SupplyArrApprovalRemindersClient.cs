using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record SupplyArrProcessApprovalRemindersResult(
    int CandidatesFound,
    int RemindersSentCount,
    int SkippedCount);

public sealed class SupplyArrApprovalRemindersClient(
    HttpClient httpClient,
    IOptions<SupplyArrApprovalRemindersOptions> options)
{
    public async Task<SupplyArrProcessApprovalRemindersResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/approval-reminders/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessApprovalRemindersPayload>(cancellationToken);
        return new SupplyArrProcessApprovalRemindersResult(
            payload?.CandidatesFound ?? 0,
            payload?.RemindersSentCount ?? 0,
            payload?.SkippedCount ?? 0);
    }

    private sealed record ProcessApprovalRemindersPayload(
        int CandidatesFound,
        int RemindersSentCount,
        int SkippedCount);
}
