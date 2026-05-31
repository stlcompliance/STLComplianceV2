using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TenantDispatchNotificationSettings : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public bool IsEnabled { get; set; }

    public string? NotificationWebhookUrl { get; set; }

    public bool NotifyOnTripAssigned { get; set; } = true;

    public bool NotifyOnTripDispatched { get; set; } = true;

    public bool NotifyOnTripAccepted { get; set; } = true;

    public bool NotifyOnTripInProgress { get; set; } = true;

    public bool NotifyOnTripCompleted { get; set; } = true;

    public bool NotifyOnTripCancelled { get; set; } = true;

    public bool NotifyOnDriverAssignmentChanged { get; set; } = true;

    public bool NotifyOnRouteCancelled { get; set; } = true;

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class DispatchNotificationDispatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string EventKind { get; set; } = string.Empty;

    public Guid? TripId { get; set; }

    public Guid? RouteId { get; set; }

    public string? DriverPersonId { get; set; }

    public string RelatedEntityType { get; set; } = string.Empty;

    public Guid RelatedEntityId { get; set; }

    public string DispatchStatus { get; set; } = string.Empty;

    public string? WebhookHost { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }
}

public static class DispatchNotificationEventKinds
{
    public const string TripAssigned = "trip_assigned";

    public const string TripDispatched = "trip_dispatched";

    public const string TripAccepted = "trip_accepted";

    public const string TripInProgress = "trip_in_progress";

    public const string TripCompleted = "trip_completed";

    public const string TripCancelled = "trip_cancelled";

    public const string DriverAssignmentChanged = "driver_assignment_changed";

    public const string RouteCancelled = "route_cancelled";
}

public static class DispatchNotificationDispatchStatuses
{
    public const string Pending = "pending";

    public const string Sent = "sent";

    public const string Failed = "failed";

    public const string Skipped = "skipped";
}
