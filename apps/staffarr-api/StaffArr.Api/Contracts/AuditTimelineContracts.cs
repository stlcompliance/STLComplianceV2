namespace StaffArr.Api.Contracts;

public sealed record AuditTimelineFilter(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Action = null,
    string? Result = null,
    string? TargetType = null,
    Guid? ActorUserId = null);

public sealed record StaffArrAuditEventExportItem(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);
