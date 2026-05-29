namespace NexArr.Api.Entities;

public static class PlatformOutboxEventKinds
{
    public const string TenantCreated = "tenant.created";
    public const string TenantUpdated = "tenant.updated";
    public const string TenantDisabled = "tenant.disabled";
    public const string TenantEnabled = "tenant.enabled";
    public const string TenantEntitlementGranted = "tenant.entitlement.granted";
    public const string TenantEntitlementUpdated = "tenant.entitlement.updated";
    public const string TenantEntitlementRevoked = "tenant.entitlement.revoked";
    public const string TenantMembershipAdded = "tenant.membership.added";
    public const string TenantMembershipRemoved = "tenant.membership.removed";
    public const string UserDisabled = "user.disabled";
    public const string UserEnabled = "user.enabled";
}

public static class PlatformOutboxEventStatuses
{
    public const string Pending = "pending";
    public const string Published = "published";
    public const string DeadLetter = "dead_letter";
}

public sealed class PlatformOutboxEvent
{
    public Guid Id { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;

    public string PayloadJson { get; set; } = "{}";

    public Guid? TenantId { get; set; }

    public Guid? ActorPersonId { get; set; }

    public string? ProductCode { get; set; }

    public Guid CorrelationId { get; set; }

    public string ProcessingStatus { get; set; } = PlatformOutboxEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PlatformOutboxPublisherSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-0000000000b1");

    public Guid Id { get; set; } = SingletonId;

    public bool IsEnabled { get; set; } = true;

    public int MaxRetryAttempts { get; set; } = 5;

    public int RetryIntervalMinutes { get; set; } = 5;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PlatformOutboxPublisherRun
{
    public Guid Id { get; set; }

    public string Outcome { get; set; } = string.Empty;

    public int PublishedCount { get; set; }

    public int FailedCount { get; set; }

    public int DeadLetterCount { get; set; }

    public int SkippedCount { get; set; }

    public string? SkipReason { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
