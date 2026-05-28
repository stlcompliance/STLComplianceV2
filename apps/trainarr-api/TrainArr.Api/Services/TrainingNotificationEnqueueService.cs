using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingNotificationEnqueueService(
    TrainArrDbContext db,
    TrainingNotificationSettingsService settingsService)
{
    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid staffarrPersonId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !TrainingNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var duplicate = await db.TrainingNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && (x.DispatchStatus == TrainingNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == TrainingNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new TrainingNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            StaffarrPersonId = staffarrPersonId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = TrainingNotificationDispatchStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }

    public async Task<Guid?> TryEnqueueRepeatableAsync(
        Guid tenantId,
        string eventKind,
        Guid staffarrPersonId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !TrainingNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var pendingDuplicate = await db.TrainingNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && x.DispatchStatus == TrainingNotificationDispatchStatuses.Pending,
            cancellationToken);

        if (pendingDuplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new TrainingNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            StaffarrPersonId = staffarrPersonId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = TrainingNotificationDispatchStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }

    public async Task TryEnqueueFromDomainEventAsync(
        TrainingDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var notificationEventKind = TrainingNotificationRules.TryMapDomainEventKind(domainEvent.EventKind);
        if (notificationEventKind is null)
        {
            return;
        }

        await TryEnqueueAsync(
            domainEvent.TenantId,
            notificationEventKind,
            domainEvent.StaffarrPersonId,
            domainEvent.RelatedEntityType,
            domainEvent.RelatedEntityId,
            cancellationToken);
    }
}
