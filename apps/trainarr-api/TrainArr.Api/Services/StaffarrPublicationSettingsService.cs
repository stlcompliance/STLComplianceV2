using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class StaffarrPublicationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<StaffarrPublicationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantStaffarrPublicationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new StaffarrPublicationSettingsResponse(
                true,
                StaffarrPublicationRules.DefaultMaxAttempts,
                StaffarrPublicationRules.DefaultRetryIntervalMinutes,
                null)
            : Map(settings);
    }

    public async Task<StaffarrPublicationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertStaffarrPublicationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantStaffarrPublicationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantStaffarrPublicationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantStaffarrPublicationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.MaxAttempts = StaffarrPublicationRules.NormalizeMaxAttempts(request.MaxAttempts);
        entity.RetryIntervalMinutes = StaffarrPublicationRules.NormalizeRetryIntervalMinutes(request.RetryIntervalMinutes);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "staffarr_publication_settings.upsert",
            tenantId,
            actorUserId,
            "staffarr_publication_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantStaffarrPublicationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantStaffarrPublicationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantStaffarrPublicationSettingsSnapshot ToSnapshot(
        TenantStaffarrPublicationSettings settings) =>
        new(settings.IsEnabled, settings.MaxAttempts, settings.RetryIntervalMinutes);

    private static StaffarrPublicationSettingsResponse Map(TenantStaffarrPublicationSettings settings) =>
        new(settings.IsEnabled, settings.MaxAttempts, settings.RetryIntervalMinutes, settings.UpdatedAt);
}
