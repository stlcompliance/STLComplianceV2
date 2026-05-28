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

        var items = trips.Select(trip =>
        {
            var deepLinkPath = $"/trips/{trip.Id:D}";
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
                DeepLinkUrl: FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(_frontendBaseUrl, deepLinkPath));
        }).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }
}
