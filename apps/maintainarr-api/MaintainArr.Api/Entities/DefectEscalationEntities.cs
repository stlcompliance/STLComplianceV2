using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class TenantDefectEscalationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int LowThresholdHours { get; set; } = DefectEscalationDefaults.LowThresholdHours;

    public int MediumThresholdHours { get; set; } = DefectEscalationDefaults.MediumThresholdHours;

    public int HighThresholdHours { get; set; } = DefectEscalationDefaults.HighThresholdHours;

    public int CriticalThresholdHours { get; set; } = DefectEscalationDefaults.CriticalThresholdHours;

    public bool AutoAcknowledgeOnEscalation { get; set; } = true;

    public bool AutoCreateWorkOrderOnEscalation { get; set; } = true;

    public bool BumpSeverityOnRepeatEscalation { get; set; } = true;

    public bool NotifyOnEscalation { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DefectEscalationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int EscalatedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class DefectEscalationEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid DefectId { get; set; }

    public string ActionKind { get; set; } = string.Empty;

    public string? PreviousSeverity { get; set; }

    public string? NewSeverity { get; set; }

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public Guid? WorkOrderId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class DefectEscalationDefaults
{
    public const int LowThresholdHours = 168;

    public const int MediumThresholdHours = 72;

    public const int HighThresholdHours = 24;

    public const int CriticalThresholdHours = 8;
}

public static class DefectEscalationActionKinds
{
    public const string Acknowledged = "acknowledged";

    public const string WorkOrderCreated = "work_order_created";

    public const string SeverityBumped = "severity_bumped";

    public const string NotificationEnqueued = "notification_enqueued";
}
