using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchException : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ExceptionKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = DispatchExceptionCategories.Other;

    public string Status { get; set; } = DispatchExceptionStatuses.Open;

    public Guid? TripId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public string ResolutionNotes { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}

public static class DispatchExceptionStatuses
{
    public const string Open = "open";

    public const string Assigned = "assigned";

    public const string Resolved = "resolved";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Assigned,
        Resolved,
        Cancelled,
    };

    public static readonly IReadOnlySet<string> OpenQueue = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Open,
        Assigned,
    };

    public static bool IsTerminal(string status) =>
        string.Equals(status, Resolved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);
}

public static class DispatchExceptionCategories
{
    public const string Delay = "delay";

    public const string Driver = "driver";

    public const string Vehicle = "vehicle";

    public const string Route = "route";

    public const string Stop = "stop";

    public const string Compliance = "compliance";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Delay,
        Driver,
        Vehicle,
        Route,
        Stop,
        Compliance,
        Other,
    };
}
