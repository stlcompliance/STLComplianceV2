using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class ActiveTripsService(
    RoutArrDbContext db,
    DispatchBoardService boardService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "dispatch_active_trips.read";

    public async Task<ActiveTripsResponse> GetAsync(
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

        var activeIds = board.ActiveTrips.Select(x => x.TripId).ToList();
        var tripsById = activeIds.Count == 0
            ? new Dictionary<Guid, Trip>()
            : await db.Trips
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && activeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var windowSpanMs = Math.Max(
            (board.WindowEnd - board.WindowStart).TotalMilliseconds,
            1);

        var items = board.ActiveTrips
            .Select(row =>
            {
                tripsById.TryGetValue(row.TripId, out var trip);
                var (offset, width) = ActiveTripsTimelineRules.ComputeTimelinePosition(
                    row.ScheduledStartAt,
                    row.ScheduledEndAt,
                    board.WindowStart,
                    board.WindowEnd,
                    windowSpanMs);

                return new ActiveTripRow(
                    row.TripId,
                    row.TripNumber,
                    row.Title,
                    row.DispatchStatus,
                    row.AssignedDriverPersonId,
                    trip?.VehicleRefKey,
                    row.ScheduledStartAt,
                    row.ScheduledEndAt,
                    trip?.DispatchedAt,
                    trip?.StartedAt,
                    row.IsLate,
                    row.IsAtRisk,
                    row.RouteCount,
                    row.PendingStopCount,
                    offset,
                    width);
            })
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
                StringComparison.OrdinalIgnoreCase)));

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "dispatch_active_trips",
            board.Scope,
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
}
