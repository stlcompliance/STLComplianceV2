using System.Text.Json;
using TrainArr.Api.Contracts;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed record CompletionEvaluationResult(
    bool AreMet,
    IReadOnlyList<string> MissingRequirements);

public static class TrainingCompletionRuleEvaluator
{
    public static CompletionEvaluationResult Evaluate(
        TrainingAssignment assignment,
        IReadOnlyList<TrainingDefinitionCompletionRule> rules,
        IReadOnlyList<TrainingAssignmentStepProgress>? stepProgress = null)
    {
        var effectiveRules = rules.Count > 0
            ? rules.OrderBy(x => x.SortOrder).ToList()
            : BuildDefaultRules();

        var missing = new List<string>();
        foreach (var rule in effectiveRules)
        {
            if (!IsRuleMet(assignment, rule, stepProgress))
            {
                missing.Add(rule.RuleKey);
            }
        }

        return new CompletionEvaluationResult(missing.Count == 0, missing);
    }

    public static IReadOnlyList<TrainingDefinitionCompletionRule> BuildDefaultRules() =>
    [
        new()
        {
            RuleKey = "default_evaluator_pass",
            RuleType = TrainingCompletionRuleTypes.RequiredEvaluatorPass,
            Label = "Passing evaluator result",
            ConfigJson = "{}",
            SortOrder = 0,
        },
        new()
        {
            RuleKey = "default_trainee_signoff",
            RuleType = TrainingCompletionRuleTypes.RequiredSignoff,
            Label = "Trainee signoff",
            ConfigJson = """{"signoffRole":"trainee"}""",
            SortOrder = 1,
        },
        new()
        {
            RuleKey = "default_trainer_signoff",
            RuleType = TrainingCompletionRuleTypes.RequiredSignoff,
            Label = "Trainer signoff",
            ConfigJson = """{"signoffRole":"trainer"}""",
            SortOrder = 2,
        },
    ];

    private static bool IsRuleMet(
        TrainingAssignment assignment,
        TrainingDefinitionCompletionRule rule,
        IReadOnlyList<TrainingAssignmentStepProgress>? stepProgress)
    {
        return rule.RuleType switch
        {
            TrainingCompletionRuleTypes.AllStepsRequired => AreAllStepsCompleted(stepProgress),
            TrainingCompletionRuleTypes.RequiredSignoff => HasConfiguredSignoff(assignment, rule.ConfigJson),
            TrainingCompletionRuleTypes.RequiredEvaluatorPass => HasPassingEvaluation(assignment),
            TrainingCompletionRuleTypes.MinimumEvaluationScore => HasMinimumEvaluationScore(assignment, rule.ConfigJson),
            _ => false,
        };
    }

    private static bool AreAllStepsCompleted(IReadOnlyList<TrainingAssignmentStepProgress>? stepProgress)
    {
        if (stepProgress is null || stepProgress.Count == 0)
        {
            return true;
        }

        return stepProgress.All(x =>
            string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasConfiguredSignoff(TrainingAssignment assignment, string configJson)
    {
        var signoffRole = ReadSignoffRole(configJson);
        return assignment.Signoffs.Any(x =>
            string.Equals(x.SignoffRole, signoffRole, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasPassingEvaluation(TrainingAssignment assignment) =>
        assignment.Evaluation is not null
        && string.Equals(assignment.Evaluation.Result, "pass", StringComparison.OrdinalIgnoreCase);

    private static bool HasMinimumEvaluationScore(TrainingAssignment assignment, string configJson)
    {
        if (assignment.Evaluation is null)
        {
            return false;
        }

        var minimumScore = ReadMinimumScorePercent(configJson);
        return assignment.Evaluation.Score is decimal score
            && score >= minimumScore;
    }

    private static string ReadSignoffRole(string configJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson);
        if (document.RootElement.TryGetProperty("signoffRole", out var roleElement)
            && roleElement.ValueKind == JsonValueKind.String)
        {
            var role = roleElement.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(role))
            {
                return role;
            }
        }

        return "trainee";
    }

    private static decimal ReadMinimumScorePercent(string configJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson);
        if (document.RootElement.TryGetProperty("minimumScorePercent", out var scoreElement)
            && scoreElement.TryGetDecimal(out var score))
        {
            return score;
        }

        return 80m;
    }
}
