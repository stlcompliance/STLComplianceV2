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
        return await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.UnitType)
            .ThenBy(x => x.Name)
            .Select(x => new OrgUnitResponse(
                x.Id,
                x.UnitType,
                x.Name,
                x.ParentOrgUnitId,
                x.Status,
                x.Description,
                x.ManagerPersonId,
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.SiteType,
                x.Timezone,
                x.Phone,
                x.EmergencyContact,
                x.TeamType,
                x.PositionCode,
                x.DefaultSiteOrgUnitId,
                x.ComplianceSensitive,
                x.SafetySensitive,
                x.CanSupervise,
                x.CanApprove))
            .ToListAsync(cancellationToken);
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

        await EnsureNoDuplicateAsync(tenantId, normalized.UnitType, normalized.Name, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitType = normalized.UnitType,
            Name = normalized.Name,
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

        return ToResponse(orgUnit);
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

        await EnsureNoDuplicateAsync(tenantId, normalized.UnitType, normalized.Name, orgUnitId, cancellationToken);

        if (normalized.Status is not null && normalized.Status != orgUnit.Status)
        {
            await EnsureStatusTransitionValidAsync(tenantId, orgUnitId, normalized.Status, cancellationToken);
        }

        orgUnit.UnitType = normalized.UnitType;
        orgUnit.Name = normalized.Name;
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

        return ToResponse(orgUnit);
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

        return ToResponse(orgUnit);
    }

    private static OrgUnitResponse ToResponse(OrgUnit orgUnit) =>
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
            orgUnit.CanApprove);

    private async Task<NormalizedOrgUnitRequest> NormalizeRequestAsync(
        Guid tenantId,
        Guid? currentOrgUnitId,
        string unitType,
        string name,
        Guid? parentOrgUnitId,
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
        Guid? excludedOrgUnitId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        var duplicateExists = await db.OrgUnits.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (excludedOrgUnitId == null || x.Id != excludedOrgUnitId.Value)
                && x.UnitType == unitType
                && x.Name.ToLower() == normalizedName,
            cancellationToken);

        if (duplicateExists)
        {
            throw new StlApiException("org_unit.name_conflict", "An org unit with that type and name already exists in this tenant.", 409);
        }
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
