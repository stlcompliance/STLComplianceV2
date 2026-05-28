using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class QualificationRecalculationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<QualificationRecalculationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantQualificationRecalculationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new QualificationRecalculationSettingsResponse(
                false,
                QualificationRecalculationRules.DefaultStalenessHours,
                false,
                null)
            : Map(settings);
    }

    public async Task<QualificationRecalculationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertQualificationRecalculationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantQualificationRecalculationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantQualificationRecalculationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantQualificationRecalculationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = QualificationRecalculationRules.NormalizeStalenessHours(request.StalenessHours);
        entity.AutoSuspendOnBlock = request.AutoSuspendOnBlock;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "qualification_recalculation_settings.upsert",
            tenantId,
            actorUserId,
            "qualification_recalculation_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantQualificationRecalculationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantQualificationRecalculationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantQualificationRecalculationSettingsSnapshot ToSnapshot(
        TenantQualificationRecalculationSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.AutoSuspendOnBlock);

    private static QualificationRecalculationSettingsResponse Map(
        TenantQualificationRecalculationSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.AutoSuspendOnBlock, settings.UpdatedAt);
}

public sealed record TenantQualificationRecalculationSettingsSnapshot(
    bool IsEnabled,
    int StalenessHours,
    bool AutoSuspendOnBlock);
