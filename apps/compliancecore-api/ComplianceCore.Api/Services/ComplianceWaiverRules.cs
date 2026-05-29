using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class ComplianceWaiverRules
{
    public const int MaxBatchSize = 200;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, MaxBatchSize);

    public static string NormalizeWaiverKey(string waiverKey)
    {
        var normalized = waiverKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 64)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "waivers.validation",
                "Waiver key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeScopeKey(string scopeKey) =>
        ProductFactMirrorRules.NormalizeScopeKey(scopeKey);

    public static string? NormalizeOptionalRuleKey(string? ruleKey)
    {
        if (string.IsNullOrWhiteSpace(ruleKey))
        {
            return null;
        }

        return ruleKey.Trim().ToLowerInvariant();
    }

    public static string? NormalizeOptionalGateKey(string? gateKey)
    {
        if (string.IsNullOrWhiteSpace(gateKey))
        {
            return null;
        }

        return gateKey.Trim().ToLowerInvariant();
    }

    public static bool IsExpirableStatus(string status) =>
        string.Equals(status, WaiverStatuses.Approved, StringComparison.OrdinalIgnoreCase);

    public static bool ShouldExpireForBatch(
        string status,
        DateTimeOffset? expiresAt,
        DateTimeOffset asOfUtc)
    {
        if (!IsExpirableStatus(status))
        {
            return false;
        }

        return expiresAt is not null && expiresAt <= asOfUtc;
    }

    public static bool IsActiveAt(ComplianceWaiver waiver, DateTimeOffset asOf) =>
        string.Equals(waiver.Status, WaiverStatuses.Approved, StringComparison.OrdinalIgnoreCase)
        && waiver.EffectiveAt <= asOf
        && (!waiver.ExpiresAt.HasValue || waiver.ExpiresAt.Value > asOf);

    public static bool MatchesEvaluationScope(
        ComplianceWaiver waiver,
        Guid rulePackId,
        string? ruleKey,
        string? gateKey,
        string scopeKey)
    {
        if (waiver.RulePackId != rulePackId)
        {
            return false;
        }

        if (!string.Equals(waiver.SubjectScopeKey, scopeKey, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(waiver.GateKey)
            && !string.Equals(waiver.GateKey, gateKey, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(waiver.RuleKey))
        {
            if (string.IsNullOrWhiteSpace(ruleKey))
            {
                return false;
            }

            return string.Equals(waiver.RuleKey, ruleKey, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public static bool HasNonWaivableRuleFailure(IReadOnlyList<RuleEvaluationItemResponse> ruleResults) =>
        ruleResults.Any(item =>
            !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase)
            && item.NonWaivable);

    public static bool IsRuleNonWaivable(RulePackContentBody content, string ruleKey)
    {
        var normalizedRuleKey = ruleKey.Trim().ToLowerInvariant();
        var rule = content.Rules.FirstOrDefault(item =>
            string.Equals(item.RuleKey.Trim(), normalizedRuleKey, StringComparison.OrdinalIgnoreCase));

        return rule?.NonWaivable ?? false;
    }
}
