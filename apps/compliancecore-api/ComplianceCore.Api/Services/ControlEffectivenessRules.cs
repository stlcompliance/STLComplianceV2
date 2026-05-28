using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class ControlEffectivenessRules
{
    public const int MaxPacksPerEvaluate = 25;

    public const int MaxListLimit = 100;

    public static int ComputeScore(
        string outcome,
        string evaluationResult,
        int unresolvedFactCount,
        int failedRuleCount,
        int passedRuleCount,
        int totalRuleCount)
    {
        var baseScore = outcome switch
        {
            ComplianceEvaluationOutcomes.Allow => 88,
            ComplianceEvaluationOutcomes.Warn => 58,
            _ => 28,
        };

        if (!string.Equals(evaluationResult, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
        {
            baseScore -= 14;
        }

        baseScore -= Math.Min(28, unresolvedFactCount * 9);
        baseScore -= Math.Min(24, failedRuleCount * 11);

        if (totalRuleCount > 0)
        {
            var passRate = (double)passedRuleCount / totalRuleCount;
            baseScore += (int)Math.Round(passRate * 12);
        }

        return Math.Clamp(baseScore, 0, 100);
    }

    public static string BuildSummary(
        string packKey,
        int score,
        string level,
        string controlStatus,
        string outcome,
        int passedRuleCount,
        int totalRuleCount,
        int unresolvedFactCount) =>
        $"Control {packKey} effectiveness {score} ({level}, {controlStatus}): {outcome} with " +
        $"{passedRuleCount}/{totalRuleCount} rule(s) passing and {unresolvedFactCount} unresolved fact(s).";
}
