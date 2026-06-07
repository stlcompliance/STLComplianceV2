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

    public string EvidenceRecordRefsJson { get; set; } = "[]";

    public string? UnresolvedDefectRefs { get; set; }

    public string? FollowUpWorkOrderRefs { get; set; }

    public string? CustomerImpactSummary { get; set; }

    public string? DowntimeSummary { get; set; }

    public string? FinalAssetReadinessStatus { get; set; }

    public string? FinalStatus { get; set; }

    public string PermitRecordRefsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedByPersonId { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}

public sealed class MaintenancePermitRef : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string PermitType { get; set; } = MaintenancePermitTypes.Other;

    public string SourceProduct { get; set; } = "maintainarr";

    public string? SourceObjectRef { get; set; }

    public string? RecordRef { get; set; }

    public string? StatusSnapshot { get; set; }

    public string? ApprovedByPersonId { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}

public static class MaintenancePermitTypes
{
    public const string LockoutTagout = "lockout_tagout";

    public const string HotWork = "hot_work";

    public const string ConfinedSpace = "confined_space";

    public const string Electrical = "electrical";

    public const string LineBreak = "line_break";

    public const string Excavation = "excavation";

    public const string WorkingAtHeight = "working_at_height";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        LockoutTagout,
        HotWork,
        ConfinedSpace,
        Electrical,
        LineBreak,
        Excavation,
        WorkingAtHeight,
        Other,
    };
}

public sealed class ReturnToService : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public Guid AssetId { get; set; }

    public string Status { get; set; } = ReturnToServiceStatuses.Pending;

    public string RequiredChecksJson { get; set; } = "[]";

    public string CompletedChecksJson { get; set; } = "[]";

    public Guid? FinalInspectionRef { get; set; }

    public string? ApprovedByPersonId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? FinalReadinessStatus { get; set; }

    public string RecordRefsJson { get; set; } = "[]";

    public WorkOrder WorkOrder { get; set; } = null!;
}

public static class ReturnToServiceStatuses
{
    public const string Pending = "pending";

    public const string Approved = "approved";

    public const string Rejected = "rejected";

    public const string NotRequired = "not_required";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Approved,
        Rejected,
        NotRequired,
    };
}
