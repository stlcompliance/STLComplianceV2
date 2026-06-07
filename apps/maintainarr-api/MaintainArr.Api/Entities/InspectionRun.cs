using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class InspectionRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid InspectionTemplateId { get; set; }

    public Guid? PmScheduleId { get; set; }

    public int TemplateVersion { get; set; }

    public string Status { get; set; } = InspectionRunStatuses.InProgress;

    public string? Result { get; set; }

    public Guid StartedByUserId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Asset Asset { get; set; } = null!;

    public InspectionTemplate InspectionTemplate { get; set; } = null!;

    public PmSchedule? PmSchedule { get; set; }

    public ICollection<InspectionRunAnswer> Answers { get; set; } = [];

    public ICollection<InspectionRunEvidence> Evidence { get; set; } = [];

    public ICollection<InspectionRunPauseEvent> PauseEvents { get; set; } = [];
}

public sealed class InspectionRunPauseEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid InspectionRunId { get; set; }

    public DateTimeOffset PausedAt { get; set; }

    public DateTimeOffset? ResumedAt { get; set; }

    public int? DurationMinutes { get; set; }

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public Guid PausedByUserId { get; set; }

    public Guid? ResumedByUserId { get; set; }

    public InspectionRun InspectionRun { get; set; } = null!;
}

public sealed class InspectionRunAnswer : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid InspectionRunId { get; set; }

    public Guid ChecklistItemId { get; set; }

    public string? PassFailValue { get; set; }

    public decimal? NumericValue { get; set; }

    public string? TextValue { get; set; }

    public string SelectedOptionsJson { get; set; } = "[]";

    public DateTimeOffset AnsweredAt { get; set; }

    public Guid AnsweredByUserId { get; set; }

    public InspectionRun InspectionRun { get; set; } = null!;

    public InspectionChecklistItem ChecklistItem { get; set; } = null!;
}

public static class InspectionRunStatuses
{
    public const string InProgress = "in_progress";

    public const string Paused = "paused";

    public const string Completed = "completed";
}

public static class InspectionRunResults
{
    public const string Passed = "passed";

    public const string Failed = "failed";
}

public static class InspectionAnswerPassFailValues
{
    public const string Pass = "pass";

    public const string Fail = "fail";

    public const string Na = "na";
}
