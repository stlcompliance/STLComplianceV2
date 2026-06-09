namespace MaintainArr.Api.Contracts;

public sealed record WorkOrderSummaryResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? DefectId,
    Guid? PmScheduleId,
    string? TemplateRef,
    string Title,
    string Priority,
    string Status,
    string Source,
    string? SourceProduct,
    string? SourceObjectRef,
    string WorkOrderType,
    string OriginType,
    string? OriginRef,
    Guid? StaffarrSiteId,
    string? StaffarrLocationId,
    string? AssignedTechnicianPersonId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt);

public sealed record WorkOrderDetailResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    Guid? DefectId,
    string? DefectTitle,
    Guid? PmScheduleId,
    string? PmScheduleName,
    string? TemplateRef,
    string Title,
    string Description,
    string Priority,
    string Status,
    string Source,
    string? SourceProduct,
    string? SourceObjectRef,
    string WorkOrderType,
    string OriginType,
    string? OriginRef,
    Guid? StaffarrSiteId,
    string? StaffarrLocationId,
    IReadOnlyList<string> AssignedTechnicianPersonIds,
    string? AssignedSupervisorPersonId,
    IReadOnlyList<string> RequiredQualificationRefs,
    IReadOnlyList<WorkOrderQualificationCheckResultResponse> QualificationCheckResults,
    IReadOnlyList<WorkOrderTechnicianAssignmentResponse> TechnicianAssignments,
    IReadOnlyList<MaintenancePermitRefResponse> PermitRefs,
    ReturnToServiceResponse? ReturnToService,
    IReadOnlyList<Guid> VendorWorkRefs,
    string? AssignedTechnicianPersonId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    string? DraftPlanJson = null,
    DateTimeOffset? PlannedStartAt = null,
    DateTimeOffset? PlannedDueAt = null,
    DowntimeFollowUpResponse? DowntimeFollowUp = null,
    IReadOnlyList<WorkOrderBlockerResponse> Blockers = null!,
    WorkOrderCloseoutResponse? Closeout = null);

public sealed record WorkOrderQualificationCheckResultResponse(
    Guid? CheckId,
    string? StaffarrPersonId,
    string QualificationKey,
    string Outcome,
    string ReasonCode,
    string Message);

public sealed record WorkOrderTechnicianAssignmentResponse(
    Guid AssignmentId,
    Guid WorkOrderId,
    string PersonId,
    string AssignmentRole,
    string Status,
    DateTimeOffset AssignedAt,
    string? AssignedByPersonId,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<string> RequiredQualificationRefs,
    IReadOnlyList<WorkOrderQualificationCheckResultResponse> QualificationCheckSnapshot);

public sealed record MaintenancePermitRefResponse(
    Guid PermitRefId,
    Guid WorkOrderId,
    string PermitType,
    string SourceProduct,
    string? SourceObjectRef,
    string? RecordRef,
    string? StatusSnapshot,
    string? ApprovedByPersonId,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidTo);

public sealed record ReturnToServiceResponse(
    Guid ReturnToServiceId,
    Guid WorkOrderId,
    Guid AssetId,
    string Status,
    IReadOnlyList<string> RequiredChecks,
    IReadOnlyList<string> CompletedChecks,
    Guid? FinalInspectionRef,
    string? ApprovedByPersonId,
    DateTimeOffset? ApprovedAt,
    string? RejectionReason,
    string? FinalReadinessStatus,
    IReadOnlyList<Guid> RecordRefs);

public sealed record CreateWorkOrderRequest(
    Guid AssetId,
    string Title,
    string Description,
    string Priority,
    string? AssignedTechnicianPersonId,
    Guid? PmScheduleId,
    Guid? DefectId = null,
    string? DraftPlanJson = null,
    DateTimeOffset? PlannedStartAt = null,
    DateTimeOffset? PlannedDueAt = null,
    string? Source = null,
    string? WorkOrderType = null,
    string? OriginType = null,
    string? OriginRef = null);

public sealed record CreateWorkOrderFromDefectRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId,
    string? DraftPlanJson = null,
    DateTimeOffset? PlannedStartAt = null,
    DateTimeOffset? PlannedDueAt = null);

public sealed record UpdateWorkOrderRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId,
    string? DraftPlanJson = null,
    DateTimeOffset? PlannedStartAt = null,
    DateTimeOffset? PlannedDueAt = null);

