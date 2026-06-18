using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceNotificationEnqueueService(
    MaintainArrDbContext db,
    MaintenanceNotificationSettingsService settingsService,
    MaintainArrTenantSettingsService tenantSettings)
{
    public async Task<Guid?> TryEnqueueAsync(
        Guid tenantId,
        string eventKind,
        Guid assetId,
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        var behaviorSettings = await tenantSettings.LoadEffectiveSettingsAsync(tenantId, cancellationToken);
        if (!ShouldNotifyForTenantBehavior(behaviorSettings, eventKind))
        {
            return null;
        }

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

    private static bool ShouldNotifyForTenantBehavior(MaintainArrTenantSettingsDto settings, string eventKind) =>
        eventKind switch
        {
            MaintenanceNotificationEventKinds.WorkOrderCreated => settings.Notifications.NotifyOnWOAssigned,
            MaintenanceNotificationEventKinds.PmScheduleDue => settings.Notifications.NotifyOnPMComingDue,
            MaintenanceNotificationEventKinds.PmScheduleOverdue => settings.Notifications.NotifyOnPMOverdue,
            MaintenanceNotificationEventKinds.DefectEscalated => settings.Notifications.NotifyOnCriticalDefect,
            _ => true
        };
}
