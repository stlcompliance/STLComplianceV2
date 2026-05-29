using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class AttachmentRetentionSettingsService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<AttachmentRetentionSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAttachmentRetentionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new AttachmentRetentionSettingsResponse(
                false,
                AttachmentRetentionRules.DefaultRetentionDays,
                null)
            : Map(settings);
    }

    public async Task<AttachmentRetentionSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertAttachmentRetentionSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantAttachmentRetentionSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantAttachmentRetentionSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantAttachmentRetentionSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.RetentionDaysAfterTripClose = AttachmentRetentionRules.NormalizeRetentionDays(
            request.RetentionDaysAfterTripClose);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "attachment_retention_settings.upsert",
            tenantId,
            actorUserId,
            "attachment_retention_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    private static AttachmentRetentionSettingsResponse Map(TenantAttachmentRetentionSettings settings) =>
        new(settings.IsEnabled, settings.RetentionDaysAfterTripClose, settings.UpdatedAt);
}
