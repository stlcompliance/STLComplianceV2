namespace MaintainArr.Api.Contracts;

public sealed record CreateWorkOrderCommentRequest(
    string Body,
    string? Visibility,
    bool? Pinned);

public sealed record WorkOrderCommentResponse(
    Guid CommentId,
    Guid WorkOrderId,
    string Body,
    string Visibility,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId,
    DateTimeOffset? EditedAt,
    string? EditedByPersonId,
    bool Pinned);

public sealed record WorkOrderTimelineEventResponse(
    Guid TimelineEventId,
    Guid WorkOrderId,
    string EventType,
    DateTimeOffset OccurredAt,
    string? ActorPersonId,
    string? ActorServiceClientId,
    string Summary,
    string? SourceProduct,
    string? SourceObjectRef,
    string? BeforeSnapshot,
    string? AfterSnapshot);
