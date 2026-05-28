using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Operations.LoadTesting;

namespace RoutArr.Api.Services;

public sealed class LoadTestJourneySeedService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    TripService tripService)
{
    public async Task<LoadTestJourneySeedResponse> EnsureSeededAsync(
        Guid tenantId,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var subjectPersonId = StlRoutArrLoadTestJourneySeedCatalog.SubjectPersonId;
        var title = StlRoutArrLoadTestJourneySeedCatalog.JourneyTripTitle;
        var now = DateTimeOffset.UtcNow;
        var scheduledStartAt = now.AddHours(2);
        var scheduledEndAt = now.AddHours(6);

        var existing = await db.Trips
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Title == title,
                cancellationToken);

        if (existing is not null)
        {
            if (existing.ScheduledEndAt is null || existing.ScheduledEndAt < now.AddHours(1))
            {
                existing.ScheduledStartAt = scheduledStartAt;
                existing.ScheduledEndAt = scheduledEndAt;
                existing.UpdatedAt = now;
                await db.SaveChangesAsync(cancellationToken);
            }

            return new LoadTestJourneySeedResponse(
                subjectPersonId,
                existing.Id,
                TripCreated: false,
                existing.ScheduledStartAt,
                existing.ScheduledEndAt);
        }

        var created = await tripService.CreateAsync(
            tenantId,
            actorUserId,
            new CreateTripRequest(
                title,
                StlRoutArrLoadTestJourneySeedCatalog.JourneyTripDescription,
                VehicleRefKey: null,
                scheduledStartAt,
                scheduledEndAt,
                Loads: null),
            cancellationToken);

        await audit.WriteAsync(
            "load_test_journey.seed",
            tenantId,
            actorUserId,
            "trip",
            created.TripId.ToString(),
            TripDispatchStatuses.Planned,
            cancellationToken: cancellationToken);

        return new LoadTestJourneySeedResponse(
            subjectPersonId,
            created.TripId,
            TripCreated: true,
            created.ScheduledStartAt,
            created.ScheduledEndAt);
    }
}
