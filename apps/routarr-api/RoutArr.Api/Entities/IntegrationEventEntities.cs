using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantIntegrationEventSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxAttempts { get; set; } = 5;

    public int RetryIntervalMinutes { get; set; } = 15;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class IntegrationOutboxEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string ProcessingStatus { get; set; } = IntegrationEventStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public static class IntegrationEventStatuses
{
    public const string Pending = "pending";

    public const string Processed = "processed";

    public const string Abandoned = "abandoned";
}

public static class RoutArrIntegrationOutboxEventKinds
{
    public const string TripDispatched = "trip.dispatched";

    public const string TripCompleted = "trip.completed";

    public const string DriverAssignmentChanged = "driver.assignment.changed";

    public const string ExceptionCreated = "exception.created";
}
