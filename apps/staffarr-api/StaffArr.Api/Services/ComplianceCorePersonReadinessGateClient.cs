using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using StaffArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

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

public sealed class ComplianceCorePersonReadinessGateClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<ComplianceCoreProductGateResponse?> CheckPersonReadinessAsync(
        Guid tenantId,
        Guid personId,
        string personDisplayName,
        IReadOnlyDictionary<string, string> context,
        CancellationToken cancellationToken = default)
    {
        var currentOptions = options.Value;
        if (string.IsNullOrWhiteSpace(currentOptions.ServiceToken))
        {
            return null;
        }

        var actionKey = string.IsNullOrWhiteSpace(currentOptions.PersonReadinessActionKey)
            ? "can-use-person"
            : currentOptions.PersonReadinessActionKey.Trim().ToLowerInvariant();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/gates/{actionKey}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentOptions.ServiceToken);
        request.Content = JsonContent.Create(new ComplianceCoreProductGateCompatibilityRequest(
            tenantId,
            string.IsNullOrWhiteSpace(currentOptions.PersonReadinessActivityContextKey)
                ? "person_readiness"
                : currentOptions.PersonReadinessActivityContextKey.Trim().ToLowerInvariant(),
            [
                new ComplianceCoreProductGateSubjectReference(
                    "person",
                    personId.ToString("D"),
                    "staffarr",
                    string.IsNullOrWhiteSpace(personDisplayName) ? null : personDisplayName)
            ],
            context,
            EmitFindings: currentOptions.EmitPersonReadinessFindings,
            WorkflowKey: string.IsNullOrWhiteSpace(currentOptions.PersonReadinessWorkflowKey)
                ? null
                : currentOptions.PersonReadinessWorkflowKey.Trim().ToLowerInvariant()));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.person_readiness_gate_failed",
                $"Compliance Core person-readiness gate failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreProductGateResponse>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.person_readiness_gate_invalid_response",
                "Compliance Core person-readiness gate returned an empty response.",
                502);
    }
}
