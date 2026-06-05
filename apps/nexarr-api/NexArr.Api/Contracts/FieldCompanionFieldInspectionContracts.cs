namespace NexArr.Api.Contracts;

public sealed record FieldCompanionFieldInspectionChecklistItem(
    Guid ChecklistItemId,
    string ItemKey,
    string Prompt,
    string ItemType,
    bool IsRequired,
    int SortOrder);

public sealed record FieldCompanionFieldInspectionAnswer(
    Guid ChecklistItemId,
    string ItemKey,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    DateTimeOffset AnsweredAt);

public sealed record FieldCompanionFieldInspectionDetailResponse(
    string TaskKey,
    string ProductKey,
    Guid InspectionRunId,
    string AssetTag,
    string AssetName,
    string TemplateName,
    string Status,
    string? Result,
    IReadOnlyList<FieldCompanionFieldInspectionChecklistItem> ChecklistItems,
    IReadOnlyList<FieldCompanionFieldInspectionAnswer> Answers);

public sealed record FieldCompanionFieldInspectionAnswerInput(
    Guid ChecklistItemId,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue);

public sealed record SubmitFieldCompanionFieldInspectionAnswersRequest(
    string TaskKey,
    IReadOnlyList<FieldCompanionFieldInspectionAnswerInput> Answers);

public sealed record FieldCompanionFieldInspectionAnswersResponse(
    string TaskKey,
    string ProductKey,
    Guid InspectionRunId,
    string Status,
    int AnswerCount,
    int RequiredItemCount,
    IReadOnlyList<FieldCompanionFieldInspectionAnswer> Answers);

public sealed record CompleteFieldCompanionFieldInspectionRequest(string TaskKey);

public sealed record FieldCompanionFieldInspectionCompleteResponse(
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
