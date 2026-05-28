namespace SupplyArr.Api.Contracts;

public sealed record AuditHistoryItemResponse(
    Guid Id,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record AuditHistoryListResponse(
    IReadOnlyList<AuditHistoryItemResponse> Items,
    string? NextCursor,
    bool HasMore);
