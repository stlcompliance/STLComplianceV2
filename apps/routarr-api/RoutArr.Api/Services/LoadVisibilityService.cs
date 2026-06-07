using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class LoadVisibilityService(RoutArrDbContext db, IRoutArrAuditService audit)
{
    public const string ReadAction = "load_visibility.read";

    public async Task<IReadOnlyList<TransportationLoadVisibilityResponse>> ListAsync(
        ClaimsPrincipal principal,
        bool viewAll,
        Guid? actorUserId,
        string? actorPersonId,
        Guid? tripId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var query = db.TripLoads
            .AsNoTracking()
            .Include(x => x.Trip)
            .Where(x => x.TenantId == tenantId);
        query = ApplyAccessFilter(query, viewAll, actorUserId, actorPersonId);

        if (tripId.HasValue)
        {
            query = query.Where(x => x.TripId == tripId.Value);
        }

        var loads = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ThenBy(x => x.SequenceNumber)
            .ToListAsync(cancellationToken);

        var routesByTrip = tripId.HasValue
            ? await db.Routes
                .AsNoTracking()
                .Include(x => x.Stops)
                .Where(x => x.TenantId == tenantId && x.TripId == tripId.Value)
                .ToListAsync(cancellationToken)
            : new List<DispatchRoute>();

        var responses = loads
            .Select(load => Map(load, routesByTrip))
            .ToList();

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            principal.GetUserId(),
            "load_visibility",
            tripId?.ToString() ?? "all",
            responses.Count.ToString(),
            cancellationToken: cancellationToken);

        return responses;
    }

    private static IQueryable<TripLoad> ApplyAccessFilter(
        IQueryable<TripLoad> query,
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
            x.Trip != null
            && (
                x.Trip.CreatedByUserId == actorUserId.Value
                || (personId != null
                    && x.Trip.AssignedDriverPersonId != null
                    && x.Trip.AssignedDriverPersonId == personId)));
    }

    private static TransportationLoadVisibilityResponse Map(
        TripLoad load,
        IReadOnlyList<DispatchRoute> routes)
    {
        var route = routes.FirstOrDefault(x => x.TripId == load.TripId);
        var itemSummary = string.IsNullOrWhiteSpace(load.Description)
            ? load.LoadKey
            : load.Description;

        return new TransportationLoadVisibilityResponse(
            load.Id,
            load.LoadKey,
            load.TripId,
            route?.Id,
            load.Trip?.TripNumber is not null ? "ordarr" : "manual",
            load.TripId.ToString(),
            load.LoadType,
            load.Status,
            load.OriginLabel,
            load.DestinationLabel,
            null,
            null,
            [],
            [],
            itemSummary,
            [],
            null,
            false,
            null,
            null,
            null,
            [],
            load.CreatedAt,
            load.UpdatedAt);
    }
}
