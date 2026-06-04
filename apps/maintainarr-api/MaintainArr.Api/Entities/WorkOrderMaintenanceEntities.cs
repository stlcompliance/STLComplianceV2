using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderBlocker : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string BlockerType { get; set; } = string.Empty;

    public string SourceProduct { get; set; } = string.Empty;

    public string? SourceObjectRef { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Severity { get; set; } = "moderate";

    public string Status { get; set; } = "active";

    public string? RequiredAction { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public string? ResolvedByPersonId { get; set; }

    public string? OverrideReason { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}

public sealed class WorkOrderCloseout : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string CompletionSummary { get; set; } = string.Empty;

    public string? RootCause { get; set; }

    public string? CorrectiveAction { get; set; }

    public string? PreventiveActionRecommendation { get; set; }

    public bool AssetReturnedToService { get; set; }

    public DateTimeOffset? ReturnToServiceAt { get; set; }

    public string? ReturnToServiceByPersonId { get; set; }

    public bool PostRepairInspectionRequired { get; set; }

    public Guid? PostRepairInspectionRef { get; set; }

    public bool SupervisorReviewRequired { get; set; }

    public string? SupervisorReviewedByPersonId { get; set; }

    public DateTimeOffset? SupervisorReviewedAt { get; set; }

    public bool ComplianceReviewRequired { get; set; }

    public string? ComplianceReviewedByPersonId { get; set; }

    public DateTimeOffset? ComplianceReviewedAt { get; set; }

    public bool QualityReviewRequired { get; set; }

    public string? QualityReviewedByPersonId { get; set; }

    public DateTimeOffset? QualityReviewedAt { get; set; }

    public bool EvidenceAccepted { get; set; }

    public string? UnresolvedDefectRefs { get; set; }

    public string? FollowUpWorkOrderRefs { get; set; }

    public string? CustomerImpactSummary { get; set; }

    public string? DowntimeSummary { get; set; }

    public string? FinalAssetReadinessStatus { get; set; }

    public string? FinalStatus { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}
