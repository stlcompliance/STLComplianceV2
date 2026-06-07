using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrder : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid? DefectId { get; set; }

    public Guid? PmScheduleId { get; set; }

    public string WorkOrderNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? TemplateRef { get; set; }

    public string Priority { get; set; } = WorkOrderPriorities.Medium;

    public string Status { get; set; } = WorkOrderStatuses.Open;

    public string Source { get; set; } = WorkOrderSources.Manual;

    public string WorkOrderType { get; set; } = WorkOrderTypes.Corrective;

    public string OriginType { get; set; } = WorkOrderOriginTypes.Manual;

    public string? OriginRef { get; set; }

    public string? StaffarrLocationId { get; set; }

    public string RequiredQualificationRefsJson { get; set; } = "[]";

    public string QualificationCheckResultsJson { get; set; } = "[]";

    public string? DraftPlanJson { get; set; }

    public DateTimeOffset? PlannedStartAt { get; set; }

    public DateTimeOffset? PlannedDueAt { get; set; }

    public string? AssignedTechnicianPersonId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public Asset Asset { get; set; } = null!;

    public Defect? Defect { get; set; }

    public PmSchedule? PmSchedule { get; set; }

    public ICollection<WorkOrderTaskLine> TaskLines { get; set; } = [];

    public ICollection<WorkOrderLaborEntry> LaborEntries { get; set; } = [];

    public ICollection<WorkOrderEvidence> Evidence { get; set; } = [];

    public ICollection<WorkOrderPartsDemandLine> PartsDemandLines { get; set; } = [];

    public ICollection<WorkOrderBlocker> Blockers { get; set; } = [];

    public ICollection<WorkOrderCloseout> Closeouts { get; set; } = [];

    public ICollection<MaintenancePermitRef> PermitRefs { get; set; } = [];

    public ICollection<ReturnToService> ReturnToServices { get; set; } = [];
}

public static class WorkOrderStatuses
{
    public const string Draft = "draft";

    public const string Open = "open";

    public const string Requested = "requested";

    public const string Triage = "triage";

    public const string Rejected = "rejected";

    public const string Approved = "approved";

    public const string Planned = "planned";

    public const string WaitingParts = "waiting_parts";

    public const string WaitingLabor = "waiting_labor";

    public const string WaitingVendor = "waiting_vendor";

    public const string WaitingApproval = "waiting_approval";

    public const string WaitingCompliance = "waiting_compliance";

    public const string Scheduled = "scheduled";

    public const string Assigned = "assigned";

    public const string InProgress = "in_progress";

    public const string Paused = "paused";

    public const string Blocked = "blocked";

    public const string CompletedPendingReview = "completed_pending_review";

    public const string Completed = "completed";

    public const string Closed = "closed";

    public const string Cancelled = "cancelled";

    public const string Canceled = "canceled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Open,
        Requested,
        Triage,
        Rejected,
        Approved,
        Planned,
        WaitingParts,
        WaitingLabor,
        WaitingVendor,
        WaitingApproval,
        WaitingCompliance,
        Scheduled,
        Assigned,
        InProgress,
        Paused,
        Blocked,
        CompletedPendingReview,
        Completed,
        Closed,
        Cancelled,
        Canceled,
    };

    public static readonly string[] Active =
    [
        Open,
        Requested,
        Triage,
        Approved,
        Planned,
        WaitingParts,
        WaitingLabor,
        WaitingVendor,
        WaitingApproval,
        WaitingCompliance,
        Scheduled,
        Assigned,
        InProgress,
        Paused,
        Blocked,
        CompletedPendingReview,
    ];
}

public static class WorkOrderPriorities
{
    public const string Low = "low";

    public const string Medium = "medium";

    public const string High = "high";

    public const string Urgent = "urgent";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Low,
        Medium,
        High,
        Urgent,
    };
}

public static class WorkOrderSources
{
    public const string Manual = "manual";

    public const string Defect = "defect";

    public const string PmSchedule = "pm_schedule";
}

public static class WorkOrderTypes
{
    public const string Corrective = "corrective";
    public const string Preventive = "preventive";
    public const string InspectionFollowup = "inspection_followup";
    public const string DefectRepair = "defect_repair";
    public const string Emergency = "emergency";
    public const string Project = "project";
    public const string Compliance = "compliance";
    public const string Recall = "recall";
    public const string Warranty = "warranty";
    public const string Calibration = "calibration";
    public const string Installation = "installation";
    public const string Removal = "removal";
    public const string OperatorRequest = "operator_request";
    public const string VendorWork = "vendor_work";
}

public static class WorkOrderOriginTypes
{
    public const string Manual = "manual";
    public const string InspectionFailure = "inspection_failure";
    public const string Defect = "defect";
    public const string PmDue = "pm_due";
    public const string AssetBreakdown = "asset_breakdown";
    public const string OperatorRequest = "operator_request";
    public const string RouteException = "route_exception";
    public const string QualityHold = "quality_hold";
    public const string ComplianceRequirement = "compliance_requirement";
    public const string CustomerRequest = "customer_request";
    public const string WarrantyClaim = "warranty_claim";
    public const string RecallNotice = "recall_notice";
}
