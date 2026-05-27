namespace MaintainArr.Api.Contracts;

public sealed record DefectSummaryResponse(
    Guid DefectId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? InspectionRunId,
    Guid? ChecklistItemId,
    string? ChecklistItemKey,
    string Title,
    string Severity,
    string Status,
    string Source,
    Guid ReportedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt);

public sealed record DefectDetailResponse(
    Guid DefectId,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? InspectionRunId,
    Guid? ChecklistItemId,
    string? ChecklistItemKey,
    string? ChecklistItemPrompt,
    string Title,
    string Description,
    string Severity,
    string Status,
    string Source,
    Guid ReportedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt);

public sealed record CreateDefectRequest(
    Guid AssetId,
    string Title,
    string Description,
    string Severity);

public sealed record CreateDefectsFromInspectionRunRequest(
    IReadOnlyList<Guid>? ChecklistItemIds);

public sealed record CreateDefectsFromInspectionRunResponse(
    Guid InspectionRunId,
    IReadOnlyList<DefectSummaryResponse> Created,
    IReadOnlyList<DefectSummaryResponse> Existing);

public sealed record UpdateDefectStatusRequest(string Status);
