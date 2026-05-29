using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class IntegrationEventSettingsService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<IntegrationEventSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new IntegrationEventSettingsResponse(
                IsEnabled: true,
                MaxAttempts: IntegrationEventRules.DefaultMaxAttempts,
                RetryIntervalMinutes: IntegrationEventRules.DefaultRetryIntervalMinutes,
                UpdatedAt: null);
        }

        return MapResponse(settings);
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
            "routarr.integration_event_settings.update",
            tenantId,
            actorUserId,
            "tenant_integration_event_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantIntegrationEventSettingsSnapshot> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantIntegrationEventSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new TenantIntegrationEventSettingsSnapshot(
                IsEnabled: true,
                MaxAttempts: IntegrationEventRules.DefaultMaxAttempts,
                RetryIntervalMinutes: IntegrationEventRules.DefaultRetryIntervalMinutes)
            : new TenantIntegrationEventSettingsSnapshot(
                settings.IsEnabled,
                settings.MaxAttempts,
                settings.RetryIntervalMinutes);
    }

    private static IntegrationEventSettingsResponse MapResponse(TenantIntegrationEventSettings settings) =>
        new(
            settings.IsEnabled,
            settings.MaxAttempts,
            settings.RetryIntervalMinutes,
            settings.UpdatedAt);
}
