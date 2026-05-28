using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class EvidenceRetentionSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<EvidenceRetentionSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantEvidenceRetentionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new EvidenceRetentionSettingsResponse(
                false,
                EvidenceRetentionRules.DefaultRetentionDays,
                null)
            : Map(settings);
    }

    public async Task<EvidenceRetentionSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertEvidenceRetentionSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantEvidenceRetentionSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantEvidenceRetentionSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantEvidenceRetentionSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.RetentionDaysAfterAssignmentClose = EvidenceRetentionRules.NormalizeRetentionDays(
            request.RetentionDaysAfterAssignmentClose);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "evidence_retention_settings.upsert",
            tenantId,
            actorUserId,
            "evidence_retention_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantEvidenceRetentionSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantEvidenceRetentionSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantEvidenceRetentionSettingsSnapshot ToSnapshot(
        TenantEvidenceRetentionSettings settings) =>
        new(settings.IsEnabled, settings.RetentionDaysAfterAssignmentClose);

    private static EvidenceRetentionSettingsResponse Map(TenantEvidenceRetentionSettings settings) =>
        new(settings.IsEnabled, settings.RetentionDaysAfterAssignmentClose, settings.UpdatedAt);
}

public sealed record TenantEvidenceRetentionSettingsSnapshot(
    bool IsEnabled,
    int RetentionDaysAfterAssignmentClose);
