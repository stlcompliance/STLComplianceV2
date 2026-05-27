namespace StaffArr.Api.Contracts;

public sealed record PersonTimelineEntryResponse(
    string EntryId,
    Guid PersonId,
    string Category,
    string EventType,
    string Title,
    string? Detail,
    DateTimeOffset OccurredAt,
    Guid? ActorUserId,
    string SourceEntityType,
    string SourceEntityId,
    string? ExternalReferenceId);
