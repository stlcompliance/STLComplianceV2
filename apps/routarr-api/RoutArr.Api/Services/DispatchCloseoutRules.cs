using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class DispatchCloseoutRules
{
    public const string ScopeDaily = "daily";

    public const string ScopeWeekly = "weekly";

    public const string TripDispositionComplete = "complete";

    public const string TripDispositionCancel = "cancel";

    public const string StopDispositionSkip = "skip";

    public const string StopDispositionComplete = "complete";

    public static readonly IReadOnlySet<string> TripDispositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        TripDispositionComplete,
        TripDispositionCancel,
    };

    public static readonly IReadOnlySet<string> StopDispositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        StopDispositionSkip,
        StopDispositionComplete,
    };

    public static readonly IReadOnlySet<string> OpenRouteStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        RouteStatuses.Draft,
        RouteStatuses.Planned,
        RouteStatuses.Active,
    };

    public static string NormalizeScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return ScopeDaily;
        }

        var normalized = scope.Trim().ToLowerInvariant();
        return normalized is ScopeDaily or ScopeWeekly
            ? normalized
            : throw new STLCompliance.Shared.Contracts.StlApiException(
                "dispatch_closeout.invalid_scope",
                "Closeout scope must be daily or weekly.",
                400);
    }

    public static (DateTimeOffset Start, DateTimeOffset End) GetWindow(string scope, DateTimeOffset now)
    {
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
        return scope == ScopeWeekly
            ? (dayStart, dayStart.AddDays(7))
            : (dayStart, dayStart.AddDays(1));
    }

    public static string NormalizeTripDisposition(string disposition)
    {
        var normalized = disposition?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!TripDispositions.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "dispatch_closeout.invalid_trip_disposition",
                "Remaining trip disposition must be complete or cancel.",
                400);
        }

        return normalized;
    }

    public static string NormalizeStopDisposition(string disposition)
    {
        var normalized = disposition?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!StopDispositions.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "dispatch_closeout.invalid_stop_disposition",
                "Open stop disposition must be skip or complete.",
                400);
        }

        return normalized;
    }

    public static TripCloseoutPlan PlanTrip(Trip trip, string tripDisposition)
    {
        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus))
        {
            return new TripCloseoutPlan(
                trip.DispatchStatus,
                trip.DispatchStatus,
                CanApply: false,
                "trip.not_open",
                "Trip is already closed.",
                []);
        }

        if (string.Equals(tripDisposition, TripDispositionCancel, StringComparison.OrdinalIgnoreCase))
        {
            return new TripCloseoutPlan(
                trip.DispatchStatus,
                TripDispatchStatuses.Cancelled,
                CanApply: true,
                null,
                null,
                [TripDispatchStatuses.Cancelled]);
        }

        return PlanTripComplete(trip);
    }

    public static StopCloseoutPlan PlanStop(RouteStop stop, string stopDisposition)
    {
        if (RouteStopStatuses.Terminal.Contains(stop.StopStatus))
        {
            return new StopCloseoutPlan(
                stop.StopStatus,
                stop.StopStatus,
                CanApply: false,
                "route_stop.not_open",
                "Stop is already closed.",
                stop.StopStatus);
        }

        if (string.Equals(stopDisposition, StopDispositionSkip, StringComparison.OrdinalIgnoreCase))
        {
            if (!RouteStopStatusRules.CanTransition(stop.StopStatus, RouteStopStatuses.Skipped))
            {
                return BlockedStop(stop, RouteStopStatuses.Skipped);
            }

            return new StopCloseoutPlan(
                stop.StopStatus,
                RouteStopStatuses.Skipped,
                CanApply: true,
                null,
                null,
                RouteStopStatuses.Skipped);
        }

        if (string.Equals(stop.StopStatus, RouteStopStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return new StopCloseoutPlan(
                stop.StopStatus,
                RouteStopStatuses.Pending,
                CanApply: false,
                "route_stop.arrival_required",
                "Pending stops must be skipped during closeout; use skip disposition or mark arrived first.",
                RouteStopStatuses.Completed);
        }

        if (!RouteStopStatusRules.CanTransition(stop.StopStatus, RouteStopStatuses.Completed))
        {
            return BlockedStop(stop, RouteStopStatuses.Completed);
        }

        return new StopCloseoutPlan(
            stop.StopStatus,
            RouteStopStatuses.Completed,
            CanApply: true,
            null,
            null,
            RouteStopStatuses.Completed);
    }

    public static RouteCloseoutPlan PlanRoute(
        DispatchRoute route,
        string tripDisposition,
        bool allStopsTerminal)
    {
        if (!OpenRouteStatuses.Contains(route.RouteStatus))
        {
            return new RouteCloseoutPlan(
                route.RouteStatus,
                route.RouteStatus,
                CanApply: false,
                "route.not_open",
                "Route is already closed.",
                route.RouteStatus);
        }

        if (string.Equals(tripDisposition, TripDispositionCancel, StringComparison.OrdinalIgnoreCase))
        {
            return new RouteCloseoutPlan(
                route.RouteStatus,
                RouteStatuses.Cancelled,
                CanApply: true,
                null,
                null,
                RouteStatuses.Cancelled);
        }

        if (!allStopsTerminal)
        {
            return new RouteCloseoutPlan(
                route.RouteStatus,
                RouteStatuses.Completed,
                CanApply: false,
                "route.stops_open",
                "All route stops must be closed before the route can be completed.",
                RouteStatuses.Completed);
        }

        return new RouteCloseoutPlan(
            route.RouteStatus,
            RouteStatuses.Completed,
            CanApply: true,
            null,
            null,
            RouteStatuses.Completed);
    }

    private static TripCloseoutPlan PlanTripComplete(Trip trip)
    {
        var status = trip.DispatchStatus.Trim().ToLowerInvariant();
        if (string.Equals(status, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            return AppliedTripPlan(trip.DispatchStatus, [TripDispatchStatuses.Completed]);
        }

        if (string.Equals(status, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase))
        {
            return AppliedTripPlan(
                trip.DispatchStatus,
                [TripDispatchStatuses.InProgress, TripDispatchStatuses.Completed]);
        }

        if (string.Equals(status, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId))
            {
                return BlockedTrip(
                    trip.DispatchStatus,
                    "trip.driver_required",
                    "A driver must be assigned before completing this trip.",
                    [TripDispatchStatuses.Dispatched, TripDispatchStatuses.InProgress, TripDispatchStatuses.Completed]);
            }

            return AppliedTripPlan(
                trip.DispatchStatus,
                [
                    TripDispatchStatuses.Dispatched,
                    TripDispatchStatuses.InProgress,
                    TripDispatchStatuses.Completed,
                ]);
        }

        return BlockedTrip(
            trip.DispatchStatus,
            "trip.closeout_complete_blocked",
            "Planned trips cannot be completed during closeout; cancel them instead.",
            [TripDispatchStatuses.Completed]);
    }

    private static TripCloseoutPlan AppliedTripPlan(string current, IReadOnlyList<string> steps) =>
        new(
            current,
            steps[^1],
            CanApply: true,
            null,
            null,
            steps);

    private static TripCloseoutPlan BlockedTrip(
        string current,
        string code,
        string message,
        IReadOnlyList<string> steps) =>
        new(current, steps[^1], CanApply: false, code, message, steps);

    private static StopCloseoutPlan BlockedStop(RouteStop stop, string target) =>
        new(
            stop.StopStatus,
            target,
            CanApply: false,
            "route_stop.invalid_transition",
            $"Cannot transition stop from {stop.StopStatus} to {target} during closeout.",
            target);
}

public sealed record TripCloseoutPlan(
    string CurrentStatus,
    string TargetStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage,
    IReadOnlyList<string> TransitionSteps);

public sealed record StopCloseoutPlan(
    string CurrentStatus,
    string TargetStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage,
    string TargetStopStatus);

public sealed record RouteCloseoutPlan(
    string CurrentStatus,
    string TargetStatus,
    bool CanApply,
    string? BlockCode,
    string? BlockMessage,
    string TargetRouteStatus);
