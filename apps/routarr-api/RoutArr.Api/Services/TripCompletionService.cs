using Microsoft.EntityFrameworkCore;

using RoutArr.Api.Contracts;

using RoutArr.Api.Data;

using RoutArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace RoutArr.Api.Services;



public sealed class TripCompletionService(RoutArrDbContext db)

{

    private const int MaxPageSize = 100;



    public async Task<IReadOnlyList<TripCompletionSummaryResponse>> ListAsync(

        Guid tenantId,

        bool viewAll,

        Guid? actorUserId,

        string? actorPersonId,

        string? dispatchStatus = null,

        CancellationToken cancellationToken = default)

    {

        var asOf = DateTimeOffset.UtcNow;

        var query = db.TripCompletionRollups.AsNoTracking()

            .Where(x => x.TenantId == tenantId);



        if (!string.IsNullOrWhiteSpace(dispatchStatus))

        {

            query = query.Where(x => x.DispatchStatus == dispatchStatus);

        }



        var rollups = await query

            .OrderByDescending(x => x.CompletedAt ?? x.CancelledAt ?? x.ComputedAt)

            .ToListAsync(cancellationToken);



        if (rollups.Count == 0)

        {

            return await ListLiveTerminalTripsAsync(

                tenantId,

                viewAll,

                actorUserId,

                actorPersonId,

                dispatchStatus,

                asOf,

                cancellationToken);

        }



        var filtered = new List<TripCompletionSummaryResponse>();

        foreach (var rollup in rollups)

        {

            if (!await CanAccessTripAsync(

                    tenantId,

                    rollup.TripId,

                    viewAll,

                    actorUserId,

                    actorPersonId,

                    cancellationToken))

            {

                continue;

            }



            var isMaterialized = !TripCompletionRollupRules.IsStale(

                rollup.ComputedAt,

                asOf,

                TripCompletionRollupRules.DefaultReadStalenessHours);

            filtered.Add(TripCompletionRollupWorkerService.MapSummary(rollup, isMaterialized));

        }



        return filtered;

    }



    public async Task<TripCompletionDetailResponse> GetAsync(

        Guid tenantId,

        Guid tripId,

        CancellationToken cancellationToken = default)

    {

        var asOf = DateTimeOffset.UtcNow;

        var rollup = await db.TripCompletionRollups.AsNoTracking()

            .Include(x => x.Events)

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TripId == tripId, cancellationToken);



        if (rollup is not null

            && !TripCompletionRollupRules.IsStale(

                rollup.ComputedAt,

                asOf,

                TripCompletionRollupRules.DefaultReadStalenessHours))

        {

            return new TripCompletionDetailResponse(

                TripCompletionRollupWorkerService.MapSummary(rollup, isMaterialized: true),

                rollup.Events

                    .OrderBy(x => x.SequenceNumber)

                    .Select(TripCompletionRollupWorkerService.MapEvent)

                    .ToList());

        }



