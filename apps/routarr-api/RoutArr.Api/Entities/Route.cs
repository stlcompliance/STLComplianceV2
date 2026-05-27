using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchRoute : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string RouteNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string RouteStatus { get; set; } = RouteStatuses.Draft;

    public Guid? TripId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ActivatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public Trip? Trip { get; set; }

    public ICollection<RouteStop> Stops { get; set; } = [];
}

public static class RouteStatuses
{
    public const string Draft = "draft";

    public const string Planned = "planned";

    public const string Active = "active";

    public const string Completed = "completed";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Planned,
        Active,
        Completed,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> Editable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Planned,
    };
}
