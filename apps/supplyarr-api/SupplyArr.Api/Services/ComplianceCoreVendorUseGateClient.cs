using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

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

public sealed class ComplianceCoreVendorUseGateClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<ComplianceCoreProductGateResponse?> CheckVendorUseAsync(
        Guid tenantId,
        Guid vendorPartyId,
        string vendorDisplayName,
        IReadOnlyDictionary<string, string> context,
        CancellationToken cancellationToken = default)
    {
        var currentOptions = options.Value;
        var serviceToken = currentOptions.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            return null;
        }

        var actionKey = string.IsNullOrWhiteSpace(currentOptions.VendorUseActionKey)
            ? "can-use-vendor"
            : currentOptions.VendorUseActionKey.Trim().ToLowerInvariant();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/gates/{actionKey}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(new ComplianceCoreProductGateCompatibilityRequest(
            tenantId,
            string.IsNullOrWhiteSpace(currentOptions.VendorUseActivityContextKey)
                ? "purchase_order_issue"
                : currentOptions.VendorUseActivityContextKey.Trim().ToLowerInvariant(),
            [
                new ComplianceCoreProductGateSubjectReference(
                    "vendor",
                    vendorPartyId.ToString("D"),
                    "supplyarr",
                    string.IsNullOrWhiteSpace(vendorDisplayName) ? null : vendorDisplayName)
            ],
            context,
            EmitFindings: currentOptions.EmitVendorUseFindings,
            WorkflowKey: string.IsNullOrWhiteSpace(currentOptions.VendorUseWorkflowKey)
                ? null
                : currentOptions.VendorUseWorkflowKey.Trim().ToLowerInvariant()));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.vendor_use_gate_failed",
                $"Compliance Core vendor-use gate failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ComplianceCoreProductGateResponse>(cancellationToken)
            ?? throw new StlApiException(
                "compliancecore.vendor_use_gate_invalid_response",
                "Compliance Core vendor-use gate returned an empty response.",
                502);
    }
}
