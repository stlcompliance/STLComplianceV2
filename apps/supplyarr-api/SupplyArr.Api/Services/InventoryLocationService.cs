using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class InventoryLocationService(
    SupplyArrDbContext db,
    StaffArrSiteReferenceService staffArrSites,
    ISupplyArrAuditService audit)
{
    private static readonly HashSet<string> AllowedLocationTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "warehouse",
        "parts_room",
        "dock",
        "yard",
        "service_truck",
        "site"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    public async Task<IReadOnlyList<InventoryLocationResponse>> ListLocationsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var locations = await db.InventoryLocations
            .AsNoTracking()
            .Include(x => x.Bins)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return locations.Select(MapLocation).ToList();
    }

    public async Task<InventoryLocationResponse> GetLocationAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadLocationAsync(tenantId, locationId, cancellationToken);
        return MapLocation(entity);
    }

    public async Task<InventoryLocationResponse> CreateLocationAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateInventoryLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var locationKey = NormalizeLocationKey(request.LocationKey);
        var exists = await db.InventoryLocations.AnyAsync(
            x => x.TenantId == tenantId && x.LocationKey == locationKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "inventory.locations.duplicate",
                "An inventory location with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var staffArrSite = await staffArrSites.RequireActiveSiteAsync(
            tenantId,
            request.StaffarrSiteOrgUnitId,
            cancellationToken);
        var entity = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = locationKey,
            Name = NormalizeName(request.Name),
            LocationType = NormalizeLocationType(request.LocationType),
            AddressLine = NormalizeAddressLine(request.AddressLine),
            StaffarrSiteOrgUnitId = staffArrSite.OrgUnitId,
            StaffarrSiteNameSnapshot = staffArrSite.Name,
            StaffarrSiteResolutionStatus = staffArrSite.ResolutionStatus,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.InventoryLocations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_location.create",
            tenantId,
            actorUserId,
            "inventory_location",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLocation(entity);
    }

    public async Task<InventoryLocationResponse> UpdateLocationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid locationId,
        UpdateInventoryLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.InventoryLocations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == locationId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("inventory.locations.not_found", "Inventory location was not found.", 404);
        }

        entity.Name = NormalizeName(request.Name);
        entity.LocationType = NormalizeLocationType(request.LocationType);
        entity.AddressLine = NormalizeAddressLine(request.AddressLine);
        if (request.StaffarrSiteOrgUnitId.HasValue
            && request.StaffarrSiteOrgUnitId != entity.StaffarrSiteOrgUnitId)
        {
            var staffArrSite = await staffArrSites.RequireActiveSiteAsync(
                tenantId,
                request.StaffarrSiteOrgUnitId,
                cancellationToken);
            entity.StaffarrSiteOrgUnitId = staffArrSite.OrgUnitId;
            entity.StaffarrSiteNameSnapshot = staffArrSite.Name;
            entity.StaffarrSiteResolutionStatus = staffArrSite.ResolutionStatus;
        }
        else if (entity.StaffarrSiteOrgUnitId is Guid existingSiteId
            && !string.Equals(entity.StaffarrSiteResolutionStatus, InventoryLocationSiteResolutionStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            var staffArrSite = await staffArrSites.RequireActiveSiteAsync(
                tenantId,
                existingSiteId,
                cancellationToken);
            entity.StaffarrSiteNameSnapshot = staffArrSite.Name;
            entity.StaffarrSiteResolutionStatus = staffArrSite.ResolutionStatus;
        }
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_location.update",
            tenantId,
            actorUserId,
            "inventory_location",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLocation(entity);
    }

    public async Task<InventoryLocationResponse> UpdateLocationStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid locationId,
        UpdateInventoryLocationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.InventoryLocations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == locationId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("inventory.locations.not_found", "Inventory location was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_location.status",
            tenantId,
            actorUserId,
            "inventory_location",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapLocation(entity);
    }

    public async Task<IReadOnlyList<InventoryBinResponse>> ListBinsAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureLocationExistsAsync(tenantId, locationId, cancellationToken);

        var bins = await db.InventoryBins
            .AsNoTracking()
            .Include(x => x.InventoryLocation)
            .Where(x => x.TenantId == tenantId && x.InventoryLocationId == locationId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return bins.Select(MapBin).ToList();
    }

    public async Task<InventoryBinResponse> CreateBinAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid locationId,
        CreateInventoryBinRequest request,
        CancellationToken cancellationToken = default)
    {
        var location = await LoadLocationAsync(tenantId, locationId, cancellationToken);
        var binKey = NormalizeBinKey(request.BinKey);
        var exists = await db.InventoryBins.AnyAsync(
            x => x.TenantId == tenantId
                 && x.InventoryLocationId == locationId
                 && x.BinKey == binKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "inventory.bins.duplicate",
                "A bin with this key already exists at this location.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = locationId,
            BinKey = binKey,
            Name = NormalizeName(request.Name),
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.InventoryBins.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_bin.create",
            tenantId,
            actorUserId,
            "inventory_bin",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        entity.InventoryLocation = location;
        return MapBin(entity);
    }

    public async Task<InventoryBinResponse> UpdateBinAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid binId,
        UpdateInventoryBinRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadBinAsync(tenantId, binId, cancellationToken);
        entity.Name = NormalizeName(request.Name);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_bin.update",
            tenantId,
            actorUserId,
            "inventory_bin",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapBin(entity);
    }

    public async Task<InventoryBinResponse> UpdateBinStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid binId,
        UpdateInventoryBinStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.InventoryBins.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == binId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "inventory_bin.status",
            tenantId,
            actorUserId,
            "inventory_bin",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetBinResponseAsync(tenantId, binId, cancellationToken);
    }

    private async Task<InventoryBinResponse> GetBinResponseAsync(
        Guid tenantId,
        Guid binId,
        CancellationToken cancellationToken)
    {
        var entity = await LoadBinAsync(tenantId, binId, cancellationToken);
        return MapBin(entity);
    }

    private async Task<InventoryLocation> LoadLocationAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var entity = await db.InventoryLocations
            .AsNoTracking()
            .Include(x => x.Bins)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == locationId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("inventory.locations.not_found", "Inventory location was not found.", 404);
        }

        return entity;
    }

    private async Task<InventoryBin> LoadBinAsync(
        Guid tenantId,
        Guid binId,
        CancellationToken cancellationToken)
    {
        var entity = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == binId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        return entity;
    }

    private async Task EnsureLocationExistsAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var exists = await db.InventoryLocations.AnyAsync(
            x => x.TenantId == tenantId && x.Id == locationId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("inventory.locations.not_found", "Inventory location was not found.", 404);
        }
    }

    private static InventoryLocationResponse MapLocation(InventoryLocation entity) =>
        new(
            entity.Id,
            entity.LocationKey,
            entity.Name,
            entity.LocationType,
            entity.AddressLine,
            entity.StaffarrSiteOrgUnitId,
            entity.StaffarrSiteNameSnapshot,
            entity.StaffarrSiteResolutionStatus,
            entity.Status,
            entity.Bins?.Count ?? 0,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static InventoryBinResponse MapBin(InventoryBin entity) =>
        new(
            entity.Id,
            entity.InventoryLocationId,
            entity.InventoryLocation?.LocationKey ?? string.Empty,
            entity.BinKey,
            entity.Name,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeLocationKey(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "inventory.locations.invalid_key",
                "Location key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeBinKey(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized.Length is < 1 or > 128)
        {
            throw new StlApiException(
                "inventory.bins.invalid_key",
                "Bin key must be between 1 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is < 1 or > 256)
        {
            throw new StlApiException(
                "inventory.invalid_name",
                "Name must be between 1 and 256 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLocationType(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedLocationTypes.Contains(normalized))
        {
            throw new StlApiException(
                "inventory.locations.invalid_type",
                "Location type must be warehouse, parts_room, dock, yard, service_truck, or legacy site.",
                400);
        }

        return normalized == "site" ? "parts_room" : normalized;
    }

    private static string NormalizeAddressLine(string value) =>
        (value ?? string.Empty).Trim()[..Math.Min((value ?? string.Empty).Trim().Length, 512)];

    private static string NormalizeStatus(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "inventory.invalid_status",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }
}