        return await BuildLiveDetailAsync(tenantId, tripId, asOf, cancellationToken);

    }



    public async Task<IReadOnlyList<RouteCompletionSummaryResponse>> ListRouteCompletionsAsync(

        Guid tenantId,

        bool viewAll,

        Guid? actorUserId,

        string? actorPersonId,

        CancellationToken cancellationToken = default)

    {

        var asOf = DateTimeOffset.UtcNow;

        var terminalStatuses = TripTerminalDispatchStatuses.All.ToList();

        var routes = await db.Routes.AsNoTracking()

            .Include(x => x.Trip)

            .Include(x => x.Stops)

            .Where(x => x.TenantId == tenantId

                && x.TripId != null

                && x.Trip != null

                && terminalStatuses.Contains(x.Trip.DispatchStatus))

            .OrderByDescending(x => x.CompletedAt ?? x.UpdatedAt)

            .ToListAsync(cancellationToken);



        var rollupLookup = await db.TripCompletionRollups.AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .ToDictionaryAsync(x => x.TripId, cancellationToken);



        var results = new List<RouteCompletionSummaryResponse>();

        foreach (var route in routes)

        {

            if (route.Trip is null || !await CanAccessTripAsync(

                    tenantId,

                    route.Trip.Id,

                    viewAll,

                    actorUserId,

                    actorPersonId,

                    cancellationToken))

            {

                continue;

            }



            rollupLookup.TryGetValue(route.Trip.Id, out var rollup);

            var isMaterialized = rollup is not null

                && !TripCompletionRollupRules.IsStale(

                    rollup.ComputedAt,

                    asOf,

                    TripCompletionRollupRules.DefaultReadStalenessHours);



            var stops = route.Stops.ToList();

            results.Add(new RouteCompletionSummaryResponse(

                route.Id,

                route.RouteNumber,

                route.Title,

                route.RouteStatus,

                route.TripId,

                route.Trip.TripNumber,

                route.Trip.DispatchStatus,

                stops.Count,

                stops.Count(x => string.Equals(x.StopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase)),

                stops.Count(x => string.Equals(x.StopStatus, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase)),

                route.CompletedAt,

                rollup?.ComputedAt,

                isMaterialized));

        }



        return results;

    }



    private async Task<IReadOnlyList<TripCompletionSummaryResponse>> ListLiveTerminalTripsAsync(

        Guid tenantId,

        bool viewAll,

        Guid? actorUserId,

        string? actorPersonId,

        string? dispatchStatus,

        DateTimeOffset asOfUtc,

        CancellationToken cancellationToken)

    {

        var terminalStatuses = TripTerminalDispatchStatuses.All.ToList();

        var query = db.Trips.AsNoTracking()

            .Include(x => x.Loads)

            .Where(x => x.TenantId == tenantId && terminalStatuses.Contains(x.DispatchStatus));



        if (!string.IsNullOrWhiteSpace(dispatchStatus))

        {

            query = query.Where(x => x.DispatchStatus == dispatchStatus);

        }



        var trips = await query

            .OrderByDescending(x => x.CompletedAt ?? x.CancelledAt ?? x.UpdatedAt)

            .ToListAsync(cancellationToken);



        var summaries = new List<TripCompletionSummaryResponse>();

        foreach (var trip in trips)

        {

            if (!await CanAccessTripAsync(

                    tenantId,

                    trip.Id,

                    viewAll,

                    actorUserId,

                    actorPersonId,

                    cancellationToken))

            {

                continue;

            }



            var routes = await db.Routes.AsNoTracking()

                .Include(x => x.Stops)

                .Where(x => x.TenantId == tenantId && x.TripId == trip.Id)

                .ToListAsync(cancellationToken);



            summaries.Add(TripCompletionRollupBuilder.Build(trip, routes, asOfUtc).Summary);

        }



        return summaries;

    }



    private async Task<TripCompletionDetailResponse> BuildLiveDetailAsync(

        Guid tenantId,

        Guid tripId,

        DateTimeOffset asOfUtc,

        CancellationToken cancellationToken)

    {

        var trip = await db.Trips.AsNoTracking()

            .Include(x => x.Loads)

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);



        if (trip is null)

        {

            throw new StlApiException("trip.not_found", "Trip was not found.", 404);

        }



        if (!TripCompletionRollupRules.IsTerminalTrip(trip.DispatchStatus))

        {

            throw new StlApiException(

                "trip.not_terminal",

                "Trip completion details are only available for completed or cancelled trips.",

                400);

        }



        var routes = await db.Routes.AsNoTracking()

            .Include(x => x.Stops)

            .Where(x => x.TenantId == tenantId && x.TripId == tripId)

            .ToListAsync(cancellationToken);



        var computation = TripCompletionRollupBuilder.Build(trip, routes, asOfUtc);

        return new TripCompletionDetailResponse(computation.Summary, computation.Events);

    }



    public async Task<Guid> GetTripCreatedByUserIdAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var createdByUserId = await db.Trips.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == tripId)
            .Select(x => (Guid?)x.CreatedByUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdByUserId is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        return createdByUserId.Value;
    }

    private async Task<bool> CanAccessTripAsync(

        Guid tenantId,

        Guid tripId,

        bool viewAll,

        Guid? actorUserId,

        string? actorPersonId,

        CancellationToken cancellationToken)

    {

        if (viewAll)

        {

            return true;

        }



        var trip = await db.Trips.AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.Id == tripId)

            .Select(x => new { x.CreatedByUserId, x.AssignedDriverPersonId })

            .FirstOrDefaultAsync(cancellationToken);



        if (trip is null)

        {

            return false;

        }



        if (actorUserId.HasValue && trip.CreatedByUserId == actorUserId.Value)

        {

            return true;

        }



        return !string.IsNullOrWhiteSpace(trip.AssignedDriverPersonId)

            && !string.IsNullOrWhiteSpace(actorPersonId)

            && string.Equals(trip.AssignedDriverPersonId, actorPersonId, StringComparison.Ordinal);

    }

}


