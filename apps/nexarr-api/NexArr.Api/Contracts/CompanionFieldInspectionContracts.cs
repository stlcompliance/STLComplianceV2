namespace NexArr.Api.Contracts;

public sealed record CompanionFieldInspectionChecklistItem(
    Guid ChecklistItemId,
    string ItemKey,
    string Prompt,
    string ItemType,
    bool IsRequired,
    int SortOrder);

public sealed record CompanionFieldInspectionAnswer(
    Guid ChecklistItemId,
    string ItemKey,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    DateTimeOffset AnsweredAt);

public sealed record CompanionFieldInspectionDetailResponse(
    string TaskKey,
    string ProductKey,
    Guid InspectionRunId,
    string AssetTag,
    string AssetName,
    string TemplateName,
    string Status,
    string? Result,
    IReadOnlyList<CompanionFieldInspectionChecklistItem> ChecklistItems,
    IReadOnlyList<CompanionFieldInspectionAnswer> Answers);

public sealed record CompanionFieldInspectionAnswerInput(
    Guid ChecklistItemId,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue);

public sealed record SubmitCompanionFieldInspectionAnswersRequest(
    string TaskKey,
    IReadOnlyList<CompanionFieldInspectionAnswerInput> Answers);

public sealed record CompanionFieldInspectionAnswersResponse(
    string TaskKey,
    string ProductKey,
    Guid InspectionRunId,
    string Status,
    int AnswerCount,
    int RequiredItemCount,
    IReadOnlyList<CompanionFieldInspectionAnswer> Answers);

public sealed record CompleteCompanionFieldInspectionRequest(string TaskKey);

public sealed record CompanionFieldInspectionCompleteResponse(
    string TaskKey,
    string ProductKey,
    Guid InspectionRunId,
    string Status,
    string Result,
    DateTimeOffset CompletedAt);

public sealed record MaintainArrInspectionChecklistItemUpstream(
    Guid ChecklistItemId,
    string ItemKey,
    string Prompt,
    string ItemType,
    bool IsRequired,
    int SortOrder);

public sealed record MaintainArrInspectionAnswerUpstreamResponse(
    Guid ChecklistItemId,
    string ItemKey,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    DateTimeOffset AnsweredAt);

public sealed record MaintainArrInspectionRunUpstreamResponse(
    Guid InspectionRunId,
    string AssetTag,
    string AssetName,
    string TemplateName,
    string Status,
    string? Result,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<MaintainArrInspectionChecklistItemUpstream> ChecklistItems,
    IReadOnlyList<MaintainArrInspectionAnswerUpstreamResponse> Answers);
