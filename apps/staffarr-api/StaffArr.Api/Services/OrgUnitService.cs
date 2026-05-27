using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

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
                x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrgUnitResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateOrgUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateOrUpdateRequest(request.UnitType, request.Name);
        await EnsureParentIsValidAsync(tenantId, null, request.ParentOrgUnitId, cancellationToken);
        await EnsureNoDuplicateAsync(tenantId, request.UnitType, request.Name, null, cancellationToken);

        var orgUnit = new OrgUnit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitType = request.UnitType.Trim().ToLowerInvariant(),
            Name = request.Name.Trim(),
            ParentOrgUnitId = request.ParentOrgUnitId,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
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
        ValidateCreateOrUpdateRequest(request.UnitType, request.Name);

        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == orgUnitId,
            cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404);
        }

        await EnsureParentIsValidAsync(tenantId, orgUnitId, request.ParentOrgUnitId, cancellationToken);
        await EnsureNoDuplicateAsync(tenantId, request.UnitType, request.Name, orgUnitId, cancellationToken);

        orgUnit.UnitType = request.UnitType.Trim().ToLowerInvariant();
        orgUnit.Name = request.Name.Trim();
        orgUnit.ParentOrgUnitId = request.ParentOrgUnitId;
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
        var normalizedStatus = NormalizeStatus(request.Status);
        var orgUnit = await db.OrgUnits.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == orgUnitId,
            cancellationToken);
        if (orgUnit is null)
        {
            throw new StlApiException("org_unit.not_found", "Org unit was not found.", 404);
        }

        if (normalizedStatus == "inactive")
        {
            var hasActiveChildren = await db.OrgUnits.AnyAsync(
                x => x.TenantId == tenantId && x.ParentOrgUnitId == orgUnitId && x.Status == "active",
                cancellationToken);
            if (hasActiveChildren)
            {
                throw new StlApiException(
                    "org_unit.status_conflict",
                    "Cannot deactivate an org unit that still has active child org units.",
                    409);
            }
        }

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
            orgUnit.Status);

    private static void ValidateCreateOrUpdateRequest(string unitType, string name)
    {
        if (string.IsNullOrWhiteSpace(unitType) || unitType.Trim().Length > 32)
        {
            throw new StlApiException("org_unit.validation", "Unit type is required and must be 32 characters or less.", 400);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 128)
        {
            throw new StlApiException("org_unit.validation", "Name is required and must be 128 characters or less.", 400);
        }
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException("org_unit.validation", "Status is required.", 400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (normalized is not ("active" or "inactive"))
        {
            throw new StlApiException("org_unit.validation", "Status must be either active or inactive.", 400);
        }

        return normalized;
    }

    private async Task EnsureNoDuplicateAsync(
        Guid tenantId,
        string unitType,
        string name,
        Guid? excludedOrgUnitId,
        CancellationToken cancellationToken)
    {
        var normalizedType = unitType.Trim().ToLowerInvariant();
        var normalizedName = name.Trim().ToLowerInvariant();

        var duplicateExists = await db.OrgUnits.AnyAsync(
            x =>
                x.TenantId == tenantId
                && (excludedOrgUnitId == null || x.Id != excludedOrgUnitId.Value)
                && x.UnitType.ToLower() == normalizedType
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
        Guid? parentOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (parentOrgUnitId is not Guid parentId)
        {
            return;
        }

        if (currentOrgUnitId == parentId)
        {
            throw new StlApiException("org_unit.hierarchy_invalid", "Org unit cannot be its own parent.", 400);
        }

        var currentParentId = await db.OrgUnits
            .Where(x => x.TenantId == tenantId && x.Id == parentId)
            .Select(x => new { x.Id, x.ParentOrgUnitId, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentParentId is null)
        {
            throw new StlApiException("org_unit.parent_not_found", "Parent org unit was not found.", 404);
        }

        if (currentParentId.Status != "active")
        {
            throw new StlApiException("org_unit.hierarchy_invalid", "Parent org unit must be active.", 409);
        }

        if (currentOrgUnitId is null)
        {
            return;
        }

        var cursor = currentParentId.ParentOrgUnitId;
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
}
