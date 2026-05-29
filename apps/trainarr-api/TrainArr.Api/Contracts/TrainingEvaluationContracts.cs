namespace TrainArr.Api.Contracts;

public sealed record SubmitTrainingEvaluationRequest(
    Guid TrainingAssignmentId,
    string Result,
    decimal? Score,
    string? Notes);

public sealed record TrainingEvaluationResponse(
    Guid EvaluationId,
    Guid TrainingAssignmentId,
    string Result,
    decimal? Score,
    string? Notes,
    Guid EvaluatorUserId,
    DateTimeOffset EvaluatedAt);

public sealed record TrainingEvaluationHistoryItemResponse(
    Guid EntryId,
    Guid TrainingAssignmentId,
    string Result,
    decimal? Score,
    string? Notes,
    Guid EvaluatorUserId,
    DateTimeOffset EvaluatedAt,
    bool IsCurrent,
    DateTimeOffset? SupersededAt);

public sealed record TrainingEvaluationHistoryResponse(
    Guid TrainingAssignmentId,
    IReadOnlyList<TrainingEvaluationHistoryItemResponse> Items);

public sealed record TrainingEvaluationReviewItemResponse(
    Guid EvaluationId,
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    string TrainingDefinitionName,
    string QualificationName,
    string AssignmentStatus,
    string Result,
    decimal? Score,
    string? Notes,
    Guid EvaluatorUserId,
    DateTimeOffset EvaluatedAt);

public sealed record TrainingEvaluationReviewTimelineResponse(
    IReadOnlyList<TrainingEvaluationReviewItemResponse> Items);
