using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MaintenanceHistoryRollupSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<MaintenanceHistoryRollupSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantMaintenanceHistoryRollupSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<MaintenanceHistoryRollupSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertMaintenanceHistoryRollupSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantMaintenanceHistoryRollupSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantMaintenanceHistoryRollupSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantMaintenanceHistoryRollupSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = MaintenanceHistoryRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.maintenance_history_rollup_settings.update",
            tenantId,
            actorUserId,
            "tenant_maintenance_history_rollup_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static MaintenanceHistoryRollupSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: MaintenanceHistoryRollupDefaults.StalenessHours,
            UpdatedAt: null);

    private static MaintenanceHistoryRollupSettingsResponse MapResponse(
        TenantMaintenanceHistoryRollupSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
