using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementCoordinationSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<ProcurementCoordinationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantProcurementCoordinationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<ProcurementCoordinationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertProcurementCoordinationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantProcurementCoordinationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantProcurementCoordinationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantProcurementCoordinationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = ProcurementCoordinationRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.procurement_coordination_settings.update",
            tenantId,
            actorUserId,
            "tenant_procurement_coordination_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static ProcurementCoordinationSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: ProcurementCoordinationDefaults.StalenessHours,
            UpdatedAt: null);

    private static ProcurementCoordinationSettingsResponse MapResponse(TenantProcurementCoordinationSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
