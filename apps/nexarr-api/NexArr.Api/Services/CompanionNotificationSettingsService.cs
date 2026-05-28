using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class CompanionNotificationSettingsService(
    NexArrDbContext db,
    IPlatformAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<CompanionNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantCompanionNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new CompanionNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnHandoffRedeemed: true,
                NotifyOnFieldInboxRefreshed: true,
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<CompanionNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertCompanionNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantCompanionNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantCompanionNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantCompanionNotificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = CompanionNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnHandoffRedeemed = request.NotifyOnHandoffRedeemed;
        entity.NotifyOnFieldInboxRefreshed = request.NotifyOnFieldInboxRefreshed;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "companion.notification_settings.update",
            "tenant_companion_notification_settings",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantCompanionNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantCompanionNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantCompanionNotificationSettingsSnapshot ToSnapshot(
        TenantCompanionNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnHandoffRedeemed,
            settings.NotifyOnFieldInboxRefreshed);

    private static CompanionNotificationSettingsResponse MapResponse(
        TenantCompanionNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnHandoffRedeemed,
            settings.NotifyOnFieldInboxRefreshed,
            settings.UpdatedAt);
}
