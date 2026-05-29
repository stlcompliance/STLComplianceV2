using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public static class RuleEvaluationOutcomeMapper
{
    public static (string Outcome, string ReasonCode, string Message, IReadOnlyList<WorkflowGateReasonResponse> Reasons)
        Map(
            string evaluationResult,
            IReadOnlyList<string> unresolvedFactKeys,
            IReadOnlyList<RuleEvaluationItemResponse> ruleResults)
    {
        var reasons = new List<WorkflowGateReasonResponse>();

        foreach (var factKey in unresolvedFactKeys)
        {
            reasons.Add(new WorkflowGateReasonResponse(
                "fact_unresolved",
                $"Required fact '{factKey}' could not be resolved.",
                null,
                factKey));
        }

        foreach (var rule in ruleResults.Where(item =>
                     !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add(new WorkflowGateReasonResponse(
                rule.NonWaivable ? "non_waivable_rule_failed" : "rule_failed",
                rule.Message,
                rule.RuleKey,
                null));
        }

        if (unresolvedFactKeys.Count > 0)
        {
            return (
                ComplianceEvaluationOutcomes.Warn,
                "facts_unresolved",
                $"One or more required facts could not be resolved: {string.Join(", ", unresolvedFactKeys)}.",
                reasons);
        }

        if (string.Equals(evaluationResult, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
        {
            return (
                ComplianceEvaluationOutcomes.Allow,
                "rule_evaluation_passed",
                "All rule checks passed for the supplied facts.",
                reasons);
        }

        var failedRules = ruleResults
            .Where(item => !string.Equals(item.Result, RuleEvaluationResults.Pass, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.RuleKey)
            .ToList();

        var message = failedRules.Count == 0
            ? "Rule evaluation did not pass."
            : $"Rule evaluation failed for: {string.Join(", ", failedRules)}.";

        return (
            ComplianceEvaluationOutcomes.Block,
            "rule_evaluation_failed",
            message,
            reasons);
    }
}
