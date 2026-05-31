using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingNotificationDispatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public Guid StaffarrPersonId { get; set; }

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string DispatchStatus { get; set; } = string.Empty;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }
}

public static class TrainingNotificationEventKinds
{
    public const string AssignmentCreated = "assignment_created";

    public const string QualificationExpiring = "qualification_expiring";

    public const string QualificationExpired = "qualification_expired";

    public const string AssignmentCompleted = "assignment_completed";

    public const string QualificationIssued = "qualification_issued";

    public const string QualificationSuspended = "qualification_suspended";

    public const string QualificationRevoked = "qualification_revoked";

    public const string RemediationRequired = "remediation_required";

    public const string AssignmentDueReminder = "assignment_due_reminder";

    public const string AssignmentOverdueEscalation = "assignment_overdue_escalation";
}

public static class TrainingNotificationDispatchStatuses
{
    public const string Pending = "pending";

    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";

    public const string Abandoned = "abandoned";
}
