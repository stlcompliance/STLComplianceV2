namespace TrainArr.Api.Contracts;

public static class TrainingCompletionRuleTypes
{
    public const string AllStepsRequired = "all_steps_required";
    public const string RequiredSignoff = "required_signoff";
    public const string RequiredEvaluatorPass = "required_evaluator_pass";
    public const string MinimumEvaluationScore = "minimum_evaluation_score";
}

public sealed record CreateTrainingDefinitionCompletionRuleRequest(
    string RuleKey,
    string RuleType,
    string Label,
    string ConfigJson,
    int SortOrder);

public sealed record UpdateTrainingDefinitionCompletionRuleRequest(
    string RuleType,
    string Label,
    string ConfigJson,
    int SortOrder);

public sealed record TrainingDefinitionCompletionRuleResponse(
    Guid CompletionRuleId,
    Guid TrainingDefinitionId,
    string RuleKey,
    string RuleType,
    string Label,
    string ConfigJson,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingCompletionRuleCatalogItemResponse(
    string RuleType,
    string Label,
    string Description,
    string DefaultConfigJson);
