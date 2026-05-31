using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TrainArr.Api.Contracts;
using TrainArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed record ComplianceCoreInternalEvaluatePayload(
    Guid TenantId,
    string RulePackKey,
    IReadOnlyDictionary<string, string>? Context);

public sealed record ComplianceCoreInternalEvaluateResult(
    Guid TenantId,
    Guid RulePackId,
    string RulePackKey,
    string Outcome,
    string ReasonCode,
    string Message,
    string EvaluationResult,
    IReadOnlyList<string> UnresolvedFactKeys,
    Guid? AppliedWaiverId = null,
    string? AppliedWaiverKey = null);

public sealed record ComplianceCoreInternalEvaluateBatchItem(
    string RulePackKey,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record ComplianceCoreInternalEvaluateBatchPayload(
    Guid TenantId,
    IReadOnlyList<ComplianceCoreInternalEvaluateBatchItem> Items,
    IReadOnlyDictionary<string, string>? Context = null);

public sealed record ComplianceCoreInternalEvaluateBatchResult(
    Guid BatchId,
    IReadOnlyList<ComplianceCoreInternalEvaluateResult> Results);

public sealed class ComplianceCoreRuleEvaluationClient(
    HttpClient httpClient,
    IOptions<ComplianceCoreClientOptions> options)
{
    public async Task<ComplianceCoreInternalEvaluateResult> EvaluateRulePackAsync(
        ComplianceCoreInternalEvaluatePayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "compliancecore.service_token_missing",
                "TrainArr Compliance Core service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/evaluate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.rule_evaluation_failed",
                $"Compliance Core rule evaluation failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ComplianceCoreEvaluateResponse>(cancellationToken);
        if (result is null)
        {
            throw new StlApiException(
                "compliancecore.rule_evaluation_failed",
                "Compliance Core returned an empty evaluation response.",
                502);
        }

        return new ComplianceCoreInternalEvaluateResult(
            result.TenantId,
            result.RulePackId,
            result.RulePackKey,
            result.Outcome,
            result.ReasonCode,
            result.Message,
            result.EvaluationResult,
            result.UnresolvedFactKeys,
            result.AppliedWaiverId,
            result.AppliedWaiverKey);
    }

    public async Task<ComplianceCoreInternalEvaluateBatchResult> EvaluateRulePackBatchAsync(
        ComplianceCoreInternalEvaluateBatchPayload payload,
        CancellationToken cancellationToken = default)
    {
        var serviceToken = options.Value.ServiceToken;
        if (string.IsNullOrWhiteSpace(serviceToken))
        {
            throw new StlApiException(
                "compliancecore.service_token_missing",
                "TrainArr Compliance Core service token is not configured.",
                500);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/evaluate/batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new StlApiException(
                "compliancecore.rule_evaluation_failed",
                $"Compliance Core batch rule evaluation failed ({(int)response.StatusCode}): {body}",
                (int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<ComplianceCoreEvaluateBatchResponse>(cancellationToken);
        if (result is null)
        {
            throw new StlApiException(
                "compliancecore.rule_evaluation_failed",
                "Compliance Core returned an empty batch evaluation response.",
                502);
        }

        var evaluations = result.Results
            .Select(item => new ComplianceCoreInternalEvaluateResult(
                item.TenantId,
                item.RulePackId,
                item.RulePackKey,
                item.Outcome,
                item.ReasonCode,
                item.Message,
                item.EvaluationResult,
                item.UnresolvedFactKeys,
                item.AppliedWaiverId,
                item.AppliedWaiverKey))
            .ToList();

        return new ComplianceCoreInternalEvaluateBatchResult(result.BatchId, evaluations);
    }

    private sealed record ComplianceCoreEvaluateResponse(
        Guid TenantId,
        Guid RulePackId,
        string RulePackKey,
        string Outcome,
        string ReasonCode,
        string Message,
        string EvaluationResult,
        IReadOnlyList<string> UnresolvedFactKeys,
        Guid? AppliedWaiverId = null,
        string? AppliedWaiverKey = null);

    private sealed record ComplianceCoreEvaluateBatchResponse(
        Guid BatchId,
        IReadOnlyList<ComplianceCoreEvaluateResponse> Results);
}
