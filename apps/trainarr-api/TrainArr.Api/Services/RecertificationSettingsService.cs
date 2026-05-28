using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class RecertificationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<RecertificationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantRecertificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null
            ? new RecertificationSettingsResponse(
                IsEnabled: false,
                LeadDays: RecertificationAssignmentRules.DefaultLeadDays,
                UpdatedAt: null)
            : Map(settings);
    }

    public async Task<RecertificationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertRecertificationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantRecertificationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantRecertificationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantRecertificationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.LeadDays = RecertificationAssignmentRules.NormalizeLeadDays(request.LeadDays);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "recertification_settings.upsert",
            tenantId,
            actorUserId,
            "tenant_recertification_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    internal async Task<TenantRecertificationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantRecertificationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantRecertificationSettingsSnapshot ToSnapshot(
        TenantRecertificationSettings settings) =>
        new(settings.TenantId, settings.IsEnabled, settings.LeadDays);

    private static RecertificationSettingsResponse Map(TenantRecertificationSettings settings) =>
        new(settings.IsEnabled, settings.LeadDays, settings.UpdatedAt);
}

public sealed record TenantRecertificationSettingsSnapshot(
    Guid TenantId,
    bool IsEnabled,
    int LeadDays);
