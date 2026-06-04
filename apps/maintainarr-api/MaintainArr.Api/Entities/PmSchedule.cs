using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class PmSchedule : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string ScheduleKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ScheduleMode { get; set; } = PmScheduleModes.Calendar;

    public Guid? AssetMeterId { get; set; }

    public decimal? IntervalUsage { get; set; }

    public decimal? NextDueAtUsage { get; set; }

    public decimal? LastCompletedUsage { get; set; }

    public int IntervalDays { get; set; }

    public DateTimeOffset NextDueAt { get; set; }

    public DateTimeOffset? LastCompletedAt { get; set; }

    public DateTimeOffset? SkippedAt { get; set; }

    public Guid? SkippedByPersonId { get; set; }

    public string? SkippedReason { get; set; }

    public string DueStatus { get; set; } = PmDueStatuses.Scheduled;

    public string Status { get; set; } = "active";

    public DateTimeOffset? LastDueScanAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Asset Asset { get; set; } = null!;

    public AssetMeter? AssetMeter { get; set; }
}

public static class PmScheduleModes
{
    public const string Calendar = "calendar";
    public const string Meter = "meter";
}

public static class PmDueStatuses
{
    public const string Scheduled = "scheduled";
    public const string Due = "due";
    public const string Overdue = "overdue";
    public const string Completed = "completed";
    public const string Skipped = "skipped";
}
