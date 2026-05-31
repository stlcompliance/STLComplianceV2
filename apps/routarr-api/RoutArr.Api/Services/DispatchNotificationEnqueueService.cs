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
            null,
            trip.AssignedDriverPersonId,
            "trip",
            trip.Id,
            cancellationToken);
    }

    public Task<Guid?> TryEnqueueTripAcceptedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            DispatchNotificationEventKinds.TripAccepted,
            trip.Id,
            null,
            trip.AssignedDriverPersonId,
            "trip",
            trip.Id,
            cancellationToken);

    public Task<Guid?> TryEnqueueDriverAssignmentChangedAsync(
        Trip trip,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            trip.TenantId,
            DispatchNotificationEventKinds.DriverAssignmentChanged,
            trip.Id,
            null,
            trip.AssignedDriverPersonId,
            "trip",
            trip.Id,
            cancellationToken,
            suppressDuplicates: false);

    public Task<Guid?> TryEnqueueRouteCancelledAsync(
        DispatchRoute route,
        CancellationToken cancellationToken = default) =>
        TryEnqueueAsync(
            route.TenantId,
            DispatchNotificationEventKinds.RouteCancelled,
            route.TripId,
            route.Id,
            null,
            "route",
            route.Id,
            cancellationToken);

    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid? tripId,
        Guid? routeId,
        string? driverPersonId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default,
        bool suppressDuplicates = true)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !DispatchNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        if (suppressDuplicates)
        {
            var duplicate = await db.DispatchNotificationDispatches.AnyAsync(
                x => x.TenantId == tenantId
                    && x.EventKind == eventKind
                    && x.RelatedEntityType == relatedEntityType
                    && x.RelatedEntityId == relatedEntityId
                    && (x.DispatchStatus == DispatchNotificationDispatchStatuses.Pending
                        || x.DispatchStatus == DispatchNotificationDispatchStatuses.Sent),
                cancellationToken);

            if (duplicate)
            {
                return null;
            }
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new DispatchNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            TripId = tripId,
            RouteId = routeId,
            DriverPersonId = driverPersonId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = DispatchNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.DispatchNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }
}
