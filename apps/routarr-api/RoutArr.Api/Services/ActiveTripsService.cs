using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class ActiveTripsService(
    RoutArrDbContext db,
    DispatchBoardService boardService,
    StaffarrPersonRefService personRefService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "dispatch_active_trips.read";

    public async Task<ActiveTripsResponse> GetAsync(
        ClaimsPrincipal principal,
        string? scope,
        bool attentionOnly,
        string? statusFilter,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchBoardRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var viewAll = authorization.CanViewAllTrips(principal);
        var actorPersonId = principal.GetPersonId().ToString();
        var normalizedStatusFilter = ActiveTripsFilterRules.NormalizeStatusFilter(statusFilter);

        var board = await boardService.GetBoardAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            scope,
            cancellationToken);

        var activeIds = board.ActiveTrips.Select(x => x.TripId).ToList();
        var tripsById = activeIds.Count == 0
            ? new Dictionary<Guid, Trip>()
            : await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && activeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var driverRefs = await personRefService.ListAsync(tenantId, cancellationToken);
        var driverNames = driverRefs.Items.ToDictionary(x => x.PersonId, x => x.DisplayName);

        var stopMetrics = await LoadStopMetricsAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            board.WindowStart,
            board.WindowEnd,
            activeIds,
            cancellationToken);

        var exceptionCounts = await LoadOpenExceptionCountsAsync(
            tenantId,
            activeIds,
            cancellationToken);

        var windowSpanMs = Math.Max(
            (board.WindowEnd - board.WindowStart).TotalMilliseconds,
            1);

        var allItems = board.ActiveTrips
            .Select(row =>
            {
                tripsById.TryGetValue(row.TripId, out var trip);
                var (offset, width) = ActiveTripsTimelineRules.ComputeTimelinePosition(
                    row.ScheduledStartAt,
                    row.ScheduledEndAt,
                    board.WindowStart,
                    board.WindowEnd,
                    windowSpanMs);

                stopMetrics.TryGetValue(row.TripId, out var stops);
                var (completedStopCount, totalStopCount, stopProgressPercent) =
                    ActiveTripsProgressRules.ComputeStopProgress(
                        stops.Completed,
                        stops.Total);

                string? driverDisplayName = null;
                if (!string.IsNullOrWhiteSpace(row.AssignedDriverPersonId))
                {
                    driverNames.TryGetValue(row.AssignedDriverPersonId, out driverDisplayName);
                    driverDisplayName ??= row.AssignedDriverPersonId;
                }

                return new ActiveTripRow(
                    row.TripId,
                    row.TripNumber,
                    row.Title,
                    row.DispatchStatus,
                    row.AssignedDriverPersonId,
                    driverDisplayName,
                    trip?.VehicleRefKey,
                    row.ScheduledStartAt,
                    row.ScheduledEndAt,
                    trip?.DispatchedAt,
                    trip?.StartedAt,
                    row.IsLate,
                    row.IsAtRisk,
                    row.RouteCount,
                    row.PendingStopCount,
                    completedStopCount,
                    totalStopCount,
                    stopProgressPercent,
                    exceptionCounts.GetValueOrDefault(row.TripId),
                    offset,
                    width);
            })
            .ToList();

        var items = allItems
            .Where(x => ActiveTripsFilterRules.MatchesStatusFilter(x.DispatchStatus, normalizedStatusFilter))
            .Where(x => ActiveTripsFilterRules.MatchesAttentionFilter(x.IsLate, x.IsAtRisk, attentionOnly))
            .ToList();

        var summary = new ActiveTripsSummary(
            items.Count,
            items.Count(x => x.IsLate),
            items.Count(x => x.IsAtRisk),
            items.Count(x => string.Equals(
                x.DispatchStatus,
                TripDispatchStatuses.Dispatched,
                StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.Equals(
                x.DispatchStatus,
                TripDispatchStatuses.InProgress,
                StringComparison.OrdinalIgnoreCase)),
            items.Count(x => string.IsNullOrWhiteSpace(x.AssignedDriverPersonId)),
            items.Sum(x => x.OpenExceptionCount));

        var auditDetail = attentionOnly
            ? "attention"
            : normalizedStatusFilter == ActiveTripsFilterRules.StatusAll
                ? board.Scope
                : normalizedStatusFilter;

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "dispatch_active_trips",
            auditDetail,
            items.Count.ToString(),
            cancellationToken: cancellationToken);

        return new ActiveTripsResponse(
            board.Scope,
            board.WindowStart,
            board.WindowEnd,
            summary,
            items,
            board.GeneratedAt);
    }

    private async Task<Dictionary<Guid, (int Completed, int Total)>> LoadStopMetricsAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        IReadOnlyList<Guid> tripIds,
        CancellationToken cancellationToken)
    {
        if (tripIds.Count == 0)
        {
            return new Dictionary<Guid, (int Completed, int Total)>();
        }

        var stopsQuery = db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .ThenInclude(x => x!.Trip)
            .Where(x => x.TenantId == tenantId
                && x.Route.TripId.HasValue
                && tripIds.Contains(x.Route.TripId.Value));

        if (!viewAll)
        {
            var personId = actorPersonId?.Trim();
            stopsQuery = stopsQuery.Where(x =>
                x.Route.CreatedByUserId == actorUserId
                || (x.Route.Trip != null && x.Route.Trip.CreatedByUserId == actorUserId)
                || (personId != null
                    && x.Route.Trip != null
                    && x.Route.Trip.AssignedDriverPersonId != null
                    && x.Route.Trip.AssignedDriverPersonId == personId));
        }

        var stops = await stopsQuery.ToListAsync(cancellationToken);
        return stops
            .Where(x => x.Route.TripId.HasValue)
            .GroupBy(x => x.Route.TripId!.Value)
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    var total = x.Count();
                    var completed = x.Count(s => ActiveTripsProgressRules.IsCompletedStop(s.StopStatus));
                    return (completed, total);
                });
    }

    private async Task<Dictionary<Guid, int>> LoadOpenExceptionCountsAsync(
        Guid tenantId,
        IReadOnlyList<Guid> tripIds,
        CancellationToken cancellationToken)
    {
        if (tripIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await db.DispatchExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.TripId.HasValue
                && tripIds.Contains(x.TripId.Value))
            .WhereDispatchExceptionOpenQueue()
            .GroupBy(x => x.TripId!.Value)
            .Select(x => new { TripId = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.TripId, x => x.Count);
    }
}
