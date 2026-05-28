using Microsoft.EntityFrameworkCore;

using RoutArr.Api.Contracts;

using RoutArr.Api.Data;

using RoutArr.Api.Entities;



namespace RoutArr.Api.Services;



public sealed class TripCompletionRollupWorkerService(

    RoutArrDbContext db,

    IRoutArrAuditService audit)

{

    public const string ProcessTripCompletionRollupsActionScope = "routarr.trips.completion.rollup";



    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fb");



    public async Task<PendingTripCompletionRollupsResponse> ListPendingAsync(

        Guid? tenantId,

        DateTimeOffset? asOfUtc,

        int? batchSize,

        int? stalenessHours,

        CancellationToken cancellationToken = default)

    {

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;

        var normalizedBatchSize = TripCompletionRollupRules.NormalizeBatchSize(batchSize);

        var normalizedStalenessHours = TripCompletionRollupRules.NormalizeStalenessHours(stalenessHours);

        var candidates = await LoadPendingCandidatesAsync(

            tenantId,

            asOf,

            normalizedStalenessHours,

            normalizedBatchSize,

            cancellationToken);



        var items = candidates

            .Select(x => new PendingTripCompletionRollupItem(

                x.TripId,

                x.TripNumber,

                x.Title,

                x.DispatchStatus,

                x.TripUpdatedAt,

                x.LastComputedAt))

            .ToList();



        return new PendingTripCompletionRollupsResponse(

            asOf,

            normalizedStalenessHours,

            normalizedBatchSize,

            items);

    }



    public async Task<ProcessTripCompletionRollupsResponse> ProcessBatchAsync(

        ProcessTripCompletionRollupsRequest request,

        CancellationToken cancellationToken = default)

    {

        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;

        var batchSize = TripCompletionRollupRules.NormalizeBatchSize(request.BatchSize);

        var stalenessHours = TripCompletionRollupRules.NormalizeStalenessHours(request.StalenessHours);

        var candidates = await LoadPendingCandidatesAsync(

            request.TenantId,

            asOf,

            stalenessHours,

            batchSize,

            cancellationToken);



        var refreshed = new List<TripCompletionSummaryResponse>();

        var skipped = new List<TripCompletionRollupRefreshSkip>();

        var runStats = new Dictionary<Guid, (int Candidates, int Refreshed, int Skipped)>();



        foreach (var candidate in candidates)

        {

            if (!runStats.ContainsKey(candidate.TenantId))

            {

                runStats[candidate.TenantId] = (0, 0, 0);

            }



            var stats = runStats[candidate.TenantId];

            stats.Candidates++;

            runStats[candidate.TenantId] = stats;



            try

            {

                var summary = await RefreshRollupAsync(candidate.TenantId, candidate.TripId, asOf, cancellationToken);

                refreshed.Add(summary);



                stats = runStats[candidate.TenantId];

                stats.Refreshed++;

                runStats[candidate.TenantId] = stats;

            }

            catch (Exception ex) when (ex is not OperationCanceledException)

            {

                skipped.Add(new TripCompletionRollupRefreshSkip(candidate.TripId, ex.Message));

                stats = runStats[candidate.TenantId];

                stats.Skipped++;

                runStats[candidate.TenantId] = stats;

            }

        }



        foreach (var (tenantIdKey, stats) in runStats)

        {

            db.TripCompletionRollupRuns.Add(new TripCompletionRollupRun

            {

                Id = Guid.NewGuid(),

                TenantId = tenantIdKey,

                AsOfUtc = asOf,

                CandidatesFound = stats.Candidates,

                RefreshedCount = stats.Refreshed,

                SkippedCount = stats.Skipped,

                CreatedAt = asOf,

            });

        }



        if (runStats.Count > 0)

        {

            await db.SaveChangesAsync(cancellationToken);

        }



        if (request.TenantId is Guid tenantId && refreshed.Count > 0)

        {

            await audit.WriteAsync(

                "routarr.trip_completion_rollup.batch",

                tenantId,

                WorkerActorUserId,

                "trip_completion_rollup_run",

                $"{refreshed.Count}",

                "success",

                cancellationToken: cancellationToken);

        }



        return new ProcessTripCompletionRollupsResponse(

            asOf,

            batchSize,

            stalenessHours,

            candidates.Count,

            refreshed.Count,

            skipped.Count,

            refreshed,

            skipped);

    }



    public async Task<TripCompletionRollupRunsResponse> ListRecentRunsAsync(

        Guid tenantId,

        int? limit,

        CancellationToken cancellationToken = default)

    {

        var normalizedLimit = TripCompletionRollupRules.NormalizeRunListLimit(limit);

        var runs = await db.TripCompletionRollupRuns

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .OrderByDescending(x => x.CreatedAt)

            .Take(normalizedLimit)

            .Select(x => new TripCompletionRollupRunItem(

                x.Id,

                x.AsOfUtc,

                x.CandidatesFound,

                x.RefreshedCount,

                x.SkippedCount,

                x.CreatedAt))

            .ToListAsync(cancellationToken);



        return new TripCompletionRollupRunsResponse(runs);

    }



    public async Task<TripCompletionSummaryResponse> RefreshRollupAsync(

        Guid tenantId,

        Guid tripId,

        DateTimeOffset asOfUtc,

        CancellationToken cancellationToken)

    {

        var trip = await db.Trips

            .Include(x => x.Loads)

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);



        if (trip is null)

        {

            throw new InvalidOperationException($"Trip {tripId} was not found.");

        }



        if (!TripCompletionRollupRules.IsTerminalTrip(trip.DispatchStatus))

        {

            throw new InvalidOperationException($"Trip {tripId} is not in a terminal dispatch status.");

        }



        var routes = await db.Routes

            .Include(x => x.Stops)

            .Where(x => x.TenantId == tenantId && x.TripId == tripId)

            .ToListAsync(cancellationToken);



        var computation = TripCompletionRollupBuilder.Build(trip, routes, asOfUtc);

        var existing = await db.TripCompletionRollups

            .Include(x => x.Events)

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.TripId == tripId, cancellationToken);



        var now = DateTimeOffset.UtcNow;

        if (existing is null)

        {

            existing = new TripCompletionRollup

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                TripId = tripId,

                CreatedAt = now,

            };

            db.TripCompletionRollups.Add(existing);

        }

        else if (existing.Events.Count > 0)

        {

            db.TripCompletionEvents.RemoveRange(existing.Events);

            existing.Events.Clear();

        }



        var summary = computation.Summary;

        existing.TripNumber = summary.TripNumber;

        existing.Title = summary.Title;

        existing.DispatchStatus = summary.DispatchStatus;

        existing.AssignedDriverPersonId = summary.AssignedDriverPersonId;

        existing.VehicleRefKey = summary.VehicleRefKey;

        existing.ScheduledStartAt = summary.ScheduledStartAt;

        existing.ScheduledEndAt = summary.ScheduledEndAt;

        existing.StartedAt = summary.StartedAt;

        existing.CompletedAt = summary.CompletedAt;

        existing.CancelledAt = summary.CancelledAt;

        existing.DurationMinutes = summary.DurationMinutes;

        existing.RouteCount = summary.RouteCount;

        existing.CompletedRouteCount = summary.CompletedRouteCount;

        existing.StopCount = summary.StopCount;

        existing.CompletedStopCount = summary.CompletedStopCount;

        existing.SkippedStopCount = summary.SkippedStopCount;

        existing.PendingStopCount = summary.PendingStopCount;

        existing.LoadCount = summary.LoadCount;

        existing.DeliveredLoadCount = summary.DeliveredLoadCount;

        existing.PendingLoadCount = summary.PendingLoadCount;

        existing.SourceUpdatedAt = summary.SourceUpdatedAt;

        existing.ComputedAt = asOfUtc;

        existing.UpdatedAt = now;



        foreach (var eventResponse in computation.Events)

        {

            existing.Events.Add(new TripCompletionEvent

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                TripId = tripId,

                RollupId = existing.Id,

                EventKind = eventResponse.EventKind,

                Title = eventResponse.Title,

                Detail = eventResponse.Detail,

                OccurredAt = eventResponse.OccurredAt,

                SequenceNumber = eventResponse.SequenceNumber,

                SourceEntityType = eventResponse.SourceEntityType,

                SourceEntityId = eventResponse.SourceEntityId,

            });

        }



        await db.SaveChangesAsync(cancellationToken);

        return MapSummary(existing, isMaterialized: true);

    }



    internal static TripCompletionSummaryResponse MapSummary(

        TripCompletionRollup rollup,

        bool isMaterialized) =>

        new(

            rollup.TripId,

            rollup.TripNumber,

            rollup.Title,

            rollup.DispatchStatus,

            rollup.AssignedDriverPersonId,

            rollup.VehicleRefKey,

            rollup.ScheduledStartAt,

            rollup.ScheduledEndAt,

            rollup.StartedAt,

            rollup.CompletedAt,

            rollup.CancelledAt,

            rollup.DurationMinutes,

            rollup.RouteCount,

            rollup.CompletedRouteCount,

            rollup.StopCount,

            rollup.CompletedStopCount,

            rollup.SkippedStopCount,

            rollup.PendingStopCount,

            rollup.LoadCount,

            rollup.DeliveredLoadCount,

            rollup.PendingLoadCount,

            rollup.SourceUpdatedAt,

            rollup.ComputedAt,

            isMaterialized);



    internal static TripCompletionEventResponse MapEvent(TripCompletionEvent entity) =>

        new(

            entity.EventKind,

            entity.Title,

            entity.Detail,

            entity.OccurredAt,

            entity.SequenceNumber,

            entity.SourceEntityType,

            entity.SourceEntityId);



    private async Task<IReadOnlyList<PendingTripCandidate>> LoadPendingCandidatesAsync(

        Guid? tenantId,

        DateTimeOffset asOfUtc,

        int stalenessHours,

        int batchSize,

        CancellationToken cancellationToken)

    {

        var enabledTenantIds = await db.TenantTripCompletionRollupSettings

            .AsNoTracking()

            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))

            .Select(x => x.TenantId)

            .ToListAsync(cancellationToken);



        if (enabledTenantIds.Count == 0)

        {

            return [];

        }



        var terminalStatuses = TripTerminalDispatchStatuses.All.ToList();

        var trips = await db.Trips.AsNoTracking()

            .Where(x => enabledTenantIds.Contains(x.TenantId) && terminalStatuses.Contains(x.DispatchStatus))

            .OrderByDescending(x => x.UpdatedAt)

            .Select(x => new PendingTripCandidate(

                x.Id,

                x.TenantId,

                x.TripNumber,

                x.Title,

                x.DispatchStatus,

                x.UpdatedAt,

                null,

                null))

            .ToListAsync(cancellationToken);



        var rollupLookup = await db.TripCompletionRollups.AsNoTracking()

            .Where(x => enabledTenantIds.Contains(x.TenantId))

            .ToDictionaryAsync(

                x => (x.TenantId, x.TripId),

                x => (x.ComputedAt, x.SourceUpdatedAt),

                cancellationToken);



        var pending = new List<PendingTripCandidate>();

        foreach (var trip in trips)
        {
            DateTimeOffset? computedAt = null;
            DateTimeOffset? sourceUpdatedAt = null;
            if (rollupLookup.TryGetValue((trip.TenantId, trip.TripId), out var rollupState))
            {
                computedAt = rollupState.ComputedAt;
                sourceUpdatedAt = rollupState.SourceUpdatedAt;
            }

            if (!TripCompletionRollupRules.IsPending(
                    trip.TripUpdatedAt,
                    sourceUpdatedAt,
                    computedAt,
                    asOfUtc,
                    stalenessHours))
            {
                continue;
            }

            pending.Add(trip with { LastComputedAt = computedAt });

            if (pending.Count >= batchSize)

            {

                break;

            }

        }



        return pending

            .OrderBy(x => x.LastComputedAt.HasValue ? 1 : 0)

            .ThenBy(x => x.LastComputedAt)

            .Take(batchSize)

            .ToList();

    }



    private sealed record PendingTripCandidate(

        Guid TripId,

        Guid TenantId,

        string TripNumber,

        string Title,

        string DispatchStatus,

        DateTimeOffset TripUpdatedAt,

        DateTimeOffset? LastComputedAt,

        DateTimeOffset? SourceUpdatedAt);

}


