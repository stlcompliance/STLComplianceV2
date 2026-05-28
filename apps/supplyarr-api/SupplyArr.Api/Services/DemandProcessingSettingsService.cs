using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class DemandProcessingSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<DemandProcessingSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDemandProcessingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<DemandProcessingSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertDemandProcessingSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantDemandProcessingSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantDemandProcessingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantDemandProcessingSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.AutoCreatePrDraftWhenShort = request.AutoCreatePrDraftWhenShort;
        entity.MinHoursBeforeProcessing = DemandProcessingRules.NormalizeMinHours(request.MinHoursBeforeProcessing);
        entity.StalenessHours = DemandProcessingRules.NormalizeStalenessHours(request.StalenessHours);
        entity.NotifyOnPrDraftCreated = request.NotifyOnPrDraftCreated;
        entity.ProcessMaintainarrDemandRefs = request.ProcessMaintainarrDemandRefs;
        entity.ProcessRoutarrDemandRefs = request.ProcessRoutarrDemandRefs;
        entity.ProcessTrainarrDemandRefs = request.ProcessTrainarrDemandRefs;
        entity.ProcessStaffarrDemandRefs = request.ProcessStaffarrDemandRefs;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.demand_processing_settings.update",
            tenantId,
            actorUserId,
            "tenant_demand_processing_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantDemandProcessingSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDemandProcessingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantDemandProcessingSettingsSnapshot ToSnapshot(TenantDemandProcessingSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoCreatePrDraftWhenShort,
            settings.MinHoursBeforeProcessing,
            settings.StalenessHours,
            settings.NotifyOnPrDraftCreated,
            settings.ProcessMaintainarrDemandRefs,
            settings.ProcessRoutarrDemandRefs,
            settings.ProcessTrainarrDemandRefs,
            settings.ProcessStaffarrDemandRefs);

    private static DemandProcessingSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            AutoCreatePrDraftWhenShort: false,
            MinHoursBeforeProcessing: DemandProcessingDefaults.MinHoursBeforeProcessing,
            StalenessHours: DemandProcessingDefaults.StalenessHours,
            NotifyOnPrDraftCreated: true,
            ProcessMaintainarrDemandRefs: true,
            ProcessRoutarrDemandRefs: false,
            ProcessTrainarrDemandRefs: false,
            ProcessStaffarrDemandRefs: false,
            UpdatedAt: null);

    private static DemandProcessingSettingsResponse MapResponse(TenantDemandProcessingSettings settings) =>
        new(
            settings.IsEnabled,
            settings.AutoCreatePrDraftWhenShort,
            settings.MinHoursBeforeProcessing,
            settings.StalenessHours,
            settings.NotifyOnPrDraftCreated,
            settings.ProcessMaintainarrDemandRefs,
            settings.ProcessRoutarrDemandRefs,
            settings.ProcessTrainarrDemandRefs,
            settings.ProcessStaffarrDemandRefs,
            settings.UpdatedAt);
}
