namespace TrainArr.Api.Contracts;

public static class TrainingStepTypes
{
    public const string Content = "content";
    public const string Quiz = "quiz";
    public const string Practical = "practical";
}

public sealed record CreateTrainingDefinitionStepRequest(
    string StepKey,
    string Name,
    string Description,
    string StepType,
    string ConfigJson,
    int SortOrder);

public sealed record UpdateTrainingDefinitionStepRequest(
    string Name,
    string Description,
    string StepType,
    string ConfigJson,
    int SortOrder);

public sealed record TrainingDefinitionStepResponse(
    Guid StepId,
    Guid TrainingDefinitionId,
    string StepKey,
    string Name,
    string Description,
    string StepType,
    string ConfigJson,
    int SortOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainingAssignmentStepProgressResponse(
    Guid ProgressId,
    Guid TrainingAssignmentId,
    Guid StepId,
    string StepKey,
    string Name,
    string Description,
    string StepType,
    string ConfigJson,
    int SortOrder,
    string Status,
    int? QuizScorePercent,
    string? ResponseJson,
    DateTimeOffset? CompletedAt);

public sealed record SubmitTrainingAssignmentStepRequest(
    IReadOnlyList<int>? SelectedOptionIndexes,
    string? PracticalResult,
    string? Notes);
