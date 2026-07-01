using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementNotificationEnqueueService(
    SupplyArrDbContext db,
    ProcurementNotificationSettingsService settingsService)
{
    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid? supplierId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !ProcurementNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var duplicate = await db.ProcurementNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && (x.DispatchStatus == ProcurementNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == ProcurementNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new ProcurementNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            SupplierId = supplierId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = ProcurementNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.ProcurementNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }

    public async Task<Guid?> TryEnqueueRepeatableAsync(
        Guid tenantId,
        string eventKind,
        Guid? supplierId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !ProcurementNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var pendingDuplicate = await db.ProcurementNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && x.DispatchStatus == ProcurementNotificationDispatchStatuses.Pending,
            cancellationToken);

        if (pendingDuplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new ProcurementNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            SupplierId = supplierId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = ProcurementNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.ProcurementNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }
}
