using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class Trip : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string TripNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string DispatchStatus { get; set; } = TripDispatchStatuses.Planned;

    public string? AssignedDriverPersonId { get; set; }

    public string? VehicleRefKey { get; set; }

    public DateTimeOffset? ScheduledStartAt { get; set; }

    public DateTimeOffset? ScheduledEndAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public ICollection<TripLoad> Loads { get; set; } = [];

    public ICollection<TripPartsDemandLine> PartsDemandLines { get; set; } = [];
}

public static class TripDispatchStatuses
{
    public const string Planned = "planned";

    public const string Assigned = "assigned";

    public const string Dispatched = "dispatched";

    public const string InProgress = "in_progress";

    public const string Completed = "completed";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Planned,
        Assigned,
        Dispatched,
        InProgress,
        Completed,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> Active = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Planned,
        Assigned,
        Dispatched,
        InProgress,
    };
}
