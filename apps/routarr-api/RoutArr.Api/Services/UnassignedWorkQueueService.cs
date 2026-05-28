using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class UnassignedWorkQueueService(
    RoutArrDbContext db,
    DispatchBoardService boardService,
    StaffarrPersonRefService personRefService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "dispatch_unassigned_work_queue.read";

    public async Task<UnassignedWorkQueueResponse> GetAsync(
        ClaimsPrincipal principal,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchBoardRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var viewAll = authorization.CanViewAllTrips(principal);
        var actorPersonId = principal.GetPersonId().ToString();

        var board = await boardService.GetBoardAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            scope,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var items = await LoadUnassignedTripsAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            board.WindowStart,
            board.WindowEnd,
            now,
            cancellationToken);

        var driverRefs = await personRefService.ListAsync(tenantId, cancellationToken);

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "dispatch_unassigned_work_queue",
            board.Scope,
            items.Count.ToString(),
            cancellationToken: cancellationToken);

        return new UnassignedWorkQueueResponse(
            board.Scope,
            board.WindowStart,
            board.WindowEnd,
            board.WorkQueue.UnassignedDriverTripCount,
            items,
            driverRefs,
            now);
    }

    private async Task<IReadOnlyList<UnassignedWorkQueueTripRow>> LoadUnassignedTripsAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        string actorPersonId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tripsQuery = db.Trips.AsNoTracking().Where(x => x.TenantId == tenantId);
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

        var scopedRoutes = accessibleRoutes
            .Where(x => IsRouteInScope(x, windowStart, windowEnd))
            .ToList();

        var scopedStops = accessibleStops
            .Where(x => IsStopInScope(x, windowStart, windowEnd))
            .ToList();

        var routeCountByTrip = scopedRoutes
            .Where(x => x.TripId.HasValue)
            .GroupBy(x => x.TripId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        var pendingStopCountByTrip = scopedStops
            .Where(x => x.Route.TripId.HasValue
                && string.Equals(x.StopStatus, RouteStopStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.Route.TripId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        return accessibleTrips
            .Where(x => IsUnassignedActiveTrip(x, windowStart, windowEnd))
            .Select(trip =>
            {
                var isLate = DispatchBoardRules.IsLateTrip(trip, now);
                var isAtRisk = !isLate && DispatchBoardRules.IsAtRiskTrip(trip, now);
                return new UnassignedWorkQueueTripRow(
                    trip.Id,
                    trip.TripNumber,
                    trip.Title,
                    trip.DispatchStatus,
                    trip.ScheduledStartAt,
                    trip.ScheduledEndAt,
                    isLate,
                    isAtRisk,
                    routeCountByTrip.GetValueOrDefault(trip.Id),
                    pendingStopCountByTrip.GetValueOrDefault(trip.Id));
            })
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToList();
    }

    private static bool IsUnassignedActiveTrip(
        Trip trip,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd) =>
        IsTripInScope(trip, windowStart, windowEnd)
        && TripDispatchStatuses.Active.Contains(trip.DispatchStatus)
        && string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId);

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

    private static IQueryable<Trip> ApplyTripAccessFilter(
        IQueryable<Trip> query,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId)
    {
        if (viewAll)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.CreatedByUserId == actorUserId
            || (personId != null
                && x.AssignedDriverPersonId != null
                && x.AssignedDriverPersonId == personId));
    }

    private static IQueryable<DispatchRoute> ApplyRouteAccessFilter(
        IQueryable<DispatchRoute> query,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId)
    {
        if (viewAll)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.CreatedByUserId == actorUserId
            || (x.Trip != null && x.Trip.CreatedByUserId == actorUserId)
            || (personId != null
                && x.Trip != null
                && x.Trip.AssignedDriverPersonId != null
                && x.Trip.AssignedDriverPersonId == personId));
    }

    private static IQueryable<RouteStop> ApplyStopAccessFilter(
        IQueryable<RouteStop> query,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId)
    {
        if (viewAll)
        {
            return query;
        }

        var personId = actorPersonId?.Trim();
        return query.Where(x =>
            x.Route.CreatedByUserId == actorUserId
            || (x.Route.Trip != null && x.Route.Trip.CreatedByUserId == actorUserId)
            || (personId != null
                && x.Route.Trip != null
                && x.Route.Trip.AssignedDriverPersonId != null
                && x.Route.Trip.AssignedDriverPersonId == personId));
    }
}
