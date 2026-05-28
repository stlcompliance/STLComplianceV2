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

                NotifyOnAssignmentCompleted: true,

                NotifyOnQualificationExpiring: true,

                NotifyOnQualificationIssued: true,

                NotifyOnQualificationSuspended: true,

                NotifyOnQualificationRevoked: true,

                NotifyOnQualificationExpired: true,

                NotifyOnAssignmentDueReminder: true,

                NotifyOnAssignmentOverdueEscalation: true,

                ExpiringLeadDays: TrainingNotificationRules.NormalizeExpiringLeadDays(null),

                MaxAttempts: TrainingNotificationRules.NormalizeMaxAttempts(null),

                RetryIntervalMinutes: TrainingNotificationRules.NormalizeRetryIntervalMinutes(null),

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

        entity.NotifyOnAssignmentCompleted = request.NotifyOnAssignmentCompleted;

        entity.NotifyOnQualificationExpiring = request.NotifyOnQualificationExpiring;

        entity.NotifyOnQualificationIssued = request.NotifyOnQualificationIssued;

        entity.NotifyOnQualificationSuspended = request.NotifyOnQualificationSuspended;

        entity.NotifyOnQualificationRevoked = request.NotifyOnQualificationRevoked;

        entity.NotifyOnQualificationExpired = request.NotifyOnQualificationExpired;

        entity.NotifyOnAssignmentDueReminder = request.NotifyOnAssignmentDueReminder;

        entity.NotifyOnAssignmentOverdueEscalation = request.NotifyOnAssignmentOverdueEscalation;

        entity.ExpiringLeadDays = TrainingNotificationRules.NormalizeExpiringLeadDays(request.ExpiringLeadDays);

        entity.MaxAttempts = TrainingNotificationRules.NormalizeMaxAttempts(request.MaxAttempts);

        entity.RetryIntervalMinutes = TrainingNotificationRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);

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

            settings.NotifyOnAssignmentCompleted,

            settings.NotifyOnQualificationExpiring,

            settings.NotifyOnQualificationIssued,

            settings.NotifyOnQualificationSuspended,

            settings.NotifyOnQualificationRevoked,

            settings.NotifyOnQualificationExpired,

            settings.NotifyOnAssignmentDueReminder,

            settings.NotifyOnAssignmentOverdueEscalation,

            settings.ExpiringLeadDays,

            settings.MaxAttempts,

            settings.RetryIntervalMinutes);



    private static TrainingNotificationSettingsResponse MapResponse(

        TenantTrainingNotificationSettings settings) =>

        new(

            settings.IsEnabled,

            settings.NotificationWebhookUrl,

            settings.NotifyOnAssignmentCreated,

            settings.NotifyOnAssignmentCompleted,

            settings.NotifyOnQualificationExpiring,

            settings.NotifyOnQualificationIssued,

            settings.NotifyOnQualificationSuspended,

            settings.NotifyOnQualificationRevoked,

            settings.NotifyOnQualificationExpired,

            settings.NotifyOnAssignmentDueReminder,

            settings.NotifyOnAssignmentOverdueEscalation,

            settings.ExpiringLeadDays,

            settings.MaxAttempts,

            settings.RetryIntervalMinutes,

            settings.UpdatedAt);

}



