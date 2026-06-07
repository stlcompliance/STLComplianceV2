namespace MaintainArr.Api.Contracts;

public sealed record InspectionRunSummaryResponse(
    Guid InspectionRunId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid InspectionTemplateId,
    Guid? PmScheduleId,
    string TemplateKey,
    string TemplateName,
    string InspectionType,
    int TemplateVersion,
    string Status,
    string? Result,
    Guid StartedByUserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? StaffarrLocationId,
    int AnswerCount,
    int RequiredItemCount);

public sealed record InspectionRunChecklistItemSnapshot(
    Guid ChecklistItemId,
    Guid? CategoryId,
    string? CategoryKey,
    string ItemKey,
    string Prompt,
    string ItemType,
    IReadOnlyList<string> ControlledOptions,
    decimal? AcceptableRangeMin,
    decimal? AcceptableRangeMax,
    string? UnitOfMeasure,
    bool IsRequired,
    int SortOrder);

public sealed record InspectionRunAnswerResponse(
    Guid AnswerId,
    Guid ChecklistItemId,
    string ItemKey,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    IReadOnlyList<string> SelectedOptions,
    string? UnitOfMeasure,
    DateTimeOffset AnsweredAt,
    Guid AnsweredByUserId);

public sealed record InspectionRunPauseEventResponse(
    Guid PauseEventId,
    DateTimeOffset PausedAt,
    DateTimeOffset? ResumedAt,
    int? DurationMinutes,
    string? Reason,
    string? Notes,
    Guid PausedByUserId,
    Guid? ResumedByUserId);

public sealed record InspectionRunDetailResponse(
    Guid InspectionRunId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid InspectionTemplateId,
    Guid? PmScheduleId,
    string TemplateKey,
    string TemplateName,
    string InspectionType,
    int TemplateVersion,
    string Status,
    string? Result,
    Guid StartedByUserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt,
    string? SourceProduct,
    string? SourceObjectRef,
    string? StaffarrLocationId,
    int BreakDurationMinutes,
    bool LongDurationFlag,
    IReadOnlyList<Guid> GeneratedWorkOrderRefs,
    IReadOnlyList<InspectionRunChecklistItemSnapshot> ChecklistItems,
    IReadOnlyList<InspectionRunAnswerResponse> Answers,
    IReadOnlyList<InspectionRunPauseEventResponse> PauseEvents);

public sealed record StartInspectionRunRequest(
    Guid AssetId,
    Guid InspectionTemplateId,
    string? SourceProduct = null,
    string? SourceObjectRef = null);

public sealed record SubmitInspectionRunAnswersRequest(
    IReadOnlyList<InspectionRunAnswerInput> Answers);

public sealed record PauseInspectionRunRequest(
    string? Reason,
    string? Notes);

public sealed record ResumeInspectionRunRequest(
    string? Notes);

public sealed record InspectionRunAnswerInput(
    Guid ChecklistItemId,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    IReadOnlyList<string>? SelectedOptions = null);
