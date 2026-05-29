namespace TrainArr.Api.Contracts;

public sealed record QualificationIssueListItemResponse(
    Guid QualificationIssueId,
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? StatusChangedAt,
    string? LifecycleReason);
