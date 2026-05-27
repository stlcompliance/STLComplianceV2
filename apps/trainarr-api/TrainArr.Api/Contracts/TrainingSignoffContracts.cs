namespace TrainArr.Api.Contracts;

public sealed record SubmitTrainingSignoffRequest(
    Guid TrainingAssignmentId,
    string SignoffRole,
    string? Notes);

public sealed record TrainingSignoffResponse(
    Guid SignoffId,
    Guid TrainingAssignmentId,
    string SignoffRole,
    Guid SignedByUserId,
    string? Notes,
    DateTimeOffset SignedAt);
