using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class VehicleRefService(RoutArrDbContext db)
{
    public async Task<VehicleRefListResponse> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var mirrored = await db.RoutarrVehicleRefs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.VehicleRefKey)
            .Select(x => new VehicleRefResponse(
                x.VehicleRefKey,
                x.DisplayLabel,
                x.AssetTag,
                x.MirroredAt,
                true))
            .ToListAsync(cancellationToken);

        var mirroredKeys = mirrored.Select(x => x.VehicleRefKey).ToHashSet(StringComparer.Ordinal);

        var fromTrips = await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VehicleRefKey != null && x.VehicleRefKey != "")
            .Select(x => x.VehicleRefKey!)
            .Distinct()
            .ToListAsync(cancellationToken);

        var fromEquipment = await db.EquipmentAvailabilities
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.VehicleRefKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        var inferred = fromTrips
            .Concat(fromEquipment)
            .Where(key => !mirroredKeys.Contains(key))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => new VehicleRefResponse(key, key, null, null, false))
            .ToList();

        return new VehicleRefListResponse(mirrored.Concat(inferred).ToList());
    }

    public async Task<VehicleRefResponse> UpsertAsync(
        Guid tenantId,
        Guid? actorUserId,
        UpsertVehicleRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var vehicleRefKey = NormalizeKey(request.VehicleRefKey);
        var displayLabel = NormalizeLabel(request.DisplayLabel, vehicleRefKey);
        var assetTag = string.IsNullOrWhiteSpace(request.AssetTag) ? null : request.AssetTag.Trim();
        var sourceUpdatedAt = request.SourceUpdatedAt ?? DateTimeOffset.UtcNow;
        var now = DateTimeOffset.UtcNow;

        var entity = await db.RoutarrVehicleRefs
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.VehicleRefKey == vehicleRefKey,
                cancellationToken);

        if (entity is null)
        {
            entity = new RoutarrVehicleRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                VehicleRefKey = vehicleRefKey,
                MirroredAt = now
            };
            db.RoutarrVehicleRefs.Add(entity);
        }

        entity.DisplayLabel = displayLabel;
        entity.AssetTag = assetTag;
        entity.SourceUpdatedAt = sourceUpdatedAt;
        entity.MirroredAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return new VehicleRefResponse(
            entity.VehicleRefKey,
            entity.DisplayLabel,
            entity.AssetTag,
            entity.MirroredAt,
            true);
    }

    private static string NormalizeKey(string key)
    {
        var normalized = key.Trim();
        if (normalized.Length is < 1 or > 128)
        {
            throw new StlApiException(
                "vehicle_refs.validation",
                "Vehicle ref key must be between 1 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLabel(string label, string fallback)
    {
        var normalized = label.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? fallback : normalized;
    }
}