public sealed record UpdateWorkOrderStatusRequest(string Status);

public sealed record WorkOrderFindingResponse(
    string Category,
    string Severity,
    string Code,
    string Message,
    string? FieldKey = null,
    string? SectionKey = null,
    string? Source = null);

public sealed record WorkOrderDuplicateMatchResponse(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Title,
    string Status,
    string AssetTag,
    string AssetName,
    string MatchReason,
    int SimilarityScore);

public sealed record WorkOrderValidationResponse(
    bool IsValid,
    IReadOnlyList<WorkOrderFindingResponse> Findings);

public sealed record WorkOrderPreviewResponse(
    WorkOrderDetailResponse WorkOrder,
    IReadOnlyList<WorkOrderFindingResponse> Findings,
    IReadOnlyList<WorkOrderDuplicateMatchResponse> DuplicateMatches,
    AssetReadinessResponse? AssetReadiness,
    bool CanOpen,
    bool CanSchedule,
    bool CanStart);

public sealed record CreateWorkOrderBlockerRequest(
    string BlockerType,
    string SourceProduct,
    string? SourceObjectRef,
    string Title,
    string Description,
    string Severity,
    string? RequiredAction,
    string? CreatedByPersonId,
    string? Status);

public sealed record WorkOrderBlockerResponse(
    Guid BlockerId,
    Guid WorkOrderId,
    string BlockerType,
    string SourceProduct,
    string? SourceObjectRef,
    string Title,
    string Description,
    string Severity,
    string Status,
    string? RequiredAction,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId,
    DateTimeOffset? ResolvedAt,
    string? ResolvedByPersonId,
    string? OverrideReason);

public sealed record CreateWorkOrderCloseoutRequest(
    string CompletionSummary,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveActionRecommendation,
    bool AssetReturnedToService,
    DateTimeOffset? ReturnToServiceAt,
    string? ReturnToServiceByPersonId,
    bool PostRepairInspectionRequired,
    Guid? PostRepairInspectionRef,
    bool SupervisorReviewRequired,
    string? SupervisorReviewedByPersonId,
    DateTimeOffset? SupervisorReviewedAt,
    bool ComplianceReviewRequired,
    string? ComplianceReviewedByPersonId,
    DateTimeOffset? ComplianceReviewedAt,
    bool QualityReviewRequired,
    string? QualityReviewedByPersonId,
    DateTimeOffset? QualityReviewedAt,
    bool EvidenceAccepted,
    string? UnresolvedDefectRefs,
    string? FollowUpWorkOrderRefs,
    string? CustomerImpactSummary,
    string? DowntimeSummary,
    string? FinalAssetReadinessStatus,
    string? FinalStatus,
    IReadOnlyList<Guid>? PermitRecordRefs = null,
    IReadOnlyList<Guid>? EvidenceRecordRefs = null);

public sealed record WorkOrderCloseoutResponse(
    Guid CloseoutId,
    Guid WorkOrderId,
    string CompletionSummary,
    string? RootCause,
    string? CorrectiveAction,
    string? PreventiveActionRecommendation,
    bool AssetReturnedToService,
    DateTimeOffset? ReturnToServiceAt,
    string? ReturnToServiceByPersonId,
    bool PostRepairInspectionRequired,
    Guid? PostRepairInspectionRef,
    bool SupervisorReviewRequired,
    string? SupervisorReviewedByPersonId,
    DateTimeOffset? SupervisorReviewedAt,
    bool ComplianceReviewRequired,
    string? ComplianceReviewedByPersonId,
    DateTimeOffset? ComplianceReviewedAt,
    bool QualityReviewRequired,
    string? QualityReviewedByPersonId,
    DateTimeOffset? QualityReviewedAt,
    bool EvidenceAccepted,
    string? UnresolvedDefectRefs,
    string? FollowUpWorkOrderRefs,
    string? CustomerImpactSummary,
    string? DowntimeSummary,
    string? FinalAssetReadinessStatus,
    string? FinalStatus,
    IReadOnlyList<Guid> PermitRecordRefs,
    IReadOnlyList<Guid> EvidenceRecordRefs,
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId);

public sealed record PmWorkOrderGenerationResult(
    Guid WorkOrderId,
    string WorkOrderNumber,
    bool LinkedExisting);
