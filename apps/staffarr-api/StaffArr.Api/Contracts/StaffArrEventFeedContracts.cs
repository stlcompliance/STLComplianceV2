namespace StaffArr.Api.Contracts;

public sealed record StaffArrEventFeedResponse(
    DateTimeOffset AsOfUtc,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasMore,
    IReadOnlyList<StaffArrEventFeedItem> Items);

public sealed record StaffArrEventFeedItem(
    Guid EventId,
    Guid TenantId,
    string EventKind,
    string SourceAction,
    string TargetType,
    string? TargetId,
    Guid? ActorUserId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);
