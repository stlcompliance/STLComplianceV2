using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceNotificationSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<MaintenanceNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantMaintenanceNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new MaintenanceNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnWorkOrderCreated: true,
                NotifyOnPmScheduleDue: true,
                NotifyOnPmScheduleOverdue: true,
                NotifyOnDefectEscalated: true,
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<MaintenanceNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertMaintenanceNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantMaintenanceNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantMaintenanceNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantMaintenanceNotificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = MaintenanceNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnWorkOrderCreated = request.NotifyOnWorkOrderCreated;
        entity.NotifyOnPmScheduleDue = request.NotifyOnPmScheduleDue;
        entity.NotifyOnPmScheduleOverdue = request.NotifyOnPmScheduleOverdue;
        entity.NotifyOnDefectEscalated = request.NotifyOnDefectEscalated;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.notification_settings.update",
            tenantId,
            actorUserId,
            "tenant_maintenance_notification_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantMaintenanceNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantMaintenanceNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantMaintenanceNotificationSettingsSnapshot ToSnapshot(
        TenantMaintenanceNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnWorkOrderCreated,
            settings.NotifyOnPmScheduleDue,
            settings.NotifyOnPmScheduleOverdue,
            settings.NotifyOnDefectEscalated);

    private static MaintenanceNotificationSettingsResponse MapResponse(
        TenantMaintenanceNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnWorkOrderCreated,
            settings.NotifyOnPmScheduleDue,
            settings.NotifyOnPmScheduleOverdue,
            settings.NotifyOnDefectEscalated,
            settings.UpdatedAt);
}
