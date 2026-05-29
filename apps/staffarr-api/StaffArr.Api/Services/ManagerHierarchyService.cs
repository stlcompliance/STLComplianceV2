using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ManagerHierarchyService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<PersonManagerResponse> UpdateManagerAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        UpdatePersonManagerRequest request,
        CancellationToken cancellationToken = default)
    {
        var person = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var manager = await ValidateManagerReferenceAsync(
            tenantId,
            personId,
            request.ManagerPersonId,
            cancellationToken);

        person.ManagerPersonId = manager?.Id;
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "people.manager_update",
            tenantId,
            actorUserId,
            "person",
            personId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new PersonManagerResponse(
            person.Id,
            person.ManagerPersonId,
            manager?.DisplayName,
            person.UpdatedAt);
    }

    public async Task<IReadOnlyList<ManagerChainEntryResponse>> GetManagerChainAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var chain = new List<ManagerChainEntryResponse>();
        var visited = new HashSet<Guid>();
        var cursor = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == personId)
            .Select(x => x.ManagerPersonId)
            .FirstOrDefaultAsync(cancellationToken);
        var level = 1;

        while (cursor is Guid managerId)
        {
            if (!visited.Add(managerId))
            {
                break;
            }

            var manager = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == managerId)
                .Select(x => new
                {
                    x.Id,
                    x.DisplayName,
                    x.PrimaryEmail,
                    x.JobTitle,
                    x.PrimaryOrgUnitId,
                    x.ManagerPersonId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (manager is null)
            {
                break;
            }

            var primaryOrgUnitName = manager.PrimaryOrgUnitId is Guid orgUnitId
                ? await db.OrgUnits
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId && x.Id == orgUnitId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            chain.Add(new ManagerChainEntryResponse(
                manager.Id,
                manager.DisplayName,
                manager.PrimaryEmail,
                manager.JobTitle,
                primaryOrgUnitName,
                manager.ManagerPersonId,
                level));

            cursor = manager.ManagerPersonId;
            level++;
        }

        return chain;
    }

    public async Task<IReadOnlyList<SubordinateSummaryResponse>> GetSubordinatesAsync(
        Guid tenantId,
        Guid managerPersonId,
        bool includeIndirect,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var managerExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == managerPersonId,
            cancellationToken);
        if (!managerExists)
        {
            throw new StlApiException("people.not_found", "Manager person was not found.", 404);
        }

        var normalizedLimit = Math.Clamp(limit <= 0 ? 200 : limit, 1, 500);
        var queue = new Queue<(Guid ManagerId, int Depth)>();
        queue.Enqueue((managerPersonId, 1));

        var orderedSubordinates = new List<(StaffPerson Person, int Depth)>();
        var seen = new HashSet<Guid>();

        while (queue.Count > 0 && orderedSubordinates.Count < normalizedLimit)
        {
            var (currentManagerId, depth) = queue.Dequeue();
            var directSubordinates = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ManagerPersonId == currentManagerId)
                .OrderBy(x => x.DisplayName)
                .ToListAsync(cancellationToken);

            foreach (var subordinate in directSubordinates)
            {
                if (!seen.Add(subordinate.Id))
                {
                    continue;
                }

                orderedSubordinates.Add((subordinate, depth));
                if (orderedSubordinates.Count >= normalizedLimit)
                {
                    break;
                }

                if (includeIndirect)
                {
                    queue.Enqueue((subordinate.Id, depth + 1));
                }
            }
        }

        return await BuildSubordinateSummariesAsync(
            tenantId,
            orderedSubordinates,
            cancellationToken);
    }

    public async Task<SubordinateSummaryResponse> GetSubordinateDetailAsync(
        Guid tenantId,
        Guid managerPersonId,
        Guid subordinatePersonId,
        CancellationToken cancellationToken = default)
    {
        var managerExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == managerPersonId,
            cancellationToken);
        if (!managerExists)
        {
            throw new StlApiException("people.not_found", "Manager person was not found.", 404);
        }

        var subordinate = await db.People
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == subordinatePersonId,
                cancellationToken);
        if (subordinate is null)
        {
            throw new StlApiException("people.not_found", "Subordinate person was not found.", 404);
        }

        var depth = await ComputeDepthFromManagerAsync(
            tenantId,
            managerPersonId,
            subordinatePersonId,
            cancellationToken);
        if (depth is null)
        {
            throw new StlApiException(
                "people.subordinate_not_found",
                "Requested person is not a subordinate of the specified manager.",
                404);
        }

        var summaries = await BuildSubordinateSummariesAsync(
            tenantId,
            [(subordinate, depth.Value)],
            cancellationToken);
        return summaries[0];
    }

    public Task<bool> IsDirectManagerOfAsync(
        Guid tenantId,
        Guid managerPersonId,
        Guid subordinatePersonId,
        CancellationToken cancellationToken = default) =>
        db.People.AsNoTracking().AnyAsync(
            x => x.TenantId == tenantId
                && x.Id == subordinatePersonId
                && x.ManagerPersonId == managerPersonId,
            cancellationToken);

    private async Task<StaffPerson?> ValidateManagerReferenceAsync(
        Guid tenantId,
        Guid personId,
        Guid? managerPersonId,
        CancellationToken cancellationToken)
    {
        if (managerPersonId is null)
        {
            return null;
        }

        if (managerPersonId.Value == personId)
        {
            throw new StlApiException("people.manager_invalid", "A person cannot be their own manager.", 400);
        }

        var manager = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == managerPersonId.Value,
            cancellationToken);
        if (manager is null)
        {
            throw new StlApiException("people.manager_not_found", "Manager person was not found.", 404);
        }

        if (!string.Equals(manager.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("people.manager_invalid", "Manager person must be active.", 409);
        }

        var cursor = manager.ManagerPersonId;
        var visited = new HashSet<Guid> { manager.Id };
        while (cursor is Guid managerCursorId)
        {
            if (!visited.Add(managerCursorId))
            {
                break;
            }

            if (managerCursorId == personId)
            {
                throw new StlApiException(
                    "people.manager_cycle",
                    "Manager assignment would create a cycle in the hierarchy.",
                    409);
            }

            cursor = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == managerCursorId)
                .Select(x => x.ManagerPersonId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return manager;
    }

    private async Task<int?> ComputeDepthFromManagerAsync(
        Guid tenantId,
        Guid managerPersonId,
        Guid subordinatePersonId,
        CancellationToken cancellationToken)
    {
        if (managerPersonId == subordinatePersonId)
        {
            return 0;
        }

        var currentId = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == subordinatePersonId)
            .Select(x => x.ManagerPersonId)
            .FirstOrDefaultAsync(cancellationToken);
        var depth = 1;
        var visited = new HashSet<Guid>();

        while (currentId is Guid cursor)
        {
            if (!visited.Add(cursor))
            {
                break;
            }

            if (cursor == managerPersonId)
            {
                return depth;
            }

            currentId = await db.People
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == cursor)
                .Select(x => x.ManagerPersonId)
                .FirstOrDefaultAsync(cancellationToken);
            depth++;
        }

        return null;
    }

    private async Task<IReadOnlyList<SubordinateSummaryResponse>> BuildSubordinateSummariesAsync(
        Guid tenantId,
        IReadOnlyList<(StaffPerson Person, int Depth)> peopleWithDepth,
        CancellationToken cancellationToken)
    {
        if (peopleWithDepth.Count == 0)
        {
            return [];
        }

        var personIds = peopleWithDepth.Select(x => x.Person.Id).ToArray();

        var orgUnitsById = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var directReportCounts = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ManagerPersonId != null && personIds.Contains(x.ManagerPersonId.Value))
            .GroupBy(x => x.ManagerPersonId!.Value)
            .Select(g => new { ManagerPersonId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ManagerPersonId, x => x.Count, cancellationToken);

        var managerIds = peopleWithDepth
            .Where(x => x.Person.ManagerPersonId != null)
            .Select(x => x.Person.ManagerPersonId!.Value)
            .Distinct()
            .ToArray();
        var managerNameById = await db.People
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && managerIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

        var activeAssignments = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == "active" && personIds.Contains(x.PersonId))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);
        var assignmentByPersonId = activeAssignments
            .GroupBy(x => x.PersonId)
            .ToDictionary(x => x.Key, x => x.First());

        var responses = new List<SubordinateSummaryResponse>(peopleWithDepth.Count);
        foreach (var (person, depth) in peopleWithDepth)
        {
            assignmentByPersonId.TryGetValue(person.Id, out var assignment);
            var assignmentPath = assignment is null
                ? null
                : string.Join(
                    " / ",
                    ResolveOrgUnitName(orgUnitsById, assignment.SiteOrgUnitId),
                    ResolveOrgUnitName(orgUnitsById, assignment.DepartmentOrgUnitId),
                    ResolveOrgUnitName(orgUnitsById, assignment.TeamOrgUnitId),
                    ResolveOrgUnitName(orgUnitsById, assignment.PositionOrgUnitId));

            var primaryOrgUnitName = person.PrimaryOrgUnitId is Guid primaryOrgUnitId && orgUnitsById.TryGetValue(primaryOrgUnitId, out var unitName)
                ? unitName
                : null;

            var managerDisplayName = person.ManagerPersonId is Guid managerId && managerNameById.TryGetValue(managerId, out var managerName)
                ? managerName
                : null;

            responses.Add(new SubordinateSummaryResponse(
                person.Id,
                person.DisplayName,
                person.PrimaryEmail,
                person.EmploymentStatus,
                person.JobTitle,
                primaryOrgUnitName,
                person.ManagerPersonId,
                managerDisplayName,
                depth,
                directReportCounts.GetValueOrDefault(person.Id),
                assignmentPath));
        }

        return responses;
    }

    private static string ResolveOrgUnitName(IReadOnlyDictionary<Guid, string> orgUnitsById, Guid orgUnitId) =>
        orgUnitsById.TryGetValue(orgUnitId, out var name) ? name : orgUnitId.ToString();
}
