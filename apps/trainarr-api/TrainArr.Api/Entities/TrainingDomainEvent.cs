using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class TrainingDomainEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public Guid StaffarrPersonId { get; set; }

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = TrainingDomainEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public static class TrainingDomainEventKinds
{
    public const string AssignmentCreated = "assignment_created";

    public const string AssignmentCompleted = "assignment_completed";

    public const string QualificationIssued = "qualification_issued";

    public const string QualificationSuspended = "qualification_suspended";

    public const string QualificationRevoked = "qualification_revoked";

    public const string QualificationExpired = "qualification_expired";

    public const string RemediationRequired = "remediation_required";
}

public static class TrainingDomainEventStatuses
{
    public const string Pending = "pending";

    public const string Processed = "processed";

    public const string Abandoned = "abandoned";
}
