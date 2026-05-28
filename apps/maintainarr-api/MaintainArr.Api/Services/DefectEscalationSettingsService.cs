using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class DefectEscalationSettingsService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<DefectEscalationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDefectEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<DefectEscalationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertDefectEscalationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantDefectEscalationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantDefectEscalationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantDefectEscalationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.LowThresholdHours = DefectEscalationRules.NormalizeThresholdHours(request.LowThresholdHours);
        entity.MediumThresholdHours = DefectEscalationRules.NormalizeThresholdHours(request.MediumThresholdHours);
        entity.HighThresholdHours = DefectEscalationRules.NormalizeThresholdHours(request.HighThresholdHours);
        entity.CriticalThresholdHours = DefectEscalationRules.NormalizeThresholdHours(request.CriticalThresholdHours);
        entity.AutoAcknowledgeOnEscalation = request.AutoAcknowledgeOnEscalation;
        entity.AutoCreateWorkOrderOnEscalation = request.AutoCreateWorkOrderOnEscalation;
        entity.BumpSeverityOnRepeatEscalation = request.BumpSeverityOnRepeatEscalation;
        entity.NotifyOnEscalation = request.NotifyOnEscalation;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "maintainarr.defect_escalation_settings.update",
            tenantId,
            actorUserId,
            "tenant_defect_escalation_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantDefectEscalationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantDefectEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantDefectEscalationSettingsSnapshot ToSnapshot(TenantDefectEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.LowThresholdHours,
            settings.MediumThresholdHours,
            settings.HighThresholdHours,
            settings.CriticalThresholdHours,
            settings.AutoAcknowledgeOnEscalation,
            settings.AutoCreateWorkOrderOnEscalation,
            settings.BumpSeverityOnRepeatEscalation,
            settings.NotifyOnEscalation);

    private static DefectEscalationSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            LowThresholdHours: DefectEscalationDefaults.LowThresholdHours,
            MediumThresholdHours: DefectEscalationDefaults.MediumThresholdHours,
            HighThresholdHours: DefectEscalationDefaults.HighThresholdHours,
            CriticalThresholdHours: DefectEscalationDefaults.CriticalThresholdHours,
            AutoAcknowledgeOnEscalation: true,
            AutoCreateWorkOrderOnEscalation: true,
            BumpSeverityOnRepeatEscalation: true,
            NotifyOnEscalation: true,
            UpdatedAt: null);

    private static DefectEscalationSettingsResponse MapResponse(TenantDefectEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.LowThresholdHours,
            settings.MediumThresholdHours,
            settings.HighThresholdHours,
            settings.CriticalThresholdHours,
            settings.AutoAcknowledgeOnEscalation,
            settings.AutoCreateWorkOrderOnEscalation,
            settings.BumpSeverityOnRepeatEscalation,
            settings.NotifyOnEscalation,
            settings.UpdatedAt);
}
