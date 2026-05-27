namespace MaintainArr.Api.Contracts;

public sealed record InspectionRunSummaryResponse(
    Guid InspectionRunId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid InspectionTemplateId,
    string TemplateKey,
    string TemplateName,
    int TemplateVersion,
    string Status,
    string? Result,
    Guid StartedByUserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int AnswerCount,
    int RequiredItemCount);

public sealed record InspectionRunChecklistItemSnapshot(
    Guid ChecklistItemId,
    Guid? CategoryId,
    string? CategoryKey,
    string ItemKey,
    string Prompt,
    string ItemType,
    bool IsRequired,
    int SortOrder);

public sealed record InspectionRunAnswerResponse(
    Guid AnswerId,
    Guid ChecklistItemId,
    string ItemKey,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue,
    DateTimeOffset AnsweredAt,
    Guid AnsweredByUserId);

public sealed record InspectionRunDetailResponse(
    Guid InspectionRunId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid InspectionTemplateId,
    string TemplateKey,
    string TemplateName,
    int TemplateVersion,
    string Status,
    string? Result,
    Guid StartedByUserId,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<InspectionRunChecklistItemSnapshot> ChecklistItems,
    IReadOnlyList<InspectionRunAnswerResponse> Answers);

public sealed record StartInspectionRunRequest(
    Guid AssetId,
    Guid InspectionTemplateId);

public sealed record SubmitInspectionRunAnswersRequest(
    IReadOnlyList<InspectionRunAnswerInput> Answers);

public sealed record InspectionRunAnswerInput(
    Guid ChecklistItemId,
    string? PassFailValue,
    decimal? NumericValue,
    string? TextValue);
