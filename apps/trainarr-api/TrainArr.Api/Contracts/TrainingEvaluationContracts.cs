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
