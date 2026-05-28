using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record ComplianceCoreProcessRuleChangeScanResult(
    int PacksScannedCount,
    int ChangesDetectedCount);

public sealed class ComplianceCoreRuleChangeMonitorClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreRuleChangeMonitorOptions> options)
{
    public async Task<ComplianceCoreProcessRuleChangeScanResult> ProcessScanAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/rule-changes/process-scan");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessRuleChangeScanPayload>(cancellationToken);
        return new ComplianceCoreProcessRuleChangeScanResult(
            payload?.PacksScannedCount ?? 0,
            payload?.ChangesDetectedCount ?? 0);
    }

    private sealed record ProcessRuleChangeScanPayload(
        int PacksScannedCount,
        int ChangesDetectedCount);
}
