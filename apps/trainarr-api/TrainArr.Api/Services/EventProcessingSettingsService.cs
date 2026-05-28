using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class EventProcessingSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<EventProcessingSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantEventProcessingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new EventProcessingSettingsResponse(
                true,
                EventProcessingRules.DefaultMaxAttempts,
                EventProcessingRules.DefaultRetryIntervalMinutes,
                null)
            : Map(settings);
    }

    public async Task<EventProcessingSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertEventProcessingSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantEventProcessingSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantEventProcessingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantEventProcessingSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.MaxAttempts = EventProcessingRules.NormalizeMaxAttempts(request.MaxAttempts);
        entity.RetryIntervalMinutes = EventProcessingRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "event_processing_settings.upsert",
            tenantId,
            actorUserId,
            "event_processing_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantEventProcessingSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantEventProcessingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantEventProcessingSettingsSnapshot ToSnapshot(TenantEventProcessingSettings settings) =>
        new(settings.IsEnabled, settings.MaxAttempts, settings.RetryIntervalMinutes);

    private static EventProcessingSettingsResponse Map(TenantEventProcessingSettings settings) =>
        new(settings.IsEnabled, settings.MaxAttempts, settings.RetryIntervalMinutes, settings.UpdatedAt);
}
