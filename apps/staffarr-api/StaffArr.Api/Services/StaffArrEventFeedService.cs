using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class StaffArrEventFeedService(StaffArrDbContext db)
{
    public const string IntegrationReadActionScope = "staffarr.events.read";

    private static readonly string[] EventSourceActions =
    [
        "person.create",
        "person.update",
        "person.employment_status_update",
        "people.manager_update",
        "org_unit.create",
        "org_unit.update",
        "org_assignment.create",
        "org_assignment.update",
        "org_assignment.status_update",
        "person_role_assignment.create",
        "person_role_assignment.status_update",
        "readiness_override.grant",
        "readiness_override.clear",
        "incident.intake",
        "incident.product_intake",
        "incident.self_report.submitted"
    ];

    public async Task<StaffArrEventFeedResponse> ListAsync(
        Guid tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch
        {
            < 1 => 50,
            > 200 => 200,
            _ => pageSize
        };

        var query = db.AuditEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && EventSourceActions.Contains(x.Action));

        if (from is DateTimeOffset fromUtc)
        {
            query = query.Where(x => x.OccurredAt >= fromUtc);
        }

        if (to is DateTimeOffset toUtc)
        {
            query = query.Where(x => x.OccurredAt <= toUtc);
        }

        var total = await query.CountAsync(cancellationToken);
        var auditEvents = await query
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var targetIds = auditEvents
            .Where(x => Guid.TryParse(x.TargetId, out _))
            .Select(x => Guid.Parse(x.TargetId!))
            .Distinct()
            .ToArray();

        var peopleById = await db.People.AsNoTracking()
            .Where(x => x.TenantId == tenantId && targetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var orgUnitsById = await db.OrgUnits.AsNoTracking()
            .Where(x => x.TenantId == tenantId && targetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var roleAssignmentsById = await db.PersonRoleAssignments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && targetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var overridesById = await db.PersonReadinessOverrides.AsNoTracking()
            .Where(x => x.TenantId == tenantId && targetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var items = auditEvents
            .Select(x => MapEvent(x, peopleById, orgUnitsById, roleAssignmentsById, overridesById))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return new StaffArrEventFeedResponse(
            DateTimeOffset.UtcNow,
            page,
            pageSize,
            total,
            page * pageSize < total,
            items);
    }

    private static StaffArrEventFeedItem? MapEvent(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, StaffPerson> peopleById,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById,
        IReadOnlyDictionary<Guid, PersonRoleAssignment> roleAssignmentsById,
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById)
    {
        var eventKind = ResolveEventKind(
            auditEvent,
            peopleById,
            orgUnitsById,
            roleAssignmentsById,
            overridesById);

        if (eventKind is null)
        {
            return null;
        }

        return new StaffArrEventFeedItem(
            auditEvent.Id,
            auditEvent.TenantId,
            eventKind,
            auditEvent.Action,
            auditEvent.TargetType,
            auditEvent.TargetId,
            auditEvent.ActorUserId,
            auditEvent.Result,
            auditEvent.ReasonCode,
            auditEvent.CorrelationId,
            auditEvent.OccurredAt);
    }

    private static string? ResolveEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, StaffPerson> peopleById,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById,
        IReadOnlyDictionary<Guid, PersonRoleAssignment> roleAssignmentsById,
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById)
    {
        return auditEvent.Action switch
        {
            "person.create" => "person.created",
            "person.update" => "person.updated",
            "person.employment_status_update" => ResolvePersonStatusEventKind(auditEvent, peopleById),
            "people.manager_update" => "person.manager.changed",
            "org_assignment.create" or "org_assignment.update" or "org_assignment.status_update" => "person.assignment.changed",
            "org_unit.create" => ResolveOrgUnitEventKind(auditEvent, orgUnitsById, "created"),
            "org_unit.update" => ResolveOrgUnitEventKind(auditEvent, orgUnitsById, "updated"),
            "person_role_assignment.create" => "permission.assigned",
            "person_role_assignment.status_update" => ResolvePermissionAssignmentEventKind(auditEvent, roleAssignmentsById),
            "readiness_override.grant" => "override.created",
            "readiness_override.clear" => ResolveOverrideEventKind(auditEvent, overridesById),
            "incident.intake" or "incident.product_intake" or "incident.self_report.submitted" => "incident.created",
            _ => null
        };
    }

    private static string ResolvePersonStatusEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, StaffPerson> peopleById)
    {
        if (TryGetTargetId(auditEvent, out var personId)
            && peopleById.TryGetValue(personId, out var person)
            && string.Equals(person.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            return "person.activated";
        }

        return "person.deactivated";
    }

    private static string ResolveOrgUnitEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById,
        string suffix)
    {
        if (TryGetTargetId(auditEvent, out var orgUnitId)
            && orgUnitsById.TryGetValue(orgUnitId, out var orgUnit)
            && orgUnit.UnitType is "site" or "department" or "position" or "team")
        {
            return $"{orgUnit.UnitType}.{suffix}";
        }

        return $"org_unit.{suffix}";
    }

    private static string ResolvePermissionAssignmentEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, PersonRoleAssignment> roleAssignmentsById)
    {
        if (TryGetTargetId(auditEvent, out var assignmentId)
            && roleAssignmentsById.TryGetValue(assignmentId, out var assignment)
            && string.Equals(assignment.Status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            return "permission.revoked";
        }

        return "permission.assigned";
    }

    private static string ResolveOverrideEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById)
    {
        if (TryGetTargetId(auditEvent, out var overrideId)
            && overridesById.TryGetValue(overrideId, out var readinessOverride)
            && string.Equals(readinessOverride.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return "override.created";
        }

        return "override.revoked";
    }

    private static bool TryGetTargetId(StaffArrAuditEvent auditEvent, out Guid targetId) =>
        Guid.TryParse(auditEvent.TargetId, out targetId);
}
