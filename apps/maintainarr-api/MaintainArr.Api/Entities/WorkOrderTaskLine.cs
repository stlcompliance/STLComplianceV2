using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class WorkOrderTaskLine : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Status { get; set; } = WorkOrderTaskStatuses.Pending;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
}

public static class WorkOrderTaskStatuses
{
    public const string Pending = "pending";

    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        InProgress,
        Completed,
    };
}
