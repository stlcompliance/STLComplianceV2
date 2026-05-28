using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class DispatchNotificationEnqueueService(
    RoutArrDbContext db,
    DispatchNotificationSettingsService settingsService)
{
    public async Task<Guid?> TryEnqueueForTripStatusAsync(
        Trip trip,
        string dispatchStatus,
        CancellationToken cancellationToken = default)
    {
        var eventKind = DispatchNotificationRules.MapDispatchStatusToEventKind(dispatchStatus);
        if (eventKind is null)
        {
            return null;
        }

        return await TryEnqueueAsync(
            trip.TenantId,
            eventKind,
            trip.Id,
            trip.AssignedDriverPersonId,
            cancellationToken);
    }

    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid tripId,
        string? driverPersonId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !DispatchNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var duplicate = await db.DispatchNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == "trip"
                && x.RelatedEntityId == tripId
                && (x.DispatchStatus == DispatchNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == DispatchNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new DispatchNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            TripId = tripId,
            DriverPersonId = driverPersonId,
            RelatedEntityType = "trip",
            RelatedEntityId = tripId,
            DispatchStatus = DispatchNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.DispatchNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }
}
