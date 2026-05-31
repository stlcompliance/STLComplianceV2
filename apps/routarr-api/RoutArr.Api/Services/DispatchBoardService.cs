using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchBoardService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "dispatch_board.read";

    public const string ScopeDaily = "daily";

    public const string ScopeWeekly = "weekly";

    public async Task<DispatchBoardResponse> GetBoardAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedScope = NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = GetWindow(normalizedScope, now);

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

        var settingsEntity = await db.TenantTripExecutionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        var captureSettings = TripExecutionCaptureRules.ResolveSettings(settingsEntity);

        var scopedTripIds = scopedTrips.Select(x => x.Id).ToHashSet();
        var proofTypesByTrip = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && scopedTripIds.Contains(x.TripId))
            .Select(x => new { x.TripId, x.ProofType })
            .ToListAsync(cancellationToken);
        var proofLookup = proofTypesByTrip
            .GroupBy(x => x.TripId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.ProofType).ToHashSet(StringComparer.OrdinalIgnoreCase));

        var missingRequiredProofByTrip = scopedTrips.ToDictionary(
            x => x.Id,
            x => DispatchBoardRules.CountMissingRequiredProof(x, captureSettings, proofLookup.GetValueOrDefault(x.Id)));

        var routeCountByTrip = scopedRoutes
            .Where(x => x.TripId.HasValue)
            .GroupBy(x => x.TripId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        var pendingStopCountByTrip = scopedStops
            .Where(x => x.Route.TripId.HasValue
                && string.Equals(x.StopStatus, RouteStopStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.Route.TripId!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        var tripRows = scopedTrips
            .Select(trip => MapTripRow(
                trip,
                routeCountByTrip.GetValueOrDefault(trip.Id),
                pendingStopCountByTrip.GetValueOrDefault(trip.Id),
                missingRequiredProofByTrip.GetValueOrDefault(trip.Id),
                now))
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToList();

        var tripsSummary = new DispatchBoardTripsSummary(
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.Planned)),
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.Assigned)),
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.Dispatched)),
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.InProgress)),
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.Completed)),
            scopedTrips.Count(x => StatusEquals(x.DispatchStatus, TripDispatchStatuses.Cancelled)),
            scopedTrips.Count,
            tripRows.Count(x => x.IsLate),
            tripRows.Count(x => x.IsAtRisk));

        var routesSummary = new DispatchBoardRoutesSummary(
            scopedRoutes.Count(x => StatusEquals(x.RouteStatus, RouteStatuses.Draft)),
            scopedRoutes.Count(x => StatusEquals(x.RouteStatus, RouteStatuses.Planned)),
            scopedRoutes.Count(x => StatusEquals(x.RouteStatus, RouteStatuses.Active)),
            scopedRoutes.Count(x => StatusEquals(x.RouteStatus, RouteStatuses.Completed)),
            scopedRoutes.Count(x => StatusEquals(x.RouteStatus, RouteStatuses.Cancelled)),
            scopedRoutes.Count);

        var stopsSummary = new DispatchBoardStopsSummary(
            scopedStops.Count(x => StatusEquals(x.StopStatus, RouteStopStatuses.Pending)),
            scopedStops.Count(x => StatusEquals(x.StopStatus, RouteStopStatuses.Arrived)),
            scopedStops.Count(x => StatusEquals(x.StopStatus, RouteStopStatuses.Completed)),
            scopedStops.Count(x => StatusEquals(x.StopStatus, RouteStopStatuses.Skipped)),
            scopedStops.Count);

        var workQueue = new DispatchBoardWorkQueueSummary(
            scopedTrips.Count(x =>
                TripDispatchStatuses.Active.Contains(x.DispatchStatus)
                && string.IsNullOrWhiteSpace(x.AssignedDriverPersonId)),
            scopedRoutes.Count(x => !x.TripId.HasValue && RouteStatuses.Editable.Contains(x.RouteStatus)),
            scopedStops.Count(x => StatusEquals(x.StopStatus, RouteStopStatuses.Pending)),
            missingRequiredProofByTrip.Count(x => x.Value > 0));

        var assignedTrips = tripRows
            .Where(x => !string.IsNullOrWhiteSpace(x.AssignedDriverPersonId))
            .ToList();

        var activeTrips = tripRows
            .Where(x => x.DispatchStatus is TripDispatchStatuses.Dispatched or TripDispatchStatuses.InProgress)
            .ToList();

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "dispatch_board",
            normalizedScope,
            "success",
            cancellationToken: cancellationToken);

        return new DispatchBoardResponse(
            normalizedScope,
            windowStart,
            windowEnd,
            tripsSummary,
            routesSummary,
            stopsSummary,
            workQueue,
            assignedTrips,
            activeTrips,
            now);
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
            "dispatch_board.invalid_scope",
            "Dispatch board scope must be daily or weekly.",
            400);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetWindow(string scope, DateTimeOffset now)
    {
        var dayStart = new DateTimeOffset(now.UtcDateTime.Date, TimeSpan.Zero);
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

    private static DispatchBoardTripRow MapTripRow(
        Trip trip,
        int routeCount,
        int pendingStopCount,
        int missingRequiredProofCount,
        DateTimeOffset now)
    {
        var isLate = DispatchBoardRules.IsLateTrip(trip, now);
        var isAtRisk = !isLate && DispatchBoardRules.IsAtRiskTrip(trip, now);
        return new DispatchBoardTripRow(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.AssignedDriverPersonId,
            trip.VehicleRefKey,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt,
            isLate,
            isAtRisk,
            routeCount,
            pendingStopCount,
            missingRequiredProofCount);
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

    private static bool StatusEquals(string actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}
