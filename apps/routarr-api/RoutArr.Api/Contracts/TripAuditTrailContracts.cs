namespace RoutArr.Api.Contracts;

public sealed record TripAuditTrailEntryResponse(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record TripAuditTrailResponse(
    Guid TripId,
    IReadOnlyList<TripAuditTrailEntryResponse> Entries);
