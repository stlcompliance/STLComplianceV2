using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class TenantProcurementExceptionEscalationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int EscalationCooldownHours { get; set; } = ProcurementExceptionEscalationDefaults.EscalationCooldownHours;

    public int MaxEscalationsPerException { get; set; } = ProcurementExceptionEscalationDefaults.MaxEscalationsPerException;

    public bool NotifyOnProcurementExceptionSlaEscalation { get; set; } = true;

    public bool AutoCloseCompletedExceptionsEnabled { get; set; }

    public int AutoCloseCompletedExceptionsAfterHours { get; set; } =
        ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ProcurementExceptionEscalationEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ProcurementExceptionId { get; set; }

    public int EscalationLevel { get; set; }

    public string ActionKind { get; set; } = string.Empty;

    public Guid? NotificationDispatchId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ProcurementExceptionEscalationRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int EscalatedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class ProcurementExceptionEscalationDefaults
{
    public const int EscalationCooldownHours = 24;

    public const int MaxEscalationsPerException = 5;

    public const int AutoCloseCompletedExceptionsAfterHours = 48;
}

public static class ProcurementExceptionEscalationActionKinds
{
    public const string NotificationEnqueued = "notification_enqueued";

    public const string Escalated = "escalated";
}
