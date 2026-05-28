using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantTripCompletionRollupSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public int StalenessHours { get; set; } = TripCompletionRollupDefaults.StalenessHours;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TripCompletionRollup : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string TripNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string DispatchStatus { get; set; } = string.Empty;

    public string? AssignedDriverPersonId { get; set; }

    public string? VehicleRefKey { get; set; }

    public DateTimeOffset? ScheduledStartAt { get; set; }

    public DateTimeOffset? ScheduledEndAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public int? DurationMinutes { get; set; }

    public int RouteCount { get; set; }

    public int CompletedRouteCount { get; set; }

    public int StopCount { get; set; }

    public int CompletedStopCount { get; set; }

    public int SkippedStopCount { get; set; }

    public int PendingStopCount { get; set; }

    public int LoadCount { get; set; }

    public int DeliveredLoadCount { get; set; }

    public int PendingLoadCount { get; set; }

    public DateTimeOffset SourceUpdatedAt { get; set; }

    public DateTimeOffset ComputedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TripCompletionEvent> Events { get; set; } = [];
}

public sealed class TripCompletionEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public Guid RollupId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Detail { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public int SequenceNumber { get; set; }

    public string SourceEntityType { get; set; } = string.Empty;

    public string SourceEntityId { get; set; } = string.Empty;

    public TripCompletionRollup Rollup { get; set; } = null!;
}

public sealed class TripCompletionRollupRun : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public DateTimeOffset AsOfUtc { get; set; }

    public int CandidatesFound { get; set; }

    public int RefreshedCount { get; set; }

    public int SkippedCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class TripCompletionRollupDefaults
{
    public const int StalenessHours = 1;
}

public static class TripCompletionEventKinds
{
    public const string TripAssigned = "trip_assigned";

    public const string TripDispatched = "trip_dispatched";

    public const string TripStarted = "trip_started";

    public const string TripCompleted = "trip_completed";

    public const string TripCancelled = "trip_cancelled";

    public const string RouteCompleted = "route_completed";

    public const string StopCompleted = "stop_completed";

    public const string StopSkipped = "stop_skipped";
}

public static class TripTerminalDispatchStatuses
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        TripDispatchStatuses.Completed,
        TripDispatchStatuses.Cancelled,
    };
}
