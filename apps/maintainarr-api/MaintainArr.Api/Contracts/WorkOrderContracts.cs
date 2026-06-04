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
    string? AssignedTechnicianPersonId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt,
    DowntimeFollowUpResponse? DowntimeFollowUp = null,
    IReadOnlyList<WorkOrderBlockerResponse> Blockers = null!,
    WorkOrderCloseoutResponse? Closeout = null);

public sealed record CreateWorkOrderRequest(
    Guid AssetId,
    string Title,
    string Description,
    string Priority,
    string? AssignedTechnicianPersonId,
    Guid? PmScheduleId);

public sealed record CreateWorkOrderFromDefectRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId);

public sealed record UpdateWorkOrderRequest(
    string? Title,
    string? Description,
    string? Priority,
    string? AssignedTechnicianPersonId);

public sealed record UpdateWorkOrderStatusRequest(string Status);

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
    string? FinalStatus);

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
    DateTimeOffset CreatedAt,
    string? CreatedByPersonId);

public sealed record PmWorkOrderGenerationResult(
    Guid WorkOrderId,
    string WorkOrderNumber,
    bool LinkedExisting);
