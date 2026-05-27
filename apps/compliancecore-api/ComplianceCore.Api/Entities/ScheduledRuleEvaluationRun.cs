namespace ComplianceCore.Api.Entities;

public sealed class ScheduledRuleEvaluationRun
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = ScheduledRuleEvaluationRunStatuses.InProgress;

    public int IntervalHours { get; set; }

    public int PacksDueCount { get; set; }

    public int PacksProcessedCount { get; set; }

    public int EvaluatedCount { get; set; }

    public int SkippedCount { get; set; }

    public int AllowCount { get; set; }

    public int WarnCount { get; set; }

    public int BlockCount { get; set; }

    public string? ErrorMessage { get; set; }
}

public static class ScheduledRuleEvaluationRunStatuses
{
    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Failed = "failed";
}
