using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchCloseoutService(
    RoutArrDbContext db,
    TripService tripService,
    IRoutArrAuditService audit)
{
    public const string SummaryAction = "dispatch_closeout.summary";

    public const string PreviewAction = "dispatch_closeout.preview";

    public const string ApplyAction = "dispatch_closeout.apply";

    public async Task<DispatchCloseoutSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        var context = await LoadScopeContextAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            scope,
            cancellationToken);

        var openTrips = context.ScopedTrips
            .Where(x => TripDispatchStatuses.Active.Contains(x.DispatchStatus))
            .OrderBy(x => x.ScheduledStartAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.TripNumber)
            .ToList();

        var openRoutes = context.ScopedRoutes
            .Where(x => DispatchCloseoutRules.OpenRouteStatuses.Contains(x.RouteStatus))
            .OrderBy(x => x.RouteNumber)
            .ToList();

        var openStops = context.ScopedStops
            .Where(x => !RouteStopStatuses.Terminal.Contains(x.StopStatus))
            .ToList();

        var openStopCountByRoute = openStops
            .GroupBy(x => x.RouteId)
            .ToDictionary(x => x.Key, x => x.Count());

        return new DispatchCloseoutSummaryResponse(
            context.Scope,
            context.WindowStart,
            context.WindowEnd,
            new DispatchCloseoutCountsSummary(
                openTrips.Count,
                openRoutes.Count,
                openStops.Count,
                context.ScopedTrips.Count,
                context.ScopedRoutes.Count),
            BuildTripsSummary(context.ScopedTrips),
            BuildRoutesSummary(context.ScopedRoutes),
            BuildStopsSummary(context.ScopedStops),
            openTrips.Select(trip => new DispatchCloseoutTripRow(
                trip.Id,
                trip.TripNumber,
                trip.Title,
                trip.DispatchStatus,
                trip.AssignedDriverPersonId)).ToList(),
            openRoutes.Select(route => new DispatchCloseoutRouteRow(
                route.Id,
                route.RouteNumber,
                route.Title,
                route.RouteStatus,
                route.TripId,
                openStopCountByRoute.GetValueOrDefault(route.Id))).ToList());
    }

    public async Task<DispatchCloseoutPreviewResponse> PreviewAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        DispatchCloseoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var tripDisposition = DispatchCloseoutRules.NormalizeTripDisposition(request.RemainingTripDisposition);
        var stopDisposition = DispatchCloseoutRules.NormalizeStopDisposition(request.OpenStopDisposition);
        var context = await LoadScopeContextAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            request.Scope,
            cancellationToken);

        var plans = BuildPlans(context, tripDisposition, stopDisposition);
        return ToPreviewResponse(context, tripDisposition, stopDisposition, plans);
    }

    public async Task<DispatchCloseoutApplyResponse> ApplyAsync(
        Guid tenantId,
        Guid actorUserId,
        string? actorPersonId,
        bool viewAll,
        DispatchCloseoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var tripDisposition = DispatchCloseoutRules.NormalizeTripDisposition(request.RemainingTripDisposition);
        var stopDisposition = DispatchCloseoutRules.NormalizeStopDisposition(request.OpenStopDisposition);
        var context = await LoadScopeContextAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            request.Scope,
            cancellationToken);

        var plans = BuildPlans(context, tripDisposition, stopDisposition);
        var preview = ToPreviewResponse(context, tripDisposition, stopDisposition, plans);

        if (string.Equals(tripDisposition, DispatchCloseoutRules.TripDispositionCancel, StringComparison.OrdinalIgnoreCase)
            && preview.Summary.TripsBlocked > 0)
        {
            // still attempt applicable trips
        }

        var stopResults = new List<DispatchCloseoutStopApplyResult>();
        foreach (var stopAction in plans.StopActions.Where(x => x.Plan.CanApply).OrderBy(x => x.SequenceNumber))
        {
            try
            {
                var stop = await db.RouteStops
                    .Include(x => x.Route)
                    .FirstAsync(x => x.TenantId == tenantId && x.Id == stopAction.StopId, cancellationToken);

                var now = DateTimeOffset.UtcNow;
                stop.StopStatus = stopAction.TargetStopStatus;
                stop.UpdatedAt = now;

                if (string.Equals(stopAction.TargetStopStatus, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(stopAction.TargetStopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    stop.CompletedAt ??= now;
                }

                if (string.Equals(stopAction.TargetStopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    stop.ArrivedAt ??= now;
                }

                stop.Route.UpdatedAt = now;
                if (string.Equals(stop.Route.RouteStatus, RouteStatuses.Planned, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(stopAction.TargetStopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    stop.Route.RouteStatus = RouteStatuses.Active;
                    stop.Route.ActivatedAt ??= now;
                }

                await db.SaveChangesAsync(cancellationToken);

                await audit.WriteAsync(
                    "route_stop.closeout",
                    tenantId,
                    actorUserId,
                    "route_stop",
                    stop.Id.ToString(),
                    stopAction.TargetStopStatus,
                    cancellationToken: cancellationToken);

                stopResults.Add(new DispatchCloseoutStopApplyResult(
                    stop.Id,
                    Applied: true,
                    stop.StopStatus,
                    null,
                    null));
            }
            catch (StlApiException ex)
            {
                stopResults.Add(new DispatchCloseoutStopApplyResult(
                    stopAction.StopId,
                    Applied: false,
                    null,
                    ex.Code,
                    ex.Message));
            }
        }

        var routeResults = new List<DispatchCloseoutRouteApplyResult>();
        foreach (var routeAction in plans.RouteActions.Where(x => x.Plan.CanApply))
        {
            try
            {
                var route = await db.Routes
                    .Include(x => x.Stops)
                    .FirstAsync(x => x.TenantId == tenantId && x.Id == routeAction.RouteId, cancellationToken);

                var now = DateTimeOffset.UtcNow;
                route.RouteStatus = routeAction.TargetRouteStatus;
                route.UpdatedAt = now;

                if (string.Equals(routeAction.TargetRouteStatus, RouteStatuses.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    route.CompletedAt ??= now;
                }

                if (string.Equals(routeAction.TargetRouteStatus, RouteStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
                {
                    route.CancelledAt ??= now;
                }

                await db.SaveChangesAsync(cancellationToken);

                await audit.WriteAsync(
                    "route.closeout",
                    tenantId,
                    actorUserId,
                    "route",
                    route.Id.ToString(),
                    routeAction.TargetRouteStatus,
                    cancellationToken: cancellationToken);

                routeResults.Add(new DispatchCloseoutRouteApplyResult(
                    route.Id,
                    Applied: true,
                    route.RouteStatus,
                    null,
                    null));
            }
            catch (StlApiException ex)
            {
                routeResults.Add(new DispatchCloseoutRouteApplyResult(
                    routeAction.RouteId,
                    Applied: false,
                    null,
                    ex.Code,
                    ex.Message));
            }
        }

        var tripResults = new List<DispatchCloseoutTripApplyResult>();
        foreach (var tripAction in plans.TripActions.Where(x => x.Plan.CanApply))
        {
            try
            {
                string? finalStatus = null;
                foreach (var step in tripAction.Plan.TransitionSteps)
                {
                    var updated = await tripService.UpdateDispatchStatusAsync(
                        tenantId,
                        actorUserId,
                        tripAction.TripId,
                        new UpdateTripDispatchStatusRequest(step),
                        canManageAny: true,
                        actorPersonId,
                        cancellationToken);
                    finalStatus = updated.DispatchStatus;
                }

                tripResults.Add(new DispatchCloseoutTripApplyResult(
                    tripAction.TripId,
                    Applied: true,
                    finalStatus,
                    null,
                    null));
            }
            catch (StlApiException ex)
            {
                tripResults.Add(new DispatchCloseoutTripApplyResult(
                    tripAction.TripId,
                    Applied: false,
                    null,
                    ex.Code,
                    ex.Message));
            }
        }

        var summary = new DispatchCloseoutApplySummary(
            preview.Summary.TripCount,
            tripResults.Count(x => x.Applied),
            tripResults.Count(x => !x.Applied),
            preview.Summary.StopCount,
            stopResults.Count(x => x.Applied),
            stopResults.Count(x => !x.Applied),
            preview.Summary.RouteCount,
            routeResults.Count(x => x.Applied),
            routeResults.Count(x => !x.Applied));

        await audit.WriteAsync(
            ApplyAction,
            tenantId,
            actorUserId,
            "dispatch_closeout",
            context.Scope,
            $"{summary.TripsCanApply}/{summary.TripCount} trips, {summary.StopsCanApply}/{summary.StopCount} stops",
            cancellationToken: cancellationToken);

        return new DispatchCloseoutApplyResponse(
            context.Scope,
            context.WindowStart,
            context.WindowEnd,
            summary,
            tripResults,
            stopResults,
            routeResults);
    }

    private static CloseoutPlans BuildPlans(
        CloseoutScopeContext context,
        string tripDisposition,
        string stopDisposition)
    {
        var openTrips = context.ScopedTrips
            .Where(x => TripDispatchStatuses.Active.Contains(x.DispatchStatus))
            .ToList();

        var openStops = context.ScopedStops
            .Where(x => !RouteStopStatuses.Terminal.Contains(x.StopStatus))
            .OrderBy(x => x.RouteId)
            .ThenBy(x => x.SequenceNumber)
            .ToList();

        var stopActions = openStops
            .Select(stop =>
            {
                var plan = DispatchCloseoutRules.PlanStop(stop, stopDisposition);
                return new CloseoutStopAction(
                    stop.Id,
                    stop.RouteId,
                    stop.StopKey,
                    stop.SequenceNumber,
                    plan);
            })
            .ToList();

        var stopsByRoute = context.ScopedStops
            .GroupBy(x => x.RouteId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var openRoutes = context.ScopedRoutes
            .Where(x => DispatchCloseoutRules.OpenRouteStatuses.Contains(x.RouteStatus))
            .ToList();

        var routeActions = openRoutes
            .Select(route =>
            {
                var routeStops = stopsByRoute.GetValueOrDefault(route.Id) ?? [];
                var simulatedTerminal = routeStops.All(stop =>
                {
                    if (RouteStopStatuses.Terminal.Contains(stop.StopStatus))
                    {
                        return true;
                    }

                    var action = stopActions.FirstOrDefault(x => x.StopId == stop.Id);
                    return action?.Plan.CanApply == true;
                });

                var plan = DispatchCloseoutRules.PlanRoute(route, tripDisposition, simulatedTerminal);
                return new CloseoutRouteAction(route.Id, route.RouteNumber, plan);
            })
            .ToList();

        var tripActions = openTrips
            .Select(trip =>
            {
                var plan = DispatchCloseoutRules.PlanTrip(trip, tripDisposition);
                return new CloseoutTripAction(trip.Id, trip.TripNumber, plan);
            })
            .ToList();

        return new CloseoutPlans(tripActions, stopActions, routeActions);
    }

    private static DispatchCloseoutPreviewResponse ToPreviewResponse(
        CloseoutScopeContext context,
        string tripDisposition,
        string stopDisposition,
        CloseoutPlans plans)
    {
        var tripPreviews = plans.TripActions
            .Select(action => new DispatchCloseoutTripActionPreview(
                action.TripId,
                action.TripNumber,
                action.Plan.CurrentStatus,
                action.Plan.TargetStatus,
                action.Plan.CanApply,
                action.Plan.BlockCode,
                action.Plan.BlockMessage,
                action.Plan.TransitionSteps))
            .ToList();

        var stopPreviews = plans.StopActions
            .Select(action => new DispatchCloseoutStopActionPreview(
                action.StopId,
                action.RouteId,
                action.StopKey,
                action.Plan.CurrentStatus,
                action.Plan.TargetStatus,
                action.Plan.CanApply,
                action.Plan.BlockCode,
                action.Plan.BlockMessage))
            .ToList();

        var routePreviews = plans.RouteActions
            .Select(action => new DispatchCloseoutRouteActionPreview(
                action.RouteId,
                action.RouteNumber,
                action.Plan.CurrentStatus,
                action.Plan.TargetStatus,
                action.Plan.CanApply,
                action.Plan.BlockCode,
                action.Plan.BlockMessage))
            .ToList();

        var summary = new DispatchCloseoutApplySummary(
            tripPreviews.Count,
            tripPreviews.Count(x => x.CanApply),
            tripPreviews.Count(x => !x.CanApply),
            stopPreviews.Count,
            stopPreviews.Count(x => x.CanApply),
            stopPreviews.Count(x => !x.CanApply),
            routePreviews.Count,
            routePreviews.Count(x => x.CanApply),
            routePreviews.Count(x => !x.CanApply));

        return new DispatchCloseoutPreviewResponse(
            context.Scope,
            context.WindowStart,
            context.WindowEnd,
            tripDisposition,
            stopDisposition,
            summary,
            tripPreviews,
            stopPreviews,
            routePreviews);
    }

    private async Task<CloseoutScopeContext> LoadScopeContextAsync(
        Guid tenantId,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        string? scope,
        CancellationToken cancellationToken)
    {
        var normalizedScope = DispatchCloseoutRules.NormalizeScope(scope);
        var now = DateTimeOffset.UtcNow;
        var (windowStart, windowEnd) = DispatchCloseoutRules.GetWindow(normalizedScope, now);

        var tripsQuery = db.Trips.AsNoTracking().Where(x => x.TenantId == tenantId);
        tripsQuery = ApplyTripAccessFilter(tripsQuery, viewAll, actorUserId, actorPersonId);
        var accessibleTrips = await tripsQuery.ToListAsync(cancellationToken);

        var routesQuery = db.Routes
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.TenantId == tenantId);
        routesQuery = ApplyRouteAccessFilter(routesQuery, viewAll, actorUserId, actorPersonId);
        var accessibleRoutes = await routesQuery.ToListAsync(cancellationToken);

        var stopsQuery = db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .ThenInclude(x => x!.Trip)
            .Where(x => x.TenantId == tenantId);
        stopsQuery = ApplyStopAccessFilter(stopsQuery, viewAll, actorUserId, actorPersonId);
        var accessibleStops = await stopsQuery.ToListAsync(cancellationToken);

        return new CloseoutScopeContext(
            normalizedScope,
            windowStart,
            windowEnd,
            accessibleTrips.Where(x => IsTripInScope(x, windowStart, windowEnd)).ToList(),
            accessibleRoutes.Where(x => IsRouteInScope(x, windowStart, windowEnd)).ToList(),
            accessibleStops.Where(x => IsStopInScope(x, windowStart, windowEnd)).ToList());
    }

    private static DispatchCloseoutTripsSummary BuildTripsSummary(IReadOnlyList<Trip> trips) =>
        new(
            CountStatus(trips, TripDispatchStatuses.Planned),
            CountStatus(trips, TripDispatchStatuses.Assigned),
            CountStatus(trips, TripDispatchStatuses.Dispatched),
            CountStatus(trips, TripDispatchStatuses.InProgress),
            CountStatus(trips, TripDispatchStatuses.Completed),
            CountStatus(trips, TripDispatchStatuses.Cancelled));

    private static DispatchCloseoutRoutesSummary BuildRoutesSummary(IReadOnlyList<DispatchRoute> routes) =>
        new(
            CountStatus(routes, RouteStatuses.Draft),
            CountStatus(routes, RouteStatuses.Planned),
            CountStatus(routes, RouteStatuses.Active),
            CountStatus(routes, RouteStatuses.Completed),
            CountStatus(routes, RouteStatuses.Cancelled));

    private static DispatchCloseoutStopsSummary BuildStopsSummary(IReadOnlyList<RouteStop> stops) =>
        new(
            CountStatus(stops, RouteStopStatuses.Pending),
            CountStatus(stops, RouteStopStatuses.Arrived),
            CountStatus(stops, RouteStopStatuses.Completed),
            CountStatus(stops, RouteStopStatuses.Skipped));

    private static int CountStatus<T>(IReadOnlyList<T> items, string status)
        where T : class
    {
        return items.Count(item => StatusEquals(GetStatus(item), status));
    }

    private static string GetStatus<T>(T item) =>
        item switch
        {
            Trip trip => trip.DispatchStatus,
            DispatchRoute route => route.RouteStatus,
            RouteStop stop => stop.StopStatus,
            _ => string.Empty,
        };

    private static bool IsTripInScope(Trip trip, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        TripDispatchStatuses.Active.Contains(trip.DispatchStatus)
        || OverlapsWindow(trip.ScheduledStartAt, trip.ScheduledEndAt, windowStart, windowEnd)
        || (trip.CreatedAt >= windowStart && trip.CreatedAt < windowEnd);

    private static bool IsRouteInScope(DispatchRoute route, DateTimeOffset windowStart, DateTimeOffset windowEnd) =>
        DispatchCloseoutRules.OpenRouteStatuses.Contains(route.RouteStatus)
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

    private sealed record CloseoutScopeContext(
        string Scope,
        DateTimeOffset WindowStart,
        DateTimeOffset WindowEnd,
        IReadOnlyList<Trip> ScopedTrips,
        IReadOnlyList<DispatchRoute> ScopedRoutes,
        IReadOnlyList<RouteStop> ScopedStops);

    private sealed record CloseoutPlans(
        IReadOnlyList<CloseoutTripAction> TripActions,
        IReadOnlyList<CloseoutStopAction> StopActions,
        IReadOnlyList<CloseoutRouteAction> RouteActions);

    private sealed record CloseoutTripAction(Guid TripId, string TripNumber, TripCloseoutPlan Plan);

    private sealed record CloseoutStopAction(
        Guid StopId,
        Guid RouteId,
        string StopKey,
        int SequenceNumber,
        StopCloseoutPlan Plan)
    {
        public string TargetStopStatus => Plan.TargetStopStatus;
    }

    private sealed record CloseoutRouteAction(Guid RouteId, string RouteNumber, RouteCloseoutPlan Plan)
    {
        public string TargetRouteStatus => Plan.TargetRouteStatus;
    }
}
