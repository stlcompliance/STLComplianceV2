using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementNotificationSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<ProcurementNotificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantProcurementNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new ProcurementNotificationSettingsResponse(
                IsEnabled: false,
                NotificationWebhookUrl: null,
                NotifyOnPurchaseRequestSubmitted: true,
                NotifyOnPurchaseRequestApproved: true,
                NotifyOnPurchaseOrderIssued: true,
                NotifyOnReceivingReceiptPosted: true,
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<ProcurementNotificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertProcurementNotificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        var entity = await db.TenantProcurementNotificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantProcurementNotificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantProcurementNotificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.NotificationWebhookUrl = ProcurementNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnPurchaseRequestSubmitted = request.NotifyOnPurchaseRequestSubmitted;
        entity.NotifyOnPurchaseRequestApproved = request.NotifyOnPurchaseRequestApproved;
        entity.NotifyOnPurchaseOrderIssued = request.NotifyOnPurchaseOrderIssued;
        entity.NotifyOnReceivingReceiptPosted = request.NotifyOnReceivingReceiptPosted;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.notification_settings.update",
            tenantId,
            actorUserId,
            "tenant_procurement_notification_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantProcurementNotificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantProcurementNotificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantProcurementNotificationSettingsSnapshot ToSnapshot(
        TenantProcurementNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnPurchaseRequestSubmitted,
            settings.NotifyOnPurchaseRequestApproved,
            settings.NotifyOnPurchaseOrderIssued,
            settings.NotifyOnReceivingReceiptPosted);

    private static ProcurementNotificationSettingsResponse MapResponse(
        TenantProcurementNotificationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.NotificationWebhookUrl,
            settings.NotifyOnPurchaseRequestSubmitted,
            settings.NotifyOnPurchaseRequestApproved,
            settings.NotifyOnPurchaseOrderIssued,
            settings.NotifyOnReceivingReceiptPosted,
            settings.UpdatedAt);
}
