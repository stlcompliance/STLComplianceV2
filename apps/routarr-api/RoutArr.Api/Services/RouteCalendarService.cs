using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class RouteCalendarService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "route_calendar.read";

    public const string ScopeDaily = "daily";

    public const string ScopeWeekly = "weekly";

    public const string ScopeCustom = "custom";

    public async Task<RouteCalendarResponse> GetCalendarAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        string? scope,
        string? start,
        string? end,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var (normalizedScope, windowStart, windowEnd) = ResolveWindow(scope, start, end, now);

        var tripsQuery = db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);
        tripsQuery = ApplyTripAccessFilter(tripsQuery, viewAll, actorUserId, actorPersonId);

        var routesQuery = db.Routes
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.TenantId == tenantId);
        routesQuery = ApplyRouteAccessFilter(routesQuery, viewAll, actorUserId, actorPersonId);

        var stopsQuery = db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .ThenInclude(x => x!.Trip)
            .Where(x => x.TenantId == tenantId);
        stopsQuery = ApplyStopAccessFilter(stopsQuery, viewAll, actorUserId, actorPersonId);

        var accessibleTrips = await tripsQuery.ToListAsync(cancellationToken);
        var accessibleRoutes = await routesQuery.ToListAsync(cancellationToken);
        var accessibleStops = await stopsQuery.ToListAsync(cancellationToken);

        var scopedTrips = accessibleTrips
            .Where(x => IsTripInScope(x, windowStart, windowEnd))
            .ToList();

        var scopedRoutes = accessibleRoutes
            .Where(x => IsRouteInScope(x, windowStart, windowEnd))
            .ToList();

        var scopedStops = accessibleStops
            .Where(x => IsStopInScope(x, windowStart, windowEnd))
            .ToList();

        var eventsByDay = RouteCalendarRules
            .EnumerateDays(windowStart, windowEnd)
            .ToDictionary(day => day, _ => new List<RouteCalendarEvent>());

        var tripEvents = scopedTrips
            .Select(trip => MapTripEvent(trip, now))
            .ToList();

        foreach (var calendarEvent in tripEvents)
        {
            foreach (var day in RouteCalendarRules.DaysForEvent(
                         calendarEvent.ScheduledAt,
                         calendarEvent.ScheduledEndAt,
                         windowStart,
                         windowEnd))
            {
                eventsByDay[day].Add(calendarEvent);
            }
        }

        foreach (var route in scopedRoutes)
        {
            var calendarEvent = MapRouteEvent(route);
            foreach (var day in RouteCalendarRules.DaysForEvent(
                         calendarEvent.ScheduledAt,
                         calendarEvent.ScheduledEndAt,
                         windowStart,
                         windowEnd))
            {
                eventsByDay[day].Add(calendarEvent);
            }
        }

        foreach (var stop in scopedStops)
        {
            var calendarEvent = MapStopEvent(stop);
            foreach (var day in RouteCalendarRules.DaysForEvent(
                         calendarEvent.ScheduledAt,
                         calendarEvent.ScheduledEndAt,
                         windowStart,
                         windowEnd))
            {
                eventsByDay[day].Add(calendarEvent);
            }
        }

        var days = eventsByDay
            .OrderBy(x => x.Key)
            .Select(x => new RouteCalendarDay(
                x.Key,
                x.Value
                    .OrderBy(e => e.ScheduledAt)
                    .ThenBy(e => e.Label, StringComparer.OrdinalIgnoreCase)
                    .ToList()))
            .ToList();

        var summary = new RouteCalendarSummary(
            tripEvents.Count,
            scopedRoutes.Count,
            scopedStops.Count,
            tripEvents.Count(x => x.IsLate),
            tripEvents.Count(x => x.IsAtRisk));

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "route_calendar",
            normalizedScope,
            "success",
            cancellationToken: cancellationToken);

        return new RouteCalendarResponse(
            normalizedScope,
            windowStart,
            windowEnd,
            days,
            summary,
            now);
    }

    private static (string Scope, DateTimeOffset Start, DateTimeOffset End) ResolveWindow(
        string? scope,
        string? start,
        string? end,
        DateTimeOffset now)
    {
        if (!string.IsNullOrWhiteSpace(start) || !string.IsNullOrWhiteSpace(end))
        {
            if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
            {
                throw new StlApiException(
                    "route_calendar.invalid_range",
                    "Route calendar start and end must both be provided for a custom range.",
                    400);
            }

            var windowStart = RouteCalendarRules.ParseUtcDate(start);
            var windowEnd = RouteCalendarRules.ParseUtcDate(end);
            if (windowEnd <= windowStart)
            {
                throw new StlApiException(
                    "route_calendar.invalid_range",
                    "Route calendar end must be after start.",
                    400);
            }

            var dayCount = (windowEnd - windowStart).TotalDays;
            if (dayCount > RouteCalendarRules.MaxCustomRangeDays)
            {
                throw new StlApiException(
                    "route_calendar.range_too_large",
                    $"Route calendar range cannot exceed {RouteCalendarRules.MaxCustomRangeDays} days.",
                    400);
            }

            return (ScopeCustom, windowStart, windowEnd);
        }

        var normalizedScope = NormalizeScope(scope);
        var (startAt, endAt) = GetScopeWindow(normalizedScope, now);
        return (normalizedScope, startAt, endAt);
    }

    private static string NormalizeScope(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return ScopeDaily;
        }

        var normalized = scope.Trim().ToLowerInvariant();
        if (normalized is ScopeDaily or ScopeWeekly)
        {
            return normalized;
        }

        throw new StlApiException(
            "route_calendar.invalid_scope",
            "Route calendar scope must be daily or weekly.",
            400);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetScopeWindow(string scope, DateTimeOffset now)
    {
        var dayStart = RouteCalendarRules.ToUtcDayStart(now);
        return scope == ScopeWeekly
            ? (dayStart, dayStart.AddDays(7))
            : (dayStart, dayStart.AddDays(1));
    }

    private static bool IsTripInScope(Trip trip, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        TripDispatchStatuses.Active.Contains(trip.DispatchStatus)
        || OverlapsWindow(trip.ScheduledStartAt, trip.ScheduledEndAt, windowStart, windowEnd)
        || (trip.CreatedAt >= windowStart && trip.CreatedAt < windowEnd);

    private static bool IsRouteInScope(DispatchRoute route, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        route.RouteStatus is RouteStatuses.Draft or RouteStatuses.Planned or RouteStatuses.Active
        || (route.CreatedAt >= windowStart && route.CreatedAt < windowEnd)
        || (route.Trip?.ScheduledStartAt is not null
            && OverlapsWindow(route.Trip.ScheduledStartAt, route.Trip.ScheduledEndAt, windowStart, windowEnd));

    private static bool IsStopInScope(RouteStop stop, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        !RouteStopStatuses.Terminal.Contains(stop.StopStatus)
        || OverlapsWindow(stop.ScheduledArrivalAt, stop.ScheduledArrivalAt, windowStart, windowEnd)
        || (stop.CreatedAt >= windowStart && stop.CreatedAt < windowEnd);

    private static bool OverlapsWindow(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd)
    {
        if (startAt.HasValue && startAt.Value >= windowStart && startAt.Value < windowEnd)
        {
            return true;
        }

        if (endAt.HasValue && endAt.Value >= windowStart && endAt.Value < windowEnd)
        {
            return true;
        }

        if (startAt.HasValue && endAt.HasValue
            && startAt.Value < windowEnd
            && endAt.Value >= windowStart)
        {
            return true;
        }

        return false;
    }

    private static RouteCalendarEvent MapTripEvent(Trip trip, DateTimeOffset now)
    {
        var scheduledAt = trip.ScheduledStartAt ?? trip.CreatedAt;
        var isLate = DispatchBoardRules.IsLateTrip(trip, now);
        var isAtRisk = !isLate && DispatchBoardRules.IsAtRiskTrip(trip, now);
        return new RouteCalendarEvent(
            "trip",
            trip.Id,
            trip.Title,
            trip.DispatchStatus,
            scheduledAt,
            trip.ScheduledEndAt,
            trip.Id,
            null,
            trip.TripNumber,
            null,
            trip.AssignedDriverPersonId,
            isLate,
            isAtRisk);
    }

    private static RouteCalendarEvent MapRouteEvent(DispatchRoute route)
    {
        var scheduledAt = route.Trip?.ScheduledStartAt
            ?? route.ActivatedAt
            ?? route.CreatedAt;
        var scheduledEndAt = route.Trip?.ScheduledEndAt;
        return new RouteCalendarEvent(
            "route",
            route.Id,
            route.Title,
            route.RouteStatus,
            scheduledAt,
            scheduledEndAt,
            route.TripId,
            route.Id,
            route.Trip?.TripNumber,
            route.RouteNumber,
            route.Trip?.AssignedDriverPersonId,
            false,
            false);
    }

    private static RouteCalendarEvent MapStopEvent(RouteStop stop)
    {
        var scheduledAt = stop.ScheduledArrivalAt ?? stop.CreatedAt;
        return new RouteCalendarEvent(
            "stop",
            stop.Id,
            stop.Label,
            stop.StopStatus,
            scheduledAt,
            null,
            stop.Route.TripId,
            stop.RouteId,
            stop.Route.Trip?.TripNumber,
            stop.Route.RouteNumber,
            stop.Route.Trip?.AssignedDriverPersonId,
            false,
            false);
    }

    private static IQueryable<Trip> ApplyTripAccessFilter(
        IQueryable<Trip> query,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId)
    {
        if (viewAll || !actorUserId.HasValue)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.CreatedByUserId == actorUserId.Value
            || (personId != null
                && x.AssignedDriverPersonId != null
                && x.AssignedDriverPersonId == personId));
    }

    private static IQueryable<DispatchRoute> ApplyRouteAccessFilter(
        IQueryable<DispatchRoute> query,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId)
    {
        if (viewAll || !actorUserId.HasValue)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.CreatedByUserId == actorUserId.Value
            || (x.Trip != null && x.Trip.CreatedByUserId == actorUserId.Value)
            || (personId != null
                && x.Trip != null
                && x.Trip.AssignedDriverPersonId != null
                && x.Trip.AssignedDriverPersonId == personId));
    }

    private static IQueryable<RouteStop> ApplyStopAccessFilter(
        IQueryable<RouteStop> query,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId)
    {
        if (viewAll || !actorUserId.HasValue)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.Route.CreatedByUserId == actorUserId.Value
            || (x.Route.Trip != null && x.Route.Trip.CreatedByUserId == actorUserId.Value)
            || (personId != null
                && x.Route.Trip != null
                && x.Route.Trip.AssignedDriverPersonId != null
                && x.Route.Trip.AssignedDriverPersonId == personId));
    }
}
