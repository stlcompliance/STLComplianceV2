using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonLookupService(StaffArrDbContext db)
{
    public async Task<PersonLookupResponse> GetByPersonIdAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => new
            {
                x.Id,
                x.ExternalUserId,
                x.GivenName,
                x.FamilyName,
                x.DisplayName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.JobTitle,
                x.WorkPhone,
                x.PrimaryOrgUnitId,
                x.ManagerPersonId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        return await BuildResponseAsync(
            tenantId,
            person.Id,
            person.ExternalUserId,
            person.GivenName,
            person.FamilyName,
            person.DisplayName,
            person.PrimaryEmail,
            person.EmploymentStatus,
            person.JobTitle,
            person.WorkPhone,
            person.PrimaryOrgUnitId,
            person.ManagerPersonId,
            cancellationToken);
    }

    public async Task<PersonLookupResponse> GetByEmailAsync(
        Guid tenantId,
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new StlApiException(
                "person_lookup.validation",
                "Email query parameter is required.",
                400);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var person = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PrimaryEmail == normalizedEmail)
            .Select(x => new
            {
                x.Id,
                x.ExternalUserId,
                x.GivenName,
                x.FamilyName,
                x.DisplayName,
                x.PrimaryEmail,
                x.EmploymentStatus,
                x.JobTitle,
                x.WorkPhone,
                x.PrimaryOrgUnitId,
                x.ManagerPersonId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        return await BuildResponseAsync(
            tenantId,
            person.Id,
            person.ExternalUserId,
            person.GivenName,
            person.FamilyName,
            person.DisplayName,
            person.PrimaryEmail,
            person.EmploymentStatus,
            person.JobTitle,
            person.WorkPhone,
            person.PrimaryOrgUnitId,
            person.ManagerPersonId,
            cancellationToken);
    }

    private async Task<PersonLookupResponse> BuildResponseAsync(
        Guid tenantId,
        Guid personId,
        Guid? externalUserId,
        string givenName,
        string familyName,
        string displayName,
        string primaryEmail,
        string employmentStatus,
        string? jobTitle,
        string? workPhone,
        Guid? primaryOrgUnitId,
        Guid? managerPersonId,
        CancellationToken cancellationToken)
    {
        string? primaryOrgUnitName = null;
        string? primaryOrgUnitType = null;
        if (primaryOrgUnitId is Guid orgUnitId)
        {
            var primaryOrgUnit = await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
                .Select(x => new { x.Name, x.UnitType })
                .FirstOrDefaultAsync(cancellationToken);
            primaryOrgUnitName = primaryOrgUnit?.Name;
            primaryOrgUnitType = primaryOrgUnit?.UnitType;
        }

        string? managerDisplayName = null;
        if (managerPersonId is Guid managerId)
        {
            managerDisplayName = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == managerId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var activeAssignments = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var orgUnitIds = activeAssignments
            .SelectMany(x => new[] { x.SiteOrgUnitId, x.DepartmentOrgUnitId, x.TeamOrgUnitId, x.PositionOrgUnitId })
            .Distinct()
            .ToArray();
        if (primaryOrgUnitId is Guid primaryId)
        {
            orgUnitIds = orgUnitIds.Append(primaryId).Distinct().ToArray();
        }

        var orgUnitsById = orgUnitIds.Length == 0
            ? new Dictionary<Guid, (string Name, string UnitType)>()
            : await db.OrgUnits
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && orgUnitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => (x.Name, x.UnitType), cancellationToken);

        var assignmentResponses = activeAssignments
            .Select(assignment => MapAssignment(assignment, orgUnitsById))
            .ToList();

        return new PersonLookupResponse(
            personId,
            externalUserId,
            givenName,
            familyName,
            displayName,
            primaryEmail,
            employmentStatus,
            jobTitle,
            workPhone,
            new PersonLookupPlacementResponse(
                primaryOrgUnitId,
                primaryOrgUnitName,
                primaryOrgUnitType,
                managerPersonId,
                managerDisplayName,
                assignmentResponses),
            DateTimeOffset.UtcNow);
    }

    private static PersonLookupOrgAssignmentResponse MapAssignment(
        Entities.OrgUnitAssignment assignment,
        IReadOnlyDictionary<Guid, (string Name, string UnitType)> orgUnitsById)
    {
        var siteName = ResolveOrgUnitName(orgUnitsById, assignment.SiteOrgUnitId);
        var departmentName = ResolveOrgUnitName(orgUnitsById, assignment.DepartmentOrgUnitId);
        var teamName = ResolveOrgUnitName(orgUnitsById, assignment.TeamOrgUnitId);
        var positionName = ResolveOrgUnitName(orgUnitsById, assignment.PositionOrgUnitId);

        return new PersonLookupOrgAssignmentResponse(
            assignment.Id,
            assignment.SiteOrgUnitId,
            siteName,
            assignment.DepartmentOrgUnitId,
            departmentName,
            assignment.TeamOrgUnitId,
            teamName,
            assignment.PositionOrgUnitId,
            positionName,
            string.Join(" / ", siteName, departmentName, teamName, positionName));
    }

    private static string ResolveOrgUnitName(
        IReadOnlyDictionary<Guid, (string Name, string UnitType)> orgUnitsById,
        Guid orgUnitId) =>
        orgUnitsById.TryGetValue(orgUnitId, out var orgUnit) ? orgUnit.Name : orgUnitId.ToString();
}
