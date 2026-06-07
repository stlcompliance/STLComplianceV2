using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class RouteStop : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RouteId { get; set; }

    public string StopKey { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string AddressLabel { get; set; } = string.Empty;

    public Guid? StaffarrSiteOrgUnitId { get; set; }

    public string StaffarrSiteNameSnapshot { get; set; } = string.Empty;

    public string StopType { get; set; } = RouteStopTypes.Waypoint;

    public string StopStatus { get; set; } = RouteStopStatuses.Pending;

    public int SequenceNumber { get; set; }

    public decimal? GeofenceAnchorLatitude { get; set; }

    public decimal? GeofenceAnchorLongitude { get; set; }

    public int? GeofenceRadiusMeters { get; set; }

    public DateTimeOffset? LastGeofenceCheckAt { get; set; }

    public string? LastGeofenceResult { get; set; }

    public decimal? LastGeofenceDistanceMeters { get; set; }

    public decimal? LastGeofenceReportedLatitude { get; set; }

    public decimal? LastGeofenceReportedLongitude { get; set; }

    public DateTimeOffset? ScheduledArrivalAt { get; set; }

    public DateTimeOffset? ArrivedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DispatchRoute Route { get; set; } = null!;
}

public static class RouteStopTypes
{
    public const string Pickup = "pickup";

    public const string Delivery = "delivery";

    public const string Waypoint = "waypoint";

    public const string Depot = "depot";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pickup,
        Delivery,
        Waypoint,
        Depot,
    };
}

public static class RouteStopStatuses
{
    public const string Pending = "pending";

    public const string Planned = "planned";

    public const string EnRoute = "en_route";

    public const string Arrived = "arrived";

    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Skipped = "skipped";

    public const string Failed = "failed";

    public const string Canceled = "canceled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Planned,
        EnRoute,
        Arrived,
        InProgress,
        Completed,
        Skipped,
        Failed,
        Canceled,
    };

    public static readonly IReadOnlySet<string> Terminal = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Completed,
        Skipped,
        Failed,
        Canceled,
    };
}
