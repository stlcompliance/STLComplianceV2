using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class FieldCompanionNotificationSettingsService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<FieldCompanionNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantFieldCompanionNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new FieldCompanionNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnHandoffRedeemed: true,
                NotifyOnFieldInboxRefreshed: true,
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<FieldCompanionNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertFieldCompanionNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantFieldCompanionNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantFieldCompanionNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantFieldCompanionNotificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = FieldCompanionNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnHandoffRedeemed = request.NotifyOnHandoffRedeemed;
        entity.NotifyOnFieldInboxRefreshed = request.NotifyOnFieldInboxRefreshed;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "fieldcompanion.notification_settings.update",
            "tenant_fieldcompanion_notification_settings",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantFieldCompanionNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantFieldCompanionNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantFieldCompanionNotificationSettingsSnapshot ToSnapshot(
        TenantFieldCompanionNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnHandoffRedeemed,
            settings.NotifyOnFieldInboxRefreshed);

    private static FieldCompanionNotificationSettingsResponse MapResponse(
        TenantFieldCompanionNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnHandoffRedeemed,
            settings.NotifyOnFieldInboxRefreshed,
            settings.UpdatedAt);
}
