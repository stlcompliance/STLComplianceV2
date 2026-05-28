using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class IntegrationEventSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<IntegrationEventSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new IntegrationEventSettingsResponse(tenantId, true, IntegrationEventRules.DefaultMaxAttempts, IntegrationEventRules.DefaultRetryIntervalMinutes, DateTimeOffset.UtcNow)
            : Map(settings);
    }

    public async Task<IntegrationEventSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertIntegrationEventSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantIntegrationEventSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantIntegrationEventSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantIntegrationEventSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.MaxAttempts = IntegrationEventRules.NormalizeMaxAttempts(request.MaxAttempts);
        entity.RetryIntervalMinutes = IntegrationEventRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.integration_event_settings.update",
            tenantId,
            actorUserId,
            "tenant_integration_event_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantIntegrationEventSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new TenantIntegrationEventSettingsSnapshot(true, IntegrationEventRules.DefaultMaxAttempts, IntegrationEventRules.DefaultRetryIntervalMinutes);
        }

        return new TenantIntegrationEventSettingsSnapshot(
            settings.IsEnabled,
            settings.MaxAttempts,
            settings.RetryIntervalMinutes);
    }

    private static IntegrationEventSettingsResponse Map(TenantIntegrationEventSettings settings) =>
        new(
            settings.TenantId,
            settings.IsEnabled,
            settings.MaxAttempts,
            settings.RetryIntervalMinutes,
            settings.UpdatedAt);
}
