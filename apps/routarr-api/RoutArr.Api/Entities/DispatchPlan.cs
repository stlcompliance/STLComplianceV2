using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class DispatchPlan : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string DispatchNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset DispatchDate { get; set; }

    public string DispatchType { get; set; } = DispatchPlanTypes.Daily;

    public string Status { get; set; } = DispatchPlanStatuses.Draft;

    public string? PlannerPersonId { get; set; }

    public string? DispatcherPersonId { get; set; }

    public Guid? StaffarrSiteId { get; set; }

    public string RouteRefsJson { get; set; } = "[]";

    public string TripRefsJson { get; set; } = "[]";

    public string BlockerRefsJson { get; set; } = "[]";

    public string Notes { get; set; } = string.Empty;

    public string CreatedByPersonId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ReleasedAt { get; set; }

    public string? ReleasedByPersonId { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CanceledAt { get; set; }

    public string? CancelReason { get; set; }
}

public static class DispatchPlanTypes
{
    public const string Daily = "daily";
    public const string Shift = "shift";
    public const string Customer = "customer";
    public const string Site = "site";
    public const string Lane = "lane";
    public const string Inbound = "inbound";
    public const string Outbound = "outbound";
    public const string Mixed = "mixed";
    public const string Emergency = "emergency";
    public const string AdHoc = "ad_hoc";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Daily,
        Shift,
        Customer,
        Site,
        Lane,
        Inbound,
        Outbound,
        Mixed,
        Emergency,
        AdHoc,
    };
}

public static class DispatchPlanStatuses
{
    public const string Draft = "draft";
    public const string Planning = "planning";
    public const string ReadyForRelease = "ready_for_release";
    public const string Released = "released";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Canceled = "canceled";
    public const string Archived = "archived";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Planning,
        ReadyForRelease,
        Released,
        InProgress,
        Completed,
        Canceled,
        Archived,
    };
}
