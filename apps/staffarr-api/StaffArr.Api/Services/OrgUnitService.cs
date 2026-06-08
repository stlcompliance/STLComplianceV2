using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Services;

public sealed class OrgUnitService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<OrgUnitResponse>> ListAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await ListAsync(
            tenantId,
            includeArchived: true,
            search: null,
            type: null,
            cancellationToken);
    }

    public async Task<IReadOnlyList<OrgUnitResponse>> ListAsync(
        Guid tenantId,
        bool includeArchived,
        string? search,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrgUnits.AsNoTracking()
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
                || (x.Code != null && x.Code.ToLower().Contains(term))
                || (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToLowerInvariant();
            query = query.Where(x => x.UnitType == normalizedType);
        }

        var units = await query.ToListAsync(cancellationToken);
        return await ProjectAsync(tenantId, units, treeOrder: false, cancellationToken);
    }

    public async Task<IReadOnlyList<OrgUnitResponse>> ListTreeAsync(
        Guid tenantId,
        bool includeArchived,
        string? search,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrgUnits.AsNoTracking()
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
                || (x.Code != null && x.Code.ToLower().Contains(term))
                || (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToLowerInvariant();
            query = query.Where(x => x.UnitType == normalizedType);
        }

        var units = await query.ToListAsync(cancellationToken);
        return await ProjectAsync(tenantId, units, treeOrder: true, cancellationToken);
    }

    public async Task<OrgUnitResponse> GetAsync(
        Guid tenantId,
        Guid orgUnitId,
        CancellationToken cancellationToken = default)
    {
        var orgUnit = await db.OrgUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == orgUnitId, cancellationToken);

        return orgUnit is null
            ? throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404)
            : await ProjectOneAsync(tenantId, orgUnit, cancellationToken);
    }

    public async Task<IReadOnlyList<StaffArrSiteLookupResponse>> ListActiveSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.OrgUnits
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.UnitType == "site"
                && x.Status == "active")
            .OrderBy(x => x.Name)
            .Select(x => new StaffArrSiteLookupResponse(
                x.Id,
                x.Name,
                x.ParentOrgUnitId,
                x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffArrSiteLookupResponse> GetActiveSiteAsync(
        Guid tenantId,
        Guid orgUnitId,
        CancellationToken cancellationToken = default)
    {
        var site = await db.OrgUnits
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.Id == orgUnitId
                && x.UnitType == "site"
                && x.Status == "active")
            .Select(x => new StaffArrSiteLookupResponse(
                x.Id,
                x.Name,
                x.ParentOrgUnitId,
                x.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return site ?? throw new StlApiException(
            "staffarr.sites.not_found",
            "Active StaffArr site was not found.",
            404);
    }

    private async Task<IReadOnlyList<OrgUnitResponse>> ProjectAsync(
        Guid tenantId,
        IReadOnlyList<OrgUnit> orgUnits,
        bool treeOrder,
        CancellationToken cancellationToken)
    {
        if (orgUnits.Count == 0)
        {
            return [];
        }

        var people = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.PrimaryOrgUnitId })
            .ToListAsync(cancellationToken);
        var assignments = await db.OrgUnitAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new
            {
                x.SiteOrgUnitId,
                x.DepartmentOrgUnitId,
                x.TeamOrgUnitId,
                x.PositionOrgUnitId
            })
            .ToListAsync(cancellationToken);

        var byId = orgUnits.ToDictionary(x => x.Id);
        var childrenByParent = new Dictionary<Guid, List<OrgUnit>>();
        foreach (var group in orgUnits.GroupBy(x => x.ParentOrgUnitId))
        {
            if (group.Key is Guid parentId)
            {
                childrenByParent[parentId] = group
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }
        var descendantCounts = new Dictionary<Guid, int>();

        int CountDescendants(Guid orgUnitId)
        {
            if (descendantCounts.TryGetValue(orgUnitId, out var cached))
            {
                return cached;
            }

            var total = 0;
            if (childrenByParent.TryGetValue(orgUnitId, out var children))
            {
                foreach (var child in children)
                {
                    total += 1 + CountDescendants(child.Id);
                }
            }

            descendantCounts[orgUnitId] = total;
            return total;
        }

        int CountAssignments(Guid orgUnitId) =>
            assignments.Count(x =>
                x.SiteOrgUnitId == orgUnitId
                || x.DepartmentOrgUnitId == orgUnitId
                || x.TeamOrgUnitId == orgUnitId
                || x.PositionOrgUnitId == orgUnitId)
            + people.Count(x => x.PrimaryOrgUnitId == orgUnitId);

        OrgUnitResponse Project(OrgUnit orgUnit) =>
            ToResponse(
                orgUnit,
                CountDescendants(orgUnit.Id),
                CountAssignments(orgUnit.Id));

        IEnumerable<OrgUnit> orderedUnits;
        if (treeOrder)
        {
            var roots = orgUnits
                .Where(x => x.ParentOrgUnitId is null || !byId.ContainsKey(x.ParentOrgUnitId.Value))
                .OrderBy(x => x.UnitType)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var ordered = new List<OrgUnit>();

            void Visit(OrgUnit current)
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

            orderedUnits = ordered;
        }
        else
        {
            orderedUnits = orgUnits
                .OrderBy(x => x.UnitType)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return orderedUnits.Select(Project).ToList();
    }

    private async Task<OrgUnitResponse> ProjectOneAsync(
        Guid tenantId,
        OrgUnit orgUnit,
        CancellationToken cancellationToken)
    {
        var projected = await ProjectAsync(tenantId, [orgUnit], treeOrder: false, cancellationToken);
        return projected[0]!;
    }

    public async Task<OrgUnitResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateOrgUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = await NormalizeRequestAsync(
            tenantId,
            currentOrgUnitId: null,
            request.UnitType,
            request.Name,
            request.ParentOrgUnitId,
            request.Code,
            request.Description,
            request.ManagerPersonId,
            request.EffectiveStartDate,
            request.EffectiveEndDate,
            request.SiteType,
            request.Timezone,
            request.Phone,
            request.EmergencyContact,
            request.TeamType,
            request.PositionCode,
            request.DefaultSiteOrgUnitId,
            request.ComplianceSensitive,
            request.SafetySensitive,
            request.CanSupervise,
            request.CanApprove,
            request.Status,
            allowTypeChange: true,
            cancellationToken);

        var orgUnitId = Guid.NewGuid();
        var normalizedCode = NormalizeOrGenerateCode(normalized.UnitType, normalized.Code, orgUnitId);
        await EnsureNoDuplicateAsync(
            tenantId,
            normalized.UnitType,
            normalized.Name,
            normalizedCode,
            normalized.ParentOrgUnitId,
            null,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var orgUnit = new OrgUnit
        {
            Id = orgUnitId,
            TenantId = tenantId,
            UnitType = normalized.UnitType,
            Name = normalized.Name,
            Code = normalizedCode,
            Description = normalized.Description,
            ParentOrgUnitId = normalized.ParentOrgUnitId,
            ManagerPersonId = normalized.ManagerPersonId,
            EffectiveStartDate = normalized.EffectiveStartDate,
            EffectiveEndDate = normalized.EffectiveEndDate,
            Status = normalized.Status ?? "planned",
            SiteType = normalized.SiteType,
            Timezone = normalized.Timezone,
            Phone = normalized.Phone,
            EmergencyContact = normalized.EmergencyContact,
            TeamType = normalized.TeamType,
            PositionCode = normalized.PositionCode,
            DefaultSiteOrgUnitId = normalized.DefaultSiteOrgUnitId,
            ComplianceSensitive = normalized.ComplianceSensitive,
            SafetySensitive = normalized.SafetySensitive,
            CanSupervise = normalized.CanSupervise,
            CanApprove = normalized.CanApprove,
            ArchivedAt = null,
            ArchivedByUserId = null,
            ArchiveReason = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.OrgUnits.Add(orgUnit);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "org_unit.create",
            tenantId,
            actorUserId,
            "org_unit",
            orgUnit.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, orgUnit.Id, cancellationToken);
    }

    public async Task<OrgUnitResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid orgUnitId,
        UpdateOrgUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == orgUnitId,
            cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404);
        }

        var normalized = await NormalizeRequestAsync(
            tenantId,
            orgUnitId,
            request.UnitType,
            request.Name,
            request.ParentOrgUnitId,
            request.Code,
            request.Description,
            request.ManagerPersonId,
            request.EffectiveStartDate,
            request.EffectiveEndDate,
            request.SiteType,
            request.Timezone,
            request.Phone,
            request.EmergencyContact,
            request.TeamType,
            request.PositionCode,
            request.DefaultSiteOrgUnitId,
            request.ComplianceSensitive,
            request.SafetySensitive,
            request.CanSupervise,
            request.CanApprove,
            request.Status,
            allowTypeChange: string.Equals(orgUnit.UnitType, request.UnitType?.Trim(), StringComparison.OrdinalIgnoreCase),
            cancellationToken);

        if (!string.Equals(orgUnit.UnitType, normalized.UnitType, StringComparison.Ordinal))
        {
            await EnsureTypeChangeIsSafeAsync(tenantId, orgUnitId, cancellationToken);
        }

        var normalizedCode = NormalizeOrGenerateCode(normalized.UnitType, normalized.Code, orgUnit.Id, orgUnit.Code);
        await EnsureNoDuplicateAsync(
            tenantId,
            normalized.UnitType,
            normalized.Name,
            normalizedCode,
            normalized.ParentOrgUnitId,
            orgUnitId,
            cancellationToken);

        if (normalized.Status is not null && normalized.Status != orgUnit.Status)
        {
            await EnsureStatusTransitionValidAsync(tenantId, orgUnitId, normalized.Status, cancellationToken);
        }

        orgUnit.UnitType = normalized.UnitType;
        orgUnit.Name = normalized.Name;
        orgUnit.Code = normalizedCode;
        orgUnit.Description = normalized.Description;
        orgUnit.ParentOrgUnitId = normalized.ParentOrgUnitId;
        orgUnit.ManagerPersonId = normalized.ManagerPersonId;
        orgUnit.EffectiveStartDate = normalized.EffectiveStartDate;
        orgUnit.EffectiveEndDate = normalized.EffectiveEndDate;
        orgUnit.Status = normalized.Status ?? orgUnit.Status;
        orgUnit.SiteType = normalized.SiteType;
        orgUnit.Timezone = normalized.Timezone;
        orgUnit.Phone = normalized.Phone;
        orgUnit.EmergencyContact = normalized.EmergencyContact;
        orgUnit.TeamType = normalized.TeamType;
        orgUnit.PositionCode = normalized.PositionCode;
        orgUnit.DefaultSiteOrgUnitId = normalized.DefaultSiteOrgUnitId;
        orgUnit.ComplianceSensitive = normalized.ComplianceSensitive;
        orgUnit.SafetySensitive = normalized.SafetySensitive;
        orgUnit.CanSupervise = normalized.CanSupervise;
        orgUnit.CanApprove = normalized.CanApprove;
        orgUnit.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "org_unit.update",
            tenantId,
            actorUserId,
            "org_unit",
            orgUnit.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, orgUnit.Id, cancellationToken);
    }

    public async Task<OrgUnitResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid orgUnitId,
        UpdateOrgUnitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = NormalizeRequiredCatalogValue(
            request.Status,
            OrgStructureCatalog.OrgUnitStatuses,
            "org_unit.validation",
            "Status is required and must be planned, active, inactive, or archived.");

        if (normalizedStatus == "archived")
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new StlApiException(
                    "org_unit.validation",
                    "Archive reason is required when archiving an org unit.",
                    400);
            }

            return await ArchiveAsync(
                tenantId,
                actorUserId,
                orgUnitId,
                new ArchiveOrgUnitRequest(request.Reason),
                cancellationToken);
        }

        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == orgUnitId,
            cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404);
        }

        await EnsureStatusTransitionValidAsync(tenantId, orgUnitId, normalizedStatus, cancellationToken);

        orgUnit.Status = normalizedStatus;
        orgUnit.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "org_unit.status_update",
            tenantId,
            actorUserId,
            "org_unit",
            orgUnit.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, orgUnit.Id, cancellationToken);
    }

    public async Task<OrgUnitResponse> ArchiveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid orgUnitId,
        ArchiveOrgUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var reason = NormalizeRequiredText(request.Reason, 512, "Archive reason");

        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == orgUnitId,
            cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404);
        }

        await EnsureStatusTransitionValidAsync(tenantId, orgUnitId, "archived", cancellationToken);

        orgUnit.Status = "archived";
        orgUnit.ArchivedAt = DateTimeOffset.UtcNow;
        orgUnit.ArchivedByUserId = actorUserId;
        orgUnit.ArchiveReason = reason;
        orgUnit.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "org_unit.archive",
            tenantId,
            actorUserId,
            "org_unit",
            orgUnit.Id.ToString(),
            "Succeeded",
            reasonCode: reason,
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, orgUnit.Id, cancellationToken);
    }

    private static OrgUnitResponse ToResponse(
        OrgUnit orgUnit,
        int descendantCount = 0,
        int assignmentCount = 0) =>
        new(
            orgUnit.Id,
            orgUnit.UnitType,
            orgUnit.Name,
            orgUnit.ParentOrgUnitId,
            orgUnit.Status,
            orgUnit.Description,
            orgUnit.ManagerPersonId,
            orgUnit.EffectiveStartDate,
            orgUnit.EffectiveEndDate,
            orgUnit.SiteType,
            orgUnit.Timezone,
            orgUnit.Phone,
            orgUnit.EmergencyContact,
            orgUnit.TeamType,
            orgUnit.PositionCode,
            orgUnit.DefaultSiteOrgUnitId,
            orgUnit.ComplianceSensitive,
            orgUnit.SafetySensitive,
            orgUnit.CanSupervise,
            orgUnit.CanApprove,
            orgUnit.Code,
            orgUnit.ArchivedAt,
            orgUnit.ArchivedByUserId,
            orgUnit.ArchiveReason,
            descendantCount,
            assignmentCount);

    private async Task<NormalizedOrgUnitRequest> NormalizeRequestAsync(
        Guid tenantId,
        Guid? currentOrgUnitId,
        string unitType,
        string name,
        Guid? parentOrgUnitId,
        string? code,
        string? description,
        Guid? managerPersonId,
        DateTimeOffset? effectiveStartDate,
        DateTimeOffset? effectiveEndDate,
        string? siteType,
        string? timezone,
        string? phone,
        string? emergencyContact,
        string? teamType,
        string? positionCode,
        Guid? defaultSiteOrgUnitId,
        bool complianceSensitive,
        bool safetySensitive,
        bool canSupervise,
        bool canApprove,
        string? status,
        bool allowTypeChange,
        CancellationToken cancellationToken)
    {
        var normalizedUnitType = NormalizeRequiredCatalogValue(
            unitType,
            OrgStructureCatalog.OrgUnitTypes,
            "org_unit.validation",
            "Unit type is required and must be a supported StaffArr org unit type.");
        var normalizedName = NormalizeRequiredText(name, 128, "Name");
        var normalizedDescription = NormalizeOptionalText(description, 512, "Description");
        var normalizedStatus = status is null
            ? null
            : NormalizeRequiredCatalogValue(
                status,
                OrgStructureCatalog.OrgUnitStatuses,
                "org_unit.validation",
                "Status must be planned, active, inactive, or archived.");
        if (string.Equals(normalizedStatus, "archived", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "org_unit.validation",
                "Use the archive endpoint to archive org units.",
                400);
        }

        ValidateEffectiveDates(effectiveStartDate, effectiveEndDate);
        ValidateTypedFieldUsage(
            normalizedUnitType,
            siteType,
            timezone,
            phone,
            emergencyContact,
            teamType,
            positionCode,
            defaultSiteOrgUnitId);

        await EnsureParentIsValidAsync(tenantId, currentOrgUnitId, normalizedUnitType, parentOrgUnitId, cancellationToken);
        await EnsureManagerIsValidAsync(tenantId, managerPersonId, cancellationToken);
        await EnsureDefaultSiteIsValidAsync(tenantId, normalizedUnitType, defaultSiteOrgUnitId, cancellationToken);

        if (!allowTypeChange && currentOrgUnitId.HasValue)
        {
            throw new StlApiException(
                "org_unit.type_conflict",
                "Changing the type of an in-use org unit is not supported.",
                409);
        }

        return new NormalizedOrgUnitRequest(
            normalizedUnitType,
            normalizedName,
            code is null ? null : NormalizeCodeValue(code),
            normalizedDescription,
            parentOrgUnitId,
            managerPersonId,
            effectiveStartDate,
            effectiveEndDate,
            NormalizeOptionalCatalogValue(siteType, OrgStructureCatalog.SiteTypes, "Site type"),
            NormalizeOptionalText(timezone, 64, "Timezone"),
            NormalizeOptionalText(phone, 32, "Phone"),
            NormalizeOptionalText(emergencyContact, 256, "Emergency contact"),
            NormalizeOptionalCatalogValue(teamType, OrgStructureCatalog.TeamTypes, "Team type"),
            NormalizeOptionalText(positionCode, 64, "Position code"),
            defaultSiteOrgUnitId,
            complianceSensitive,
            safetySensitive,
            canSupervise,
            canApprove,
            normalizedStatus);
    }

    private static void ValidateTypedFieldUsage(
        string unitType,
        string? siteType,
        string? timezone,
        string? phone,
        string? emergencyContact,
        string? teamType,
        string? positionCode,
        Guid? defaultSiteOrgUnitId)
    {
        if (unitType != "site" && (siteType is not null || timezone is not null || phone is not null || emergencyContact is not null))
        {
            throw new StlApiException(
                "org_unit.validation",
                "Site metadata can only be set on site org units.",
                400);
        }

        if (unitType != "team" && teamType is not null)
        {
            throw new StlApiException(
                "org_unit.validation",
                "Team type can only be set on team org units.",
                400);
        }

        if (unitType != "position" && positionCode is not null)
        {
            throw new StlApiException(
                "org_unit.validation",
                "Position code can only be set on position org units.",
                400);
        }

        if (defaultSiteOrgUnitId.HasValue && unitType is not ("department" or "team" or "position"))
        {
            throw new StlApiException(
                "org_unit.validation",
                "Default site is only supported for department, team, or position org units.",
                400);
        }
    }

    private static void ValidateEffectiveDates(DateTimeOffset? effectiveStartDate, DateTimeOffset? effectiveEndDate)
    {
        if (effectiveStartDate.HasValue
            && effectiveEndDate.HasValue
            && effectiveEndDate.Value < effectiveStartDate.Value)
        {
            throw new StlApiException(
                "org_unit.validation",
                "Effective end date must be on or after the effective start date.",
                400);
        }
    }

    private async Task EnsureManagerIsValidAsync(
        Guid tenantId,
        Guid? managerPersonId,
        CancellationToken cancellationToken)
    {
        if (managerPersonId is not Guid requestedManagerId)
        {
            return;
        }

        var manager = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == requestedManagerId)
            .Select(x => new { x.Id, x.EmploymentStatus })
            .FirstOrDefaultAsync(cancellationToken);

        if (manager is null)
        {
            throw new StlApiException("org_unit.manager_not_found", "Manager person was not found.", 404);
        }

        if (!string.Equals(manager.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("org_unit.manager_invalid", "Manager person must be active.", 409);
        }
    }

    private async Task EnsureDefaultSiteIsValidAsync(
        Guid tenantId,
        string unitType,
        Guid? defaultSiteOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (defaultSiteOrgUnitId is not Guid requestedSiteId)
        {
            return;
        }

        var site = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == requestedSiteId)
            .Select(x => new { x.UnitType, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (site is null)
        {
            throw new StlApiException("org_unit.default_site_not_found", "Default site org unit was not found.", 404);
        }

        if (!string.Equals(site.UnitType, "site", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("org_unit.default_site_invalid", "Default site must reference a site org unit.", 409);
        }

        if (!OrgStructureCatalog.IsSelectableOrgUnitStatus(site.Status))
        {
            throw new StlApiException("org_unit.default_site_invalid", "Default site must be planned or active.", 409);
        }

        if (unitType == "department" || unitType == "team" || unitType == "position")
        {
            return;
        }
    }

    private async Task EnsureTypeChangeIsSafeAsync(
        Guid tenantId,
        Guid orgUnitId,
        CancellationToken cancellationToken)
    {
        var hasChildren = await db.OrgUnits.AnyAsync(
            x => x.TenantId == tenantId && x.ParentOrgUnitId == orgUnitId,
            cancellationToken);
        var isReferencedByAssignments = await db.OrgUnitAssignments.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (x.SiteOrgUnitId == orgUnitId
                    || x.DepartmentOrgUnitId == orgUnitId
                    || x.TeamOrgUnitId == orgUnitId
                    || x.PositionOrgUnitId == orgUnitId),
            cancellationToken);
        var isPrimarySnapshot = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.PrimaryOrgUnitId == orgUnitId,
            cancellationToken);

        if (hasChildren || isReferencedByAssignments || isPrimarySnapshot)
        {
            throw new StlApiException(
                "org_unit.type_conflict",
                "Cannot change the type of an org unit that is already in use.",
                409);
        }
    }

    private async Task EnsureStatusTransitionValidAsync(
        Guid tenantId,
        Guid orgUnitId,
        string normalizedStatus,
        CancellationToken cancellationToken)
    {
        if (normalizedStatus is "inactive" or "archived")
        {
            var hasSelectableChildren = await db.OrgUnits.AnyAsync(
                x =>
                    x.TenantId == tenantId
                    && x.ParentOrgUnitId == orgUnitId
                    && (x.Status == "planned" || x.Status == "active"),
                cancellationToken);
            if (hasSelectableChildren)
            {
                throw new StlApiException(
                    "org_unit.status_conflict",
                    "Cannot deactivate or archive an org unit that still has planned or active child org units.",
                    409);
            }

            var hasSelectableAssignments = await db.OrgUnitAssignments.AnyAsync(
                x =>
                    x.TenantId == tenantId
                    && (x.Status == "planned" || x.Status == "active")
                    && (x.SiteOrgUnitId == orgUnitId
                        || x.DepartmentOrgUnitId == orgUnitId
                        || x.TeamOrgUnitId == orgUnitId
                        || x.PositionOrgUnitId == orgUnitId),
                cancellationToken);
            if (hasSelectableAssignments)
            {
                throw new StlApiException(
                    "org_unit.status_conflict",
                    "Cannot deactivate or archive an org unit with planned or active placements.",
                    409);
            }
        }
    }

    private async Task EnsureNoDuplicateAsync(
        Guid tenantId,
        string unitType,
        string name,
        string code,
        Guid? parentOrgUnitId,
        Guid? excludedOrgUnitId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        var duplicateNameExists = await db.OrgUnits.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (excludedOrgUnitId == null || x.Id != excludedOrgUnitId.Value)
                && x.UnitType == unitType
                && x.Name.ToLower() == normalizedName,
            cancellationToken);

        if (duplicateNameExists)
        {
            throw new StlApiException(
                "org_unit.name_conflict",
                "An org unit with that type and name already exists in this tenant.",
                409);
        }

        var duplicateCodeExists = await db.OrgUnits.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (excludedOrgUnitId == null || x.Id != excludedOrgUnitId.Value)
                && x.Code == code
                && (parentOrgUnitId.HasValue
                    ? x.ParentOrgUnitId == parentOrgUnitId
                    : x.ParentOrgUnitId == null),
            cancellationToken);

        if (duplicateCodeExists)
        {
            throw new StlApiException(
                "org_unit.code_conflict",
                "An org unit with that code already exists under the same parent.",
                409);
        }
    }

    private static string NormalizeCodeValue(string code) =>
        OrgStructureCatalog.NormalizeCode(code);

    private static string NormalizeOrGenerateCode(
        string unitType,
        string? requestedCode,
        Guid orgUnitId,
        string? existingCode = null)
    {
        if (!string.IsNullOrWhiteSpace(requestedCode))
        {
            return NormalizeCodeValue(requestedCode);
        }

        if (!string.IsNullOrWhiteSpace(existingCode))
        {
            return existingCode;
        }

        var prefix = unitType == "site" ? "SITE" : "OU";
        return OrgStructureCatalog.BuildStableCode(prefix, orgUnitId);
    }

    private async Task EnsureParentIsValidAsync(
        Guid tenantId,
        Guid? currentOrgUnitId,
        string unitType,
        Guid? parentOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (parentOrgUnitId is not Guid parentId)
        {
            if (!OrgStructureCatalog.IsAllowedParentType(unitType, null))
            {
                throw new StlApiException(
                    "org_unit.hierarchy_invalid",
                    $"Org unit type {unitType} requires a valid parent org unit.",
                    409);
            }

            return;
        }

        if (currentOrgUnitId == parentId)
        {
            throw new StlApiException("org_unit.hierarchy_invalid", "Org unit cannot be its own parent.", 400);
        }

        var parent = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == parentId)
            .Select(x => new { x.Id, x.UnitType, x.ParentOrgUnitId, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (parent is null)
        {
            throw new StlApiException("org_unit.parent_not_found", "Parent org unit was not found.", 404);
        }

        if (!OrgStructureCatalog.IsSelectableOrgUnitStatus(parent.Status))
        {
            throw new StlApiException("org_unit.hierarchy_invalid", "Parent org unit must be planned or active.", 409);
        }

        if (!OrgStructureCatalog.IsAllowedParentType(unitType, parent.UnitType))
        {
            throw new StlApiException(
                "org_unit.hierarchy_invalid",
                $"Parent org unit type {parent.UnitType} is not valid for child type {unitType}.",
                409);
        }

        if (currentOrgUnitId is null)
        {
            return;
        }

        var cursor = parent.ParentOrgUnitId;
        while (cursor is Guid candidateId)
        {
            if (candidateId == currentOrgUnitId.Value)
            {
                throw new StlApiException("org_unit.hierarchy_invalid", "Org unit hierarchy cannot contain cycles.", 409);
            }

            cursor = await db.OrgUnits
                .Where(x => x.TenantId == tenantId && x.Id == candidateId)
                .Select(x => x.ParentOrgUnitId)
                .FirstOrDefaultAsync(cancellationToken);
        }
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

    private static string? NormalizeOptionalCatalogValue(
        string? value,
        IReadOnlyCollection<string> allowedValues,
        string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(
                "org_unit.validation",
                $"{fieldName} must be a supported value.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequiredText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("org_unit.validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("org_unit.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
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
            throw new StlApiException("org_unit.validation", $"{fieldName} must be {maxLength} characters or less.", 400);
        }

        return normalized;
    }

    private sealed record NormalizedOrgUnitRequest(
        string UnitType,
        string Name,
        string? Code,
        string? Description,
        Guid? ParentOrgUnitId,
        Guid? ManagerPersonId,
        DateTimeOffset? EffectiveStartDate,
        DateTimeOffset? EffectiveEndDate,
        string? SiteType,
        string? Timezone,
        string? Phone,
        string? EmergencyContact,
        string? TeamType,
        string? PositionCode,
        Guid? DefaultSiteOrgUnitId,
        bool ComplianceSensitive,
        bool SafetySensitive,
        bool CanSupervise,
        bool CanApprove,
        string? Status);
}
