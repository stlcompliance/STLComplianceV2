using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class PmOccurrence : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PmScheduleId { get; set; }

    public Guid AssetId { get; set; }

    public int OccurrenceNumber { get; set; }

    public DateTimeOffset DueAt { get; set; }

    public string? DueMeterType { get; set; }

    public decimal? DueMeterValue { get; set; }

    public string Status { get; set; } = PmOccurrenceStatuses.Upcoming;

    public string? GeneratedWorkOrderRef { get; set; }

    public string? GeneratedInspectionRef { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? CompletedByWorkOrderRef { get; set; }

    public Guid? SkippedByPersonId { get; set; }

    public DateTimeOffset? SkippedAt { get; set; }

    public string? SkippedReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public PmSchedule PmSchedule { get; set; } = null!;

    public Asset Asset { get; set; } = null!;
}

public static class PmOccurrenceStatuses
{
    public const string Upcoming = "upcoming";

    public const string Due = "due";

    public const string Overdue = "overdue";

    public const string Generated = "generated";

    public const string Skipped = "skipped";

    public const string Completed = "completed";

    public const string Canceled = "canceled";
}
