using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class OrgUnitAssignmentService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<OrgUnitAssignmentResponse>> ListByPersonAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        return await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToResponseExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        CreateOrgUnitAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateAssignmentAsync(
            tenantId,
            personId,
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId,
            null,
            cancellationToken);

        var assignment = new OrgUnitAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            SiteOrgUnitId = request.SiteOrgUnitId,
            DepartmentOrgUnitId = request.DepartmentOrgUnitId,
            TeamOrgUnitId = request.TeamOrgUnitId,
            PositionOrgUnitId = request.PositionOrgUnitId,
            Status = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.OrgUnitAssignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "org_assignment.create",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid assignmentId,
        UpdateOrgUnitAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentEntityAsync(tenantId, personId, assignmentId, cancellationToken);
        await ValidateAssignmentAsync(
            tenantId,
            personId,
            request.SiteOrgUnitId,
            request.DepartmentOrgUnitId,
            request.TeamOrgUnitId,
            request.PositionOrgUnitId,
            assignmentId,
            cancellationToken);

        assignment.SiteOrgUnitId = request.SiteOrgUnitId;
        assignment.DepartmentOrgUnitId = request.DepartmentOrgUnitId;
        assignment.TeamOrgUnitId = request.TeamOrgUnitId;
        assignment.PositionOrgUnitId = request.PositionOrgUnitId;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "org_assignment.update",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    public async Task<OrgUnitAssignmentResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid assignmentId,
        UpdateOrgUnitAssignmentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignmentEntityAsync(tenantId, personId, assignmentId, cancellationToken);
        var normalizedStatus = NormalizeStatus(request.Status);
        if (normalizedStatus == "active")
        {
            await EnsureOrgUnitAsync(tenantId, assignment.SiteOrgUnitId, "site", true, cancellationToken);
            await EnsureOrgUnitAsync(tenantId, assignment.DepartmentOrgUnitId, "department", true, cancellationToken);
            await EnsureOrgUnitAsync(tenantId, assignment.TeamOrgUnitId, "team", true, cancellationToken);
            await EnsureOrgUnitAsync(tenantId, assignment.PositionOrgUnitId, "position", true, cancellationToken);
            await EnsureHierarchyLinkageAsync(
                tenantId,
                assignment.SiteOrgUnitId,
                assignment.DepartmentOrgUnitId,
                assignment.TeamOrgUnitId,
                assignment.PositionOrgUnitId,
                cancellationToken);
        }

        assignment.Status = normalizedStatus;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "org_assignment.status_update",
            tenantId,
            actorUserId,
            "org_assignment",
            assignment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetByIdAsync(tenantId, personId, assignment.Id, cancellationToken);
    }

    private async Task<OrgUnitAssignmentResponse> GetByIdAsync(
        Guid tenantId,
        Guid personId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Id == assignmentId)
            .Select(ToResponseExpression())
            .FirstOrDefaultAsync(cancellationToken);
        return assignment ?? throw new StlApiException("org_assignment.not_found", "Org assignment was not found.", 404);
    }

    private async Task<OrgUnitAssignment> GetAssignmentEntityAsync(
        Guid tenantId,
        Guid personId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        var assignment = await db.OrgUnitAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && x.Id == assignmentId,
            cancellationToken);
        return assignment ?? throw new StlApiException("org_assignment.not_found", "Org assignment was not found.", 404);
    }

    private async Task ValidateAssignmentAsync(
        Guid tenantId,
        Guid personId,
        Guid siteOrgUnitId,
        Guid departmentOrgUnitId,
        Guid teamOrgUnitId,
        Guid positionOrgUnitId,
        Guid? excludedAssignmentId,
        CancellationToken cancellationToken)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, siteOrgUnitId, "site", true, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, departmentOrgUnitId, "department", true, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, teamOrgUnitId, "team", true, cancellationToken);
        await EnsureOrgUnitAsync(tenantId, positionOrgUnitId, "position", true, cancellationToken);
        await EnsureHierarchyLinkageAsync(
            tenantId,
            siteOrgUnitId,
            departmentOrgUnitId,
            teamOrgUnitId,
            positionOrgUnitId,
            cancellationToken);

        var duplicateExists = await db.OrgUnitAssignments.AnyAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (excludedAssignmentId == null || x.Id != excludedAssignmentId.Value)
                && x.SiteOrgUnitId == siteOrgUnitId
                && x.DepartmentOrgUnitId == departmentOrgUnitId
                && x.TeamOrgUnitId == teamOrgUnitId
                && x.PositionOrgUnitId == positionOrgUnitId,
            cancellationToken);

        if (duplicateExists)
        {
            throw new StlApiException("org_assignment.duplicate", "An identical org assignment already exists for this person.", 409);
        }
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private async Task EnsureOrgUnitAsync(
        Guid tenantId,
        Guid orgUnitId,
        string expectedType,
        bool mustBeActive,
        CancellationToken cancellationToken)
    {
        var orgUnit = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
            .Select(x => new { x.UnitType, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (orgUnit is null)
        {
            throw new StlApiException("org_assignment.org_unit_not_found", $"Referenced {expectedType} org unit was not found.", 404);
        }

        if (!string.Equals(orgUnit.UnitType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                $"Referenced org unit must be of type {expectedType}.",
                409);
        }

        if (mustBeActive && !string.Equals(orgUnit.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "org_assignment.link_inactive",
                $"Referenced {expectedType} org unit must be active.",
                409);
        }
    }

    private async Task EnsureHierarchyLinkageAsync(
        Guid tenantId,
        Guid siteOrgUnitId,
        Guid departmentOrgUnitId,
        Guid teamOrgUnitId,
        Guid positionOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (!await IsDescendantOrSelfAsync(tenantId, departmentOrgUnitId, siteOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Department must be linked under the selected site in the org hierarchy.",
                409);
        }

        if (!await IsDescendantOrSelfAsync(tenantId, teamOrgUnitId, departmentOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Team must be linked under the selected department in the org hierarchy.",
                409);
        }

        if (!await IsDescendantOrSelfAsync(tenantId, positionOrgUnitId, teamOrgUnitId, cancellationToken))
        {
            throw new StlApiException(
                "org_assignment.link_invalid",
                "Position must be linked under the selected team in the org hierarchy.",
                409);
        }
    }

    private async Task<bool> IsDescendantOrSelfAsync(
        Guid tenantId,
        Guid nodeId,
        Guid ancestorId,
        CancellationToken cancellationToken)
    {
        if (nodeId == ancestorId)
        {
            return true;
        }

        var cursor = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == nodeId)
            .Select(x => x.ParentOrgUnitId)
            .FirstOrDefaultAsync(cancellationToken);

        while (cursor is Guid parentId)
        {
            if (parentId == ancestorId)
            {
                return true;
            }

            cursor = await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == parentId)
                .Select(x => x.ParentOrgUnitId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException("org_assignment.validation", "Status is required.", 400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (normalized is not ("active" or "inactive"))
        {
            throw new StlApiException("org_assignment.validation", "Status must be either active or inactive.", 400);
        }

        return normalized;
    }

    private static System.Linq.Expressions.Expression<Func<OrgUnitAssignment, OrgUnitAssignmentResponse>> ToResponseExpression() =>
        x => new OrgUnitAssignmentResponse(
            x.Id,
            x.PersonId,
            x.SiteOrgUnitId,
            x.DepartmentOrgUnitId,
            x.TeamOrgUnitId,
            x.PositionOrgUnitId,
            x.Status,
            x.CreatedAt,
            x.UpdatedAt);
}
