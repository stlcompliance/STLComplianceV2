using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class RulePackImpactSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<RulePackImpactSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantRulePackImpactSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new RulePackImpactSettingsResponse(
                false,
                RulePackImpactRules.DefaultStalenessHours,
                false,
                null)
            : Map(settings);
    }

    public async Task<RulePackImpactSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertRulePackImpactSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantRulePackImpactSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantRulePackImpactSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantRulePackImpactSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = RulePackImpactRules.NormalizeStalenessHours(request.StalenessHours);
        entity.AutoUpdateRequirementBaselines = request.AutoUpdateRequirementBaselines;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "rule_pack_impact_settings.upsert",
            tenantId,
            actorUserId,
            "rule_pack_impact_settings",
            tenantId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantRulePackImpactSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantRulePackImpactSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantRulePackImpactSettingsSnapshot ToSnapshot(
        TenantRulePackImpactSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.AutoUpdateRequirementBaselines);

    private static RulePackImpactSettingsResponse Map(TenantRulePackImpactSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.AutoUpdateRequirementBaselines, settings.UpdatedAt);
}

public sealed record TenantRulePackImpactSettingsSnapshot(
    bool IsEnabled,
    int StalenessHours,
    bool AutoUpdateRequirementBaselines);
