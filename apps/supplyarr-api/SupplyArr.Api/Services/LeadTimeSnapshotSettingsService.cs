using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class LeadTimeSnapshotSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<LeadTimeSnapshotSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantLeadTimeSnapshotSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<LeadTimeSnapshotSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertLeadTimeSnapshotSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantLeadTimeSnapshotSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantLeadTimeSnapshotSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantLeadTimeSnapshotSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = LeadTimeSnapshotCaptureRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.lead_time_snapshot_settings.update",
            tenantId,
            actorUserId,
            "tenant_lead_time_snapshot_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static LeadTimeSnapshotSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: LeadTimeSnapshotWorkerDefaults.StalenessHours,
            UpdatedAt: null);

    private static LeadTimeSnapshotSettingsResponse MapResponse(TenantLeadTimeSnapshotSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
