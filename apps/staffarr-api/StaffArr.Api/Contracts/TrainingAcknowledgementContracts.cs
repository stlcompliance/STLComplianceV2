namespace StaffArr.Api.Contracts;

public sealed record TrainingAcknowledgementResponse(
    Guid AcknowledgementId,
    Guid PersonId,
    Guid TrainarrAcknowledgementRequestId,
    Guid TrainarrAssignmentId,
    string TrainingTitle,
    string AssignmentReason,
    string Summary,
    string Status,
    DateTimeOffset? DueAt,
    DateTimeOffset RequestedAt,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record IngestTrainingAcknowledgementRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrAcknowledgementRequestId,
    Guid TrainarrAssignmentId,
    string TrainingTitle,
    string AssignmentReason,
    string Summary,
    DateTimeOffset? DueAt);

public sealed record TrainingAcknowledgementIngestionResponse(
    Guid AcknowledgementId,
    Guid TrainarrAcknowledgementRequestId,
    string Status);

public sealed record SupersedeTrainingAcknowledgementRequest(
    Guid TenantId,
    Guid PersonId,
    Guid TrainarrAcknowledgementRequestId);

public sealed record TrainingAcknowledgementStatusResponse(
    Guid TrainarrAcknowledgementRequestId,
    Guid TrainarrAssignmentId,
    Guid PersonId,
    string Status,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedByUserId);
