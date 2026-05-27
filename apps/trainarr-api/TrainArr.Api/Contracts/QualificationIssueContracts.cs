namespace TrainArr.Api.Contracts;

public sealed record QualificationIssueResponse(
    Guid QualificationIssueId,
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    Guid GrantPublicationId,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? StatusChangedAt,
    string? LifecycleReason,
    Guid? LifecyclePublicationId);

public sealed record QualificationLifecycleActionRequest(
    string? Reason);

public sealed record PublishQualificationGrantRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid TrainingAssignmentId,
    string QualificationKey,
    string QualificationName,
    string TrainingDefinitionName,
    DateTimeOffset? ExpiresAt,
    string? Notes);

public sealed record PublishQualificationLifecycleRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid GrantPublicationId,
    string QualificationKey,
    string QualificationName,
    string LifecycleAction,
    string Message);
