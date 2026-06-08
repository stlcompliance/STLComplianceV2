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
    DateTimeOffset? ResolvedAt,
    int EvidenceCount,
    DowntimeFollowUpResponse? DowntimeFollowUp = null,
    string? Priority = null,
    string? DefectType = null,
    string? ReportSource = null,
    string? ReportedByPersonId = null,
    string? DiscoveredByPersonId = null,
    string? CreatedByPersonId = null,
    string? UpdatedByPersonId = null,
    DateTimeOffset? ReportedAt = null,
    DateTimeOffset? DiscoveredAt = null,
    bool? IsSafetyCritical = null,
    bool? IsComplianceImpacting = null,
    bool? IsOperabilityImpacting = null,
    string? FailureMode = null,
    string? SystemKey = null,
    string? ComponentKey = null,
    string? Symptom = null,
    string? SidePosition = null,
    string? OperatingCondition = null,
    string? DeferralCode = null,
    string? SourceType = null,
    string? SourceReferenceId = null,
    string? IncidentReferenceId = null);

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
    DateTimeOffset? ResolvedAt,
    int EvidenceCount,
    DowntimeFollowUpResponse? DowntimeFollowUp = null,
    string? Priority = null,
    string? DefectType = null,
    string? ReportSource = null,
    string? ReportedByPersonId = null,
    string? DiscoveredByPersonId = null,
    string? CreatedByPersonId = null,
    string? UpdatedByPersonId = null,
    DateTimeOffset? ReportedAt = null,
    DateTimeOffset? DiscoveredAt = null,
    bool? IsSafetyCritical = null,
    bool? IsComplianceImpacting = null,
    bool? IsOperabilityImpacting = null,
    string? FailureMode = null,
    string? SystemKey = null,
    string? ComponentKey = null,
    string? Symptom = null,
    string? SidePosition = null,
    string? OperatingCondition = null,
    string? DeferralCode = null,
    string? SourceType = null,
    string? SourceReferenceId = null,
    string? IncidentReferenceId = null,
    string? ReadinessNotes = null,
    string? CorrectiveAction = null);

public sealed record CreateDefectRequest(
    Guid AssetId,
    string Title,
    string Description,
    string Severity);

public sealed record DefectValidationFindingResponse(
    string Category,
    string Severity,
    string Code,
    string Message,
    string? FieldKey = null,
    string? SectionKey = null,
    string? Source = null);

public sealed record DefectValidationResponse(
    bool IsValid,
    IReadOnlyList<DefectValidationFindingResponse> Findings);

public sealed record DefectDuplicateMatchResponse(
    Guid DefectId,
    string Title,
    string Status,
    string Severity,
    string AssetTag,
    string AssetName,
    string MatchReason,
    int SimilarityScore);

public sealed record DefectDraftPreviewResponse(
    DefectDetailResponse Defect,
    IReadOnlyList<DefectValidationFindingResponse> Findings,
    IReadOnlyList<DefectDuplicateMatchResponse> DuplicateMatches,
    AssetReadinessResponse? AssetReadiness,
    bool CanSubmit,
    bool CanCreateWorkOrder,
    bool CanMarkAssetNotReady);

public sealed record UpsertDefectDraftRequest(
    Guid AssetId,
    string? Title,
    string? Description,
    string? Severity,
    string? Priority,
    string? DefectType,
    string? ReportSource,
    DateTimeOffset? ReportedAt,
    DateTimeOffset? DiscoveredAt,
    string? ReportedByPersonId,
    string? DiscoveredByPersonId,
    string? FailureMode,
    string? SystemKey,
    string? ComponentKey,
    string? Symptom,
    string? SidePosition,
    string? OperatingCondition,
    string? DeferralCode,
    bool? IsSafetyCritical,
    bool? IsComplianceImpacting,
    bool? IsOperabilityImpacting,
    string? ReadinessNotes,
    string? CorrectiveAction,
    string? SourceType,
    string? SourceReferenceId,
    string? IncidentReferenceId);

public sealed record SubmitDefectRequest(
    bool CreateWorkOrder,
    bool MarkAssetNotReady,
    string? WorkOrderTitle,
    string? WorkOrderDescription,
    string? WorkOrderPriority,
    string? WorkOrderAssignedTechnicianPersonId,
    string? WorkOrderDraftPlanJson,
    DateTimeOffset? WorkOrderPlannedStartAt,
    DateTimeOffset? WorkOrderPlannedDueAt,
    string? HoldType,
    string? HoldTitle,
    string? HoldDescription,
    string? HoldSeverity,
    string? HoldSourceProduct,
    string? HoldSourceObjectRef,
    string? HoldCreatedByPersonId);

public sealed record DefectSubmissionResponse(
    DefectDetailResponse Defect,
    WorkOrderDetailResponse? WorkOrder,
    AssetQualityHoldResponse? AssetQualityHold);

public sealed record CreateDefectsFromInspectionRunRequest(
    IReadOnlyList<Guid>? ChecklistItemIds);

public sealed record CreateDefectsFromInspectionRunResponse(
    Guid InspectionRunId,
    IReadOnlyList<DefectSummaryResponse> Created,
    IReadOnlyList<DefectSummaryResponse> Existing);

public sealed record UpdateDefectStatusRequest(string Status);
