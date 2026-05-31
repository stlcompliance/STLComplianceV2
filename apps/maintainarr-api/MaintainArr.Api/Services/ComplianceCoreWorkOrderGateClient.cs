using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed record ComplianceCoreProductGateSubjectReference(
    string SubjectType,
    string SubjectReference,
    string? SourceProduct = null,
    string? DisplayLabel = null);

public sealed record ComplianceCoreProductGateCompatibilityRequest(
    Guid TenantId,
    string ActivityContextKey,
    IReadOnlyList<ComplianceCoreProductGateSubjectReference> SubjectReferences,
    IReadOnlyDictionary<string, string>? RuleContext = null,
    IReadOnlyList<object>? FactSnapshotReferences = null,
    bool EmitFindings = false,
    string? WorkflowKey = null);

public sealed record ComplianceCoreProductGateResponse(
    Guid TraceId,
    Guid TenantId,
    string WorkflowKey,
    string ActionKey,
    string ActivityContextKey,
    Guid CheckResultId,
    string Outcome,
    string ReasonCode,
    string Message,
    Guid? AppliedWaiverId,
    string? AppliedWaiverKey,
    DateTimeOffset EvaluatedAt);

public sealed class ComplianceCoreWorkOrderGateClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<ComplianceCoreProductGateResponse?> CheckWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        Guid assetId,
        string assetLabel,
        IReadOnlyDictionary<string, string> context,
        CancellationToken cancellationToken = default)
    {
        var currentOptions = options.Value;
        if (string.IsNullOrWhiteSpace(currentOptions.ServiceToken))
        {
            return null;
        }

        var actionKey = string.IsNullOrWhiteSpace(currentOptions.WorkOrderActionKey)
            ? "can-perform-maintenance"
            : currentOptions.WorkOrderActionKey.Trim().ToLowerInvariant();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/gates/{actionKey}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentOptions.ServiceToken);
        request.Content = JsonContent.Create(new ComplianceCoreProductGateCompatibilityRequest(
            tenantId,
            string.IsNullOrWhiteSpace(currentOptions.WorkOrderActivityContextKey)
                ? "maintenance_work_order"
                : currentOptions.WorkOrderActivityContextKey.Trim().ToLowerInvariant(),
            [
                new("work_order", workOrderId.ToString("D"), "maintainarr"),
                new("asset", assetId.ToString("D"), "maintainarr", string.IsNullOrWhiteSpace(assetLabel) ? null : assetLabel)
            ],
            context,
            EmitFindings: currentOptions.EmitWorkOrderFindings,
            WorkflowKey: string.IsNullOrWhiteSpace(currentOptions.WorkOrderWorkflowKey)
                ? null
                : currentOptions.WorkOrderWorkflowKey.Trim().ToLowerInvariant()));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.work_order_gate_failed",
                $"Compliance Core work-order gate failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreProductGateResponse>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.work_order_gate_invalid_response",
                "Compliance Core work-order gate returned an empty response.",
                502);
    }
}
