using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceNotificationEnqueueService(
    MaintainArrDbContext db,
    MaintenanceNotificationSettingsService settingsService)
{
    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid assetId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (settings is null || !MaintenanceNotificationRules.ShouldNotifyForEvent(settings, eventKind))
        {
            return null;
        }

        var duplicate = await db.MaintenanceNotificationDispatches.AnyAsync(
            x => x.TenantId == tenantId
                && x.EventKind == eventKind
                && x.RelatedEntityType == relatedEntityType
                && x.RelatedEntityId == relatedEntityId
                && (x.DispatchStatus == MaintenanceNotificationDispatchStatuses.Pending
                    || x.DispatchStatus == MaintenanceNotificationDispatchStatuses.Sent),
            cancellationToken);

        if (duplicate)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var dispatch = new MaintenanceNotificationDispatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventKind = eventKind,
            AssetId = assetId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            DispatchStatus = MaintenanceNotificationDispatchStatuses.Pending,
            CreatedAt = now,
        };

        db.MaintenanceNotificationDispatches.Add(dispatch);
        await db.SaveChangesAsync(cancellationToken);
        return dispatch.Id;
    }
}
