using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public sealed class TripCompletionRollupSettingsService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<TripCompletionRollupSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantTripCompletionRollupSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<TripCompletionRollupSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertTripCompletionRollupSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantTripCompletionRollupSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantTripCompletionRollupSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantTripCompletionRollupSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.StalenessHours = TripCompletionRollupRules.NormalizeStalenessHours(request.StalenessHours);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "routarr.trip_completion_rollup_settings.update",
            tenantId,
            actorUserId,
            "tenant_trip_completion_rollup_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static TripCompletionRollupSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            StalenessHours: TripCompletionRollupDefaults.StalenessHours,
            UpdatedAt: null);

    private static TripCompletionRollupSettingsResponse MapResponse(
        TenantTripCompletionRollupSettings settings) =>
        new(settings.IsEnabled, settings.StalenessHours, settings.UpdatedAt);
}
