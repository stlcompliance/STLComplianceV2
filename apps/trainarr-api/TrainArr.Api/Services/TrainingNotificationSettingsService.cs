using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingNotificationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<TrainingNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantTrainingNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new TrainingNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnAssignmentCreated: true,
                NotifyOnQualificationExpiring: true,
                NotifyOnQualificationExpired: true,
                ExpiringLeadDays: TrainingNotificationRules.NormalizeExpiringLeadDays(null),
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<TrainingNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertTrainingNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantTrainingNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantTrainingNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantTrainingNotificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = TrainingNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnAssignmentCreated = request.NotifyOnAssignmentCreated;
        entity.NotifyOnQualificationExpiring = request.NotifyOnQualificationExpiring;
        entity.NotifyOnQualificationExpired = request.NotifyOnQualificationExpired;
        entity.ExpiringLeadDays = TrainingNotificationRules.NormalizeExpiringLeadDays(request.ExpiringLeadDays);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trainarr.notification_settings.update",
            tenantId,
            actorUserId,
            "tenant_training_notification_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantTrainingNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantTrainingNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantTrainingNotificationSettingsSnapshot ToSnapshot(
        TenantTrainingNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnAssignmentCreated,
            settings.NotifyOnQualificationExpiring,
            settings.NotifyOnQualificationExpired,
            settings.ExpiringLeadDays);

    private static TrainingNotificationSettingsResponse MapResponse(
        TenantTrainingNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnAssignmentCreated,
            settings.NotifyOnQualificationExpiring,
            settings.NotifyOnQualificationExpired,
            settings.ExpiringLeadDays,
            settings.UpdatedAt);
}
