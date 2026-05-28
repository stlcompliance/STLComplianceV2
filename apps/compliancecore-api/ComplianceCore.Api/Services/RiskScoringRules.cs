using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class RiskScoringRules
{
    public const int MaxPacksPerEvaluate = 25;

    public const int MaxListLimit = 100;

    public static int ComputeScore(
        string outcome,
        string evaluationResult,
        int unresolvedFactCount,
        int failedRuleCount,
        int requiredFactCount,
        int mirrorFactCount)
    {
        var baseScore = outcome switch
        {
            ComplianceEvaluationOutcomes.Allow => 12,
            ComplianceEvaluationOutcomes.Warn => 42,
            _ => 68,
        };

        if (!string.Equals(evaluationResult, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
        {
            baseScore += 8;
        }

        baseScore += Math.Min(24, unresolvedFactCount * 8);
        baseScore += Math.Min(20, failedRuleCount * 10);

        if (requiredFactCount > 0 && mirrorFactCount == 0)
        {
            baseScore += 12;
        }

        return Math.Clamp(baseScore, 0, 100);
    }

    public static string BuildSummary(
        string packKey,
        int score,
        string level,
        string outcome,
        int unresolvedFactCount,
        int failedRuleCount,
        int mirrorFactCount) =>
        $"Risk {score} ({level}) for {packKey}: {outcome} with {unresolvedFactCount} unresolved fact(s), " +
        $"{failedRuleCount} failed rule(s), {mirrorFactCount} mirror fact(s) at scope.";
}
