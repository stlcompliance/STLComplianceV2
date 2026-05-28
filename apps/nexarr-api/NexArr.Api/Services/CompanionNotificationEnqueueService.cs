using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class CompanionNotificationEnqueueService(
    NexArrDbContext db,
    CompanionNotificationSettingsService settingsService)
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
        if (settings is null || !CompanionNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var duplicate = await db.CompanionNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && (x.DispatchStatus == CompanionNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == CompanionNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new CompanionNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            ActorUserId = actorUserId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = CompanionNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.CompanionNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }
}
