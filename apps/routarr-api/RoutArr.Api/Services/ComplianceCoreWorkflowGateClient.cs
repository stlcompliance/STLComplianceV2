using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed record ComplianceCoreInternalWorkflowGateBatchCheckItem(
    string GateKey,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record ComplianceCoreInternalWorkflowGateBatchCheckRequest(
    Guid TenantId,
    IReadOnlyList<ComplianceCoreInternalWorkflowGateBatchCheckItem> Items,
    IReadOnlyDictionary<string, string>? Context = null,
    bool EmitFindings = false);

public sealed record ComplianceCoreWorkflowGateCheckResult(
    Guid CheckResultId,
    string GateKey,
    string GateLabel,
    string Outcome,
    string ReasonCode,
    string Message);

public sealed record ComplianceCoreWorkflowGateBatchCheckSummary(
    int Total,
    int AllowCount,
    int WarnCount,
    int BlockCount);

public sealed record ComplianceCoreWorkflowGateBatchCheckResponse(
    Guid BatchId,
    IReadOnlyList<ComplianceCoreWorkflowGateCheckResult> Results,
    ComplianceCoreWorkflowGateBatchCheckSummary Summary);

public sealed class ComplianceCoreWorkflowGateClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<ComplianceCoreWorkflowGateBatchCheckResponse?> CheckBatchAsync(
        Guid tenantId,
        IReadOnlyList<ComplianceCoreInternalWorkflowGateBatchCheckItem> items,
        IReadOnlyDictionary<string, string>? sharedContext,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/workflow-gate-check/batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(new ComplianceCoreInternalWorkflowGateBatchCheckRequest(
            tenantId,
            items,
            sharedContext,
            EmitFindings: false));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.workflow_gate_check_failed",
                $"Compliance Core workflow gate check failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreWorkflowGateBatchCheckResponse>(cancellationToken);
    }
}
