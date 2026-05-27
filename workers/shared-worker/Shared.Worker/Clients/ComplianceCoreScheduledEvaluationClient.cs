using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Worker.Options;

namespace Shared.Worker.Clients;

public sealed record ComplianceCoreProcessScheduledEvaluationsResult(
    int PacksDueCount,
    int EvaluatedCount,
    int SkippedCount,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed class ComplianceCoreScheduledEvaluationClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreScheduledEvaluationOptions> options)
{
    public async Task<ComplianceCoreProcessScheduledEvaluationsResult> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/scheduled-evaluations/process-batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ServiceToken);
        request.Content = JsonContent.Create(new
        {
            tenantId = settings.TenantId,
            asOfUtc = (DateTimeOffset?)null,
            batchSize = settings.BatchSize,
            intervalHours = settings.IntervalHours,
            emitFindings = settings.EmitFindings,
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ProcessScheduledEvaluationsPayload>(cancellationToken);
        return new ComplianceCoreProcessScheduledEvaluationsResult(
            payload?.PacksDueCount ?? 0,
            payload?.EvaluatedCount ?? 0,
            payload?.SkippedCount ?? 0,
            payload?.AllowCount ?? 0,
            payload?.WarnCount ?? 0,
            payload?.BlockCount ?? 0);
    }

    private sealed record ProcessScheduledEvaluationsPayload(
        int PacksDueCount,
        int EvaluatedCount,
        int SkippedCount,
        int AllowCount,
        int WarnCount,
        int BlockCount);
}
