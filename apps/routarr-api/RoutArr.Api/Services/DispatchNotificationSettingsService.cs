using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class DispatchNotificationSettingsService(
    RoutArrDbContext db,
    IRoutArrAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<DispatchNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDispatchNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new DispatchNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnTripAssigned: true,
                NotifyOnTripDispatched: true,
                NotifyOnTripAccepted: true,
                NotifyOnTripInProgress: true,
                NotifyOnTripCompleted: true,
                NotifyOnTripCancelled: true,
                NotifyOnDriverAssignmentChanged: true,
                NotifyOnRouteCancelled: true,
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<DispatchNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertDispatchNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantDispatchNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantDispatchNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantDispatchNotificationSettings.Add(entity);
        }

        DispatchNotificationRules.ValidateUpsertRequest(request.IsEnabled, request.NotificationWebhookUrl);

        var normalizedWebhook = DispatchNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        if (!request.IsEnabled
            && normalizedWebhook is null
            && !request.ClearNotificationWebhookOnDisable
            && !string.IsNullOrWhiteSpace(entity.NotificationWebhookUrl))
        {
            normalizedWebhook = entity.NotificationWebhookUrl;
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = normalizedWebhook;
        entity.NotifyOnTripAssigned = request.NotifyOnTripAssigned;
        entity.NotifyOnTripDispatched = request.NotifyOnTripDispatched;
        entity.NotifyOnTripAccepted = request.NotifyOnTripAccepted;
        entity.NotifyOnTripInProgress = request.NotifyOnTripInProgress;
        entity.NotifyOnTripCompleted = request.NotifyOnTripCompleted;
        entity.NotifyOnTripCancelled = request.NotifyOnTripCancelled;
        entity.NotifyOnDriverAssignmentChanged = request.NotifyOnDriverAssignmentChanged;
        entity.NotifyOnRouteCancelled = request.NotifyOnRouteCancelled;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "routarr.notification_settings.update",
            tenantId,
            actorUserId,
            "tenant_dispatch_notification_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantDispatchNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDispatchNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantDispatchNotificationSettingsSnapshot ToSnapshot(
        TenantDispatchNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnTripAssigned,
            settings.NotifyOnTripDispatched,
            settings.NotifyOnTripAccepted,
            settings.NotifyOnTripInProgress,
            settings.NotifyOnTripCompleted,
            settings.NotifyOnTripCancelled,
            settings.NotifyOnDriverAssignmentChanged,
            settings.NotifyOnRouteCancelled);

    private static DispatchNotificationSettingsResponse MapResponse(
        TenantDispatchNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnTripAssigned,
            settings.NotifyOnTripDispatched,
            settings.NotifyOnTripAccepted,
            settings.NotifyOnTripInProgress,
            settings.NotifyOnTripCompleted,
            settings.NotifyOnTripCancelled,
            settings.NotifyOnDriverAssignmentChanged,
            settings.NotifyOnRouteCancelled,
            settings.UpdatedAt);
}
