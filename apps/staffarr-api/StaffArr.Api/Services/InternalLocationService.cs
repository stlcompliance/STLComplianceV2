using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class InternalLocationService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<InternalLocationResponse>> ListAsync(
        Guid tenantId,
        bool includeArchived,
        string? search,
        string? type,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        var query = db.InternalLocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!includeArchived)
        {
            query = query.Where(x => x.Status != "archived");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term)
                || x.LocationNumber.ToLower().Contains(term)
                || (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToLowerInvariant();
            query = query.Where(x => x.LocationType == normalizedType);
        }

        if (siteOrgUnitId is Guid requestedSiteId)
        {
            query = query.Where(x => x.SiteOrgUnitId == requestedSiteId);
        }

        var locations = await query.ToListAsync(cancellationToken);
        return await ProjectAsync(tenantId, locations, treeOrder: false, cancellationToken);
    }

    public async Task<IReadOnlyList<InternalLocationResponse>> ListTreeAsync(
        Guid tenantId,
        bool includeArchived,
        string? search,
        string? type,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        var query = db.InternalLocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!includeArchived)
        {
            query = query.Where(x => x.Status != "archived");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term)
                || x.LocationNumber.ToLower().Contains(term)
                || (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToLowerInvariant();
            query = query.Where(x => x.LocationType == normalizedType);
        }

        if (siteOrgUnitId is Guid requestedSiteId)
        {
            query = query.Where(x => x.SiteOrgUnitId == requestedSiteId);
        }

        var locations = await query.ToListAsync(cancellationToken);
        return await ProjectAsync(tenantId, locations, treeOrder: true, cancellationToken);
    }

    public async Task<InternalLocationResponse> GetAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var location = await db.InternalLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == locationId, cancellationToken);

        return location is null
            ? throw new StlApiException("location.not_found", "Location was not found.", 404)
            : await ProjectOneAsync(tenantId, location, cancellationToken);
    }

    public async Task<InternalLocationResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateInternalLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = await NormalizeRequestAsync(
            tenantId,
            currentLocationId: null,
            request.Name,
            request.LocationType,
            request.ParentLocationId,
            request.SiteOrgUnitId,
            request.Code,
            request.Description,
            request.Status,
            request.AllowedProductUsage,
            cancellationToken);

        var locationId = Guid.NewGuid();
        var locationNumber = NormalizeOrGenerateCode(normalized.LocationNumber, locationId);
        await EnsureNoDuplicateAsync(
            tenantId,
            normalized.SiteOrgUnitId,
            normalized.ParentLocationId,
            locationNumber,
            null,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var location = new InternalLocation
        {
            Id = locationId,
            TenantId = tenantId,
            LocationNumber = locationNumber,
            Name = normalized.Name,
            Description = normalized.Description,
            LocationType = normalized.LocationType,
            ParentLocationId = normalized.ParentLocationId,
            SiteOrgUnitId = normalized.SiteOrgUnitId,
            Status = normalized.Status,
            AllowedProductUsage = normalized.AllowedProductUsage,
            ArchivedAt = null,
            ArchivedByUserId = null,
            ArchiveReason = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.InternalLocations.Add(location);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "location.create",
            tenantId,
            actorUserId,
            "internal_location",
            location.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, location.Id, cancellationToken);
    }

    public async Task<InternalLocationResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid locationId,
        UpdateInternalLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var location = await db.InternalLocations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == locationId,
            cancellationToken);
        if (location is null)
        {
            throw new StlApiException("location.not_found", "Location was not found.", 404);
        }

        var normalized = await NormalizeRequestAsync(
            tenantId,
            locationId,
            request.Name,
            request.LocationType,
            request.ParentLocationId,
            request.SiteOrgUnitId,
            request.Code,
            request.Description,
            request.Status,
            request.AllowedProductUsage,
            cancellationToken);

        var locationNumber = NormalizeOrGenerateCode(normalized.LocationNumber, location.Id, location.LocationNumber);
        await EnsureNoDuplicateAsync(
            tenantId,
            normalized.SiteOrgUnitId,
            normalized.ParentLocationId,
            locationNumber,
            locationId,
            cancellationToken);

        location.LocationNumber = locationNumber;
        location.Name = normalized.Name;
        location.Description = normalized.Description;
        location.LocationType = normalized.LocationType;
        location.ParentLocationId = normalized.ParentLocationId;
        location.SiteOrgUnitId = normalized.SiteOrgUnitId;
        location.Status = normalized.Status;
        location.AllowedProductUsage = normalized.AllowedProductUsage;
        location.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "location.update",
            tenantId,
            actorUserId,
            "internal_location",
            location.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, location.Id, cancellationToken);
    }

    public async Task<InternalLocationResponse> ArchiveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid locationId,
        ArchiveInternalLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        var reason = NormalizeRequiredText(request.Reason, 512, "Archive reason");

        var location = await db.InternalLocations.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == locationId,
            cancellationToken);
        if (location is null)
        {
            throw new StlApiException("location.not_found", "Location was not found.", 404);
        }

        await EnsureCanArchiveAsync(tenantId, locationId, cancellationToken);

        location.Status = "archived";
        location.ArchivedAt = DateTimeOffset.UtcNow;
        location.ArchivedByUserId = actorUserId;
        location.ArchiveReason = reason;
        location.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "location.archive",
            tenantId,
            actorUserId,
            "internal_location",
            location.Id.ToString(),
            "Succeeded",
            reasonCode: reason,
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, location.Id, cancellationToken);
    }

    public static StaffArrIntegrationLocationResponse ToIntegrationResponse(InternalLocationResponse response) =>
        new(
            response.LocationId,
            response.TenantId,
            response.LocationNumber,
            response.Name,
            response.LocationType,
            response.ParentLocationId,
            response.SiteOrgUnitId,
            response.SiteNameSnapshot,
            response.ParentPathSnapshot,
            response.Status,
            response.AllowedProductUsage,
            response.Description,
            response.ArchivedAt,
            response.ArchivedByUserId,
            response.ArchiveReason);

    private async Task<IReadOnlyList<InternalLocationResponse>> ProjectAsync(
        Guid tenantId,
        IReadOnlyList<InternalLocation> locations,
        bool treeOrder,
        CancellationToken cancellationToken)
    {
        if (locations.Count == 0)
        {
            return [];
        }

        var siteIds = locations
            .Select(x => x.SiteOrgUnitId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        var sitesById = await db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && siteIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var peopleHomeBaseCounts = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.HomeBaseLocationId != null)
            .GroupBy(x => x.HomeBaseLocationId!.Value)
            .Select(group => new { LocationId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.LocationId, x => x.Count, cancellationToken);

        var byId = locations.ToDictionary(x => x.Id);
        var childrenByParent = new Dictionary<Guid, List<InternalLocation>>();
        foreach (var group in locations.GroupBy(x => x.ParentLocationId))
        {
            if (group.Key is Guid parentId)
            {
                childrenByParent[parentId] = group
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }
        var descendantCounts = new Dictionary<Guid, int>();

        int CountDescendants(Guid locationId)
        {
            if (descendantCounts.TryGetValue(locationId, out var cached))
            {
                return cached;
            }

            var total = 0;
            if (childrenByParent.TryGetValue(locationId, out var children))
            {
                foreach (var child in children)
                {
                    total += 1 + CountDescendants(child.Id);
                }
            }

            descendantCounts[locationId] = total;
            return total;
        }

        string BuildPath(InternalLocation location)
        {
            var path = new List<string> { location.Name };
            var cursor = location.ParentLocationId;
            while (cursor is Guid parentId && byId.TryGetValue(parentId, out var parent))
            {
                path.Add(parent.Name);
                cursor = parent.ParentLocationId;
            }

            path.Reverse();
            return string.Join(" / ", path);
        }

        InternalLocationResponse Project(InternalLocation location)
        {
            var siteName = location.SiteOrgUnitId is Guid siteId && sitesById.TryGetValue(siteId, out var site)
                ? site.Name
                : location.SiteOrgUnitId?.ToString() ?? string.Empty;
            return new InternalLocationResponse(
                location.Id,
                tenantId,
                location.LocationNumber,
                location.Name,
                location.LocationType,
                location.ParentLocationId,
                location.SiteOrgUnitId,
                siteName,
                BuildPath(location),
                location.Status,
                location.AllowedProductUsage,
                location.LocationNumber,
                location.Description,
                location.ArchivedAt,
                location.ArchivedByUserId,
                location.ArchiveReason,
                CountDescendants(location.Id),
                peopleHomeBaseCounts.TryGetValue(location.Id, out var count) ? count : 0);
        }

        IEnumerable<InternalLocation> orderedLocations;
        if (treeOrder)
        {
            var roots = locations
                .Where(x => x.ParentLocationId is null || !byId.ContainsKey(x.ParentLocationId.Value))
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var ordered = new List<InternalLocation>();

            void Visit(InternalLocation current)
            {
                ordered.Add(current);
                if (!childrenByParent.TryGetValue(current.Id, out var children))
                {
                    return;
                }

                foreach (var child in children)
                {
                    Visit(child);
                }
            }

            foreach (var root in roots)
            {
                Visit(root);
            }

            orderedLocations = ordered;
        }
        else
        {
            orderedLocations = locations
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.LocationNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return orderedLocations.Select(Project).ToList();
    }

    private async Task<InternalLocationResponse> ProjectOneAsync(
        Guid tenantId,
        InternalLocation location,
        CancellationToken cancellationToken)
    {
        var projected = await ProjectAsync(tenantId, [location], treeOrder: false, cancellationToken);
        return projected[0]!;
    }

    private async Task<NormalizedInternalLocationRequest> NormalizeRequestAsync(
        Guid tenantId,
        Guid? currentLocationId,
        string name,
        string locationType,
        Guid? parentLocationId,
        Guid? siteOrgUnitId,
        string? locationNumber,
        string? description,
        string status,
        string allowedProductUsage,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeRequiredText(name, 128, "Name");
        var normalizedLocationType = NormalizeRequiredCatalogValue(
            locationType,
            OrgStructureCatalog.LocationTypes,
            "location.validation",
            "Location type is required and must be supported.");
        var normalizedStatus = NormalizeRequiredCatalogValue(
            status,
            OrgStructureCatalog.LocationStatuses,
            "location.validation",
            "Status is required and must be planned, active, inactive, restricted, or archived.");
        if (string.Equals(normalizedStatus, "archived", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "location.validation",
                "Use the archive endpoint to archive locations.",
                400);
        }

        var normalizedUsage = NormalizeRequiredCatalogValue(
            allowedProductUsage,
            OrgStructureCatalog.AllowedProductUsages,
            "location.validation",
            "Allowed product usage must be supported.");
        var normalizedDescription = NormalizeOptionalText(description, 512, "Description");
        var normalizedNumber = string.IsNullOrWhiteSpace(locationNumber)
            ? null
            : OrgStructureCatalog.NormalizeCode(locationNumber);

        await EnsureSiteIsValidAsync(tenantId, siteOrgUnitId, cancellationToken);
        await EnsureParentIsValidAsync(tenantId, currentLocationId, normalizedLocationType, parentLocationId, siteOrgUnitId, cancellationToken);

        return new NormalizedInternalLocationRequest(
            normalizedName,
            normalizedLocationType,
            parentLocationId,
            siteOrgUnitId,
            normalizedNumber,
            normalizedDescription,
            normalizedStatus,
            normalizedUsage);
    }

    private async Task EnsureSiteIsValidAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (siteOrgUnitId is not Guid requestedSiteId)
        {
            throw new StlApiException("location.site_not_found", "Site org unit is required.", 400);
        }

        var site = await db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == requestedSiteId)
            .Select(x => new { x.Id, x.UnitType, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (site is null)
        {
            throw new StlApiException("location.site_not_found", "Site org unit was not found.", 404);
        }

        if (!string.Equals(site.UnitType, "site", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("location.validation", "Internal locations must belong to a site org unit.", 409);
        }

        if (!OrgStructureCatalog.IsSelectableOrgUnitStatus(site.Status))
        {
            throw new StlApiException("location.validation", "Site org unit must be planned or active.", 409);
        }
    }

    private async Task EnsureParentIsValidAsync(
        Guid tenantId,
        Guid? currentLocationId,
        string locationType,
        Guid? parentLocationId,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (parentLocationId is null)
        {
            return;
        }

        if (currentLocationId == parentLocationId)
        {
            throw new StlApiException("location.validation", "Location cannot be its own parent.", 400);
        }

        var parent = await db.InternalLocations.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == parentLocationId)
            .Select(x => new { x.Id, x.ParentLocationId, x.SiteOrgUnitId, x.Status, x.LocationType })
            .FirstOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            throw new StlApiException("location.parent_not_found", "Parent location was not found.", 404);
        }

        if (!OrgStructureCatalog.IsSelectableLocationStatus(parent.Status))
        {
            throw new StlApiException("location.validation", "Parent location must be planned, active, or restricted.", 409);
        }

        if (siteOrgUnitId.HasValue && parent.SiteOrgUnitId != siteOrgUnitId)
        {
            throw new StlApiException("location.validation", "Parent location must belong to the selected site.", 409);
        }

        if (currentLocationId is null)
        {
            return;
        }

        var cursor = parent.ParentLocationId;
        while (cursor is Guid candidateId)
        {
            if (candidateId == currentLocationId.Value)
            {
                throw new StlApiException("location.validation", "Location hierarchy cannot contain cycles.", 409);
            }

            cursor = await db.InternalLocations
                .Where(x => x.TenantId == tenantId && x.Id == candidateId)
                .Select(x => x.ParentLocationId)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }

    private async Task EnsureNoDuplicateAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        Guid? parentLocationId,
        string locationNumber,
        Guid? excludedLocationId,
        CancellationToken cancellationToken)
    {
        var duplicateExists = await db.InternalLocations.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (excludedLocationId == null || x.Id != excludedLocationId.Value)
                && x.LocationNumber == locationNumber
                && (parentLocationId.HasValue
                    ? x.ParentLocationId == parentLocationId
                    : x.ParentLocationId == null)
                && (siteOrgUnitId.HasValue ? x.SiteOrgUnitId == siteOrgUnitId : x.SiteOrgUnitId == null),
            cancellationToken);

        if (duplicateExists)
        {
            throw new StlApiException(
                "location.code_conflict",
                "A location with that code already exists under the same parent.",
                409);
        }
    }

    private async Task EnsureCanArchiveAsync(
        Guid tenantId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var hasActiveChildren = await db.InternalLocations.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.ParentLocationId == locationId
                && x.Status != "archived",
            cancellationToken);

        if (hasActiveChildren)
        {
            throw new StlApiException(
                "location.status_conflict",
                "Cannot archive a location that still has active child locations.",
                409);
        }
    }

    private static string NormalizeOrGenerateCode(string? requestedCode, Guid id, string? existingCode = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedCode))
        {
            return OrgStructureCatalog.NormalizeCode(requestedCode);
        }

        if (!string.IsNullOrWhiteSpace(existingCode))
        {
            return existingCode;
        }

        return OrgStructureCatalog.BuildStableCode("LOC", id);
    }

    private static string NormalizeRequiredCatalogValue(
        string? value,
        IReadOnlyCollection<string> allowedValues,
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(errorCode, errorMessage, 400);
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(errorCode, errorMessage, 400);
        }

        return normalized;
    }

    private static string NormalizeRequiredText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("location.validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("location.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("location.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private sealed record NormalizedInternalLocationRequest(
        string Name,
        string LocationType,
        Guid? ParentLocationId,
        Guid? SiteOrgUnitId,
        string? LocationNumber,
        string? Description,
        string Status,
        string AllowedProductUsage);
}
