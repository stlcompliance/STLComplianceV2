using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class FieldInboxService(RoutArrDbContext db, IConfiguration configuration)
{
    private readonly string? _frontendBaseUrl = configuration["RoutArr:FrontendBaseUrl"]
        ?? configuration["Cors:RoutArrFrontendOrigin"];
    private static readonly HashSet<string> ActiveTripStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        TripDispatchStatuses.Assigned,
        TripDispatchStatuses.Dispatched,
        TripDispatchStatuses.InProgress,
        TripDispatchStatuses.Planned,
    };

    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && ActiveTripStatuses.Contains(x.DispatchStatus));

        if (!viewAll)
        {
            var personId = actorPersonId?.Trim();
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId
                || (personId != null
                    && x.AssignedDriverPersonId != null
                    && x.AssignedDriverPersonId == personId));
        }

        var trips = await query
            .OrderByDescending(x => x.ScheduledStartAt ?? x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (trips.Count == 0)
        {
            return FieldInboxRules.BuildProductResponse([]);
        }

        var tripIds = trips.Select(x => x.Id).ToList();
        var dvirPhases = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && tripIds.Contains(x.TripId))
            .Select(x => new { x.TripId, x.Phase })
            .ToListAsync(cancellationToken);

        var preTripByTrip = dvirPhases
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.TripId)
            .ToHashSet();
        var postTripByTrip = dvirPhases
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.TripId)
            .ToHashSet();

        var items = trips.Select(trip =>
        {
            var deepLinkPath = $"/trips/{trip.Id:D}";
            var blockedReason = ResolveBlockedReason(trip, preTripByTrip, postTripByTrip);
            return new FieldInboxTaskItem(
                $"routarr:trip:{trip.Id:D}",
                "routarr",
                "trip",
                trip.Title,
                trip.TripNumber,
                trip.DispatchStatus,
                null,
                trip.ScheduledStartAt,
                trip.ScheduledStartAt ?? trip.UpdatedAt,
                deepLinkPath,
                blockedReason,
                DeepLinkUrl: FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(_frontendBaseUrl, deepLinkPath));
        }).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }

    private static string? ResolveBlockedReason(
        Trip trip,
        IReadOnlySet<Guid> preTripByTrip,
        IReadOnlySet<Guid> postTripByTrip)
    {
        var needsPreTrip = !preTripByTrip.Contains(trip.Id)
            && (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Assigned, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trip.DispatchStatus, TripDispatchStatuses.Dispatched, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trip.DispatchStatus, TripDispatchStatuses.Planned, StringComparison.OrdinalIgnoreCase)
                || string.Equals(trip.DispatchStatus, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase));

        var needsPostTrip = !postTripByTrip.Contains(trip.Id)
            && string.Equals(trip.DispatchStatus, TripDispatchStatuses.InProgress, StringComparison.OrdinalIgnoreCase);

        if (needsPreTrip && needsPostTrip)
        {
            return "Pre-trip and post-trip DVIR required";
        }

        if (needsPreTrip)
        {
            return "Pre-trip DVIR required";
        }

        if (needsPostTrip)
        {
            return "Post-trip DVIR required";
        }

        return null;
    }
}
