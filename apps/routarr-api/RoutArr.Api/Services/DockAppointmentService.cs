using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DockAppointmentService(RoutArrDbContext db, IRoutArrAuditService audit)
{
    public const string ReadAction = "dock_appointment.read";

    public async Task<IReadOnlyList<DockAppointmentNotificationResponse>> ListAsync(
        ClaimsPrincipal principal,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        Guid? tripId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var query = db.RouteStops
            .AsNoTracking()
            .Include(x => x.Route)
            .ThenInclude(x => x!.Trip)
            .Where(x => x.TenantId == tenantId);
        query = ApplyAccessFilter(query, viewAll, actorUserId, actorPersonId);

        if (tripId.HasValue)
        {
            query = query.Where(x => x.Route.TripId == tripId.Value);
        }

        var now = DateTimeOffset.UtcNow;
        var stops = await query
            .OrderBy(x => x.ScheduledArrivalAt ?? now)
            .ThenBy(x => x.SequenceNumber)
            .ToListAsync(cancellationToken);

        var responses = stops.Select(stop => Map(stop)).ToList();

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            principal.GetUserId(),
            "dock_appointment",
            tripId?.ToString() ?? "all",
            responses.Count.ToString(),
            cancellationToken: cancellationToken);

        return responses;
    }

    private static IQueryable<RouteStop> ApplyAccessFilter(
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

    private static DockAppointmentNotificationResponse Map(RouteStop stop)
    {
        var trip = stop.Route.Trip;
        var route = stop.Route;
        var appointmentType = stop.StopType == RouteStopTypes.Depot
            ? "request"
            : stop.StopType == RouteStopTypes.Delivery
                ? "delivery"
                : "eta_update";

        var eta = stop.ScheduledArrivalAt ?? trip?.ScheduledStartAt;
        var status = stop.StopStatus switch
        {
            RouteStopStatuses.Completed => "completed",
            RouteStopStatuses.Arrived => "arrived",
            _ => "draft",
        };

        return new DockAppointmentNotificationResponse(
            stop.Id,
            $"DAPT-{stop.Id.ToString("N")[..10].ToUpperInvariant()}",
            trip?.Id,
            route.Id,
            stop.Id,
            appointmentType,
            stop.ScheduledArrivalAt,
            stop.ScheduledArrivalAt?.AddMinutes(30),
            stop.ArrivedAt,
            stop.CompletedAt,
            eta,
            status,
            trip?.AssignedDriverPersonId,
            trip?.AssignedDriverPersonId,
            trip?.VehicleRefKey,
            null,
            "routarr",
            trip?.Id.ToString(),
            null,
            stop.CreatedAt,
            stop.ArrivedAt,
            stop.CompletedAt,
            null);
    }
}
