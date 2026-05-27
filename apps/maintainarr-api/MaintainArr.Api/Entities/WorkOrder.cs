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

    public string Priority { get; set; } = WorkOrderPriorities.Medium;

    public string Status { get; set; } = WorkOrderStatuses.Open;

    public string Source { get; set; } = WorkOrderSources.Manual;

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
}

public static class WorkOrderStatuses
{
    public const string Open = "open";

    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        InProgress,
        Completed,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> Active = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        InProgress,
    };
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
