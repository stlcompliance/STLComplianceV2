namespace StaffArr.Api.Contracts;

public sealed record TrainarrPersonTrainingHistoryEntryItem(
    Guid EntryId,
    string EventKind,
    string Summary,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    DateTimeOffset OccurredAt);

public sealed record TrainarrPersonTrainingHistoryResponse(
    Guid PersonId,
    string SourceProduct,
    string SourceNote,
    int TotalCount,
    IReadOnlyList<TrainarrPersonTrainingHistoryEntryItem> Items);
