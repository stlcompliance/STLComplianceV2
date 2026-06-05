using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class FieldCompanionNotificationEnqueueService(
    NexArrDbContext db,
    FieldCompanionNotificationSettingsService settingsService,
    FieldCompanionPushSubscriptionService pushSubscriptionService)
{
    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid? actorUserId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null)
        {
            return null;
        }

        var targetUserId = ResolveTargetUserId(eventKind, actorUserId, relatedEntityId);
        var hasPushSubscription = targetUserId is Guid userId
            && await pushSubscriptionService.UserHasSubscriptionAsync(tenantId, userId, cancellationToken);

        if (!FieldCompanionNotificationRules.ShouldEnqueueForEvent(settings, eventKind, hasPushSubscription))
        {
            return null;
        }

        var duplicate = await db.FieldCompanionNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && (x.DispatchStatus == FieldCompanionNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == FieldCompanionNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new FieldCompanionNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            ActorUserId = actorUserId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = FieldCompanionNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.FieldCompanionNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }

    public static Guid? ResolveTargetUserId(string eventKind, Guid? actorUserId, Guid relatedEntityId) =>
        eventKind switch
        {
            FieldCompanionNotificationEventKinds.HandoffRedeemed => actorUserId,
            FieldCompanionNotificationEventKinds.FieldInboxRefreshed => relatedEntityId,
            _ => actorUserId ?? relatedEntityId,
        };
}
