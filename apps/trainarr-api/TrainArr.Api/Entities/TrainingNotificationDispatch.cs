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

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }
}

public static class TrainingNotificationEventKinds
{
    public const string AssignmentCreated = "assignment_created";

    public const string QualificationExpiring = "qualification_expiring";

    public const string QualificationExpired = "qualification_expired";
}

public static class TrainingNotificationDispatchStatuses
{
    public const string Pending = "pending";

    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
