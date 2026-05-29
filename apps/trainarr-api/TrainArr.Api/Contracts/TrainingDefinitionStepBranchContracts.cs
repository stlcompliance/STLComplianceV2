namespace TrainArr.Api.Contracts;

public static class TrainingStepBranchTypes
{
    public const string QuizFailedRemediation = "quiz_failed_remediation";
    public const string StepVisibility = "step_visibility";
}

public sealed record CreateTrainingDefinitionStepBranchRequest(
    string BranchKey,
    string BranchType,
    string Label,
    string ConfigJson,
    int SortOrder);

public sealed record UpdateTrainingDefinitionStepBranchRequest(
    string BranchType,
    string Label,
    string ConfigJson,
    int SortOrder);

public sealed record TrainingDefinitionStepBranchResponse(
    Guid BranchId,
    Guid TrainingDefinitionStepId,
    string BranchKey,
    string BranchType,
    string Label,
    string ConfigJson,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingStepBranchCatalogItemResponse(
    string BranchType,
    string Label,
    string Description,
    string DefaultConfigJson);
