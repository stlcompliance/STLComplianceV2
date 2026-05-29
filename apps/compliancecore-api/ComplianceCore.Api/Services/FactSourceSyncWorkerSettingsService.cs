using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;

namespace ComplianceCore.Api.Services;

public sealed class FactSourceSyncWorkerSettingsService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task<FactSourceSyncWorkerSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantFactSourceSyncWorkerSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<FactSourceSyncWorkerSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertFactSourceSyncWorkerSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantFactSourceSyncWorkerSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantFactSourceSyncWorkerSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantFactSourceSyncWorkerSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.DefaultScopeKey = FactSourceSyncRules.NormalizeScopeKey(request.DefaultScopeKey);
        entity.IntervalMinutes = FactSourceSyncRules.NormalizeIntervalMinutes(request.IntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "fact_source_sync_worker_settings.update",
            tenantId,
            actorUserId,
            "tenant_fact_source_sync_worker_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantFactSourceSyncWorkerSettings?> LoadEnabledSettingsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await db.TenantFactSourceSyncWorkerSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsEnabled, cancellationToken);

    private static FactSourceSyncWorkerSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            DefaultScopeKey: "tenant",
            IntervalMinutes: FactSourceSyncRules.DefaultIntervalMinutes,
            LastBatchRunAt: null,
            UpdatedAt: null);

    private static FactSourceSyncWorkerSettingsResponse MapResponse(TenantFactSourceSyncWorkerSettings settings) =>
        new(
            settings.IsEnabled,
            settings.DefaultScopeKey,
            settings.IntervalMinutes,
            settings.LastBatchRunAt,
            settings.UpdatedAt);
}
