namespace MaintainArr.Api.Contracts;

public sealed record MaintenanceHistoryEntryResponse(
    string EntryId,
    Guid AssetId,
    string Category,
    string EventType,
    string Title,
    string? Detail,
    DateTimeOffset OccurredAt,
    Guid? ActorUserId,
    string SourceEntityType,
    string SourceEntityId,
    string? RelatedEntityId);
