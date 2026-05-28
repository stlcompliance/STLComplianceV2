using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class M12AnalyticsWorkerSettingsService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<M12AnalyticsWorkerSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantM12AnalyticsWorkerSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<M12AnalyticsWorkerSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertM12AnalyticsWorkerSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantM12AnalyticsWorkerSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantM12AnalyticsWorkerSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantM12AnalyticsWorkerSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.DefaultScopeKey = M12AnalyticsBatchRules.NormalizeScopeKey(request.DefaultScopeKey);
        entity.IntervalHours = M12AnalyticsBatchRules.NormalizeIntervalHours(request.IntervalHours);
        entity.RiskScoringEnabled = request.RiskScoringEnabled ?? entity.RiskScoringEnabled;
        entity.MissingEvidenceEnabled = request.MissingEvidenceEnabled ?? entity.MissingEvidenceEnabled;
        entity.ControlEffectivenessEnabled = request.ControlEffectivenessEnabled ?? entity.ControlEffectivenessEnabled;
        entity.ReadinessForecastEnabled = request.ReadinessForecastEnabled ?? entity.ReadinessForecastEnabled;
        entity.AuditDeliveryEnabled = request.AuditDeliveryEnabled ?? entity.AuditDeliveryEnabled;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "m12_analytics_worker_settings.update",
            tenantId,
            actorUserId,
            "tenant_m12_analytics_worker_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal static M12AnalyticsWorkerSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            DefaultScopeKey: "tenant",
            IntervalHours: M12AnalyticsBatchRules.DefaultIntervalHours,
            RiskScoringEnabled: true,
            MissingEvidenceEnabled: true,
            ControlEffectivenessEnabled: true,
            ReadinessForecastEnabled: true,
            AuditDeliveryEnabled: false,
            LastBatchRunAt: null,
            LastRiskScoringRunAt: null,
            LastMissingEvidenceRunAt: null,
            LastControlEffectivenessRunAt: null,
            LastReadinessForecastRunAt: null,
            LastAuditDeliveryRunAt: null,
            UpdatedAt: null);

    private static M12AnalyticsWorkerSettingsResponse MapResponse(TenantM12AnalyticsWorkerSettings settings) =>
        new(
            settings.IsEnabled,
            settings.DefaultScopeKey,
            settings.IntervalHours,
            settings.RiskScoringEnabled,
            settings.MissingEvidenceEnabled,
            settings.ControlEffectivenessEnabled,
            settings.ReadinessForecastEnabled,
            settings.AuditDeliveryEnabled,
            settings.LastBatchRunAt,
            settings.LastRiskScoringRunAt,
            settings.LastMissingEvidenceRunAt,
            settings.LastControlEffectivenessRunAt,
            settings.LastReadinessForecastRunAt,
            settings.LastAuditDeliveryRunAt,
            settings.UpdatedAt);
}
