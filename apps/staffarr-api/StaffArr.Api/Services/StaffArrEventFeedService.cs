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
        "org_unit.status_update",
        "org_assignment.create",
        "org_assignment.update",
        "org_assignment.status_update",
        "person.site_changed",
        "person.department_changed",
        "person.team_changed",
        "person.position_changed",
        "person_role_assignment.create",
        "person_role_assignment.status_update",
        "readiness_override.grant",
        "readiness_override.clear",
        "incident.intake",
        "incident.product_intake",
        "incident.self_report.submitted",
        "incident.status_update",
        "incident.note.create",
        "incident.corrective_action.create",
        "incident.corrective_action.status_update",
        "incident.attachment.upload"
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

        var incidentsById = await db.PersonnelIncidents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && targetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var items = auditEvents
            .Select(x => MapEvent(x, peopleById, orgUnitsById, roleAssignmentsById, overridesById, incidentsById))
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
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById,
        IReadOnlyDictionary<Guid, PersonnelIncident> incidentsById)
    {
        var eventKind = ResolveEventKind(
            auditEvent,
            peopleById,
            orgUnitsById,
            roleAssignmentsById,
            overridesById,
            incidentsById);

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
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById,
        IReadOnlyDictionary<Guid, PersonnelIncident> incidentsById)
    {
        return auditEvent.Action switch
        {
            "person.create" => "staffarr.person.created",
            "person.update" => "staffarr.person.updated",
            "person.employment_status_update" => ResolvePersonStatusEventKind(auditEvent, peopleById),
            "people.manager_update" => "staffarr.person.manager_changed",
            "org_assignment.create" => "staffarr.person.assignment_created",
            "org_assignment.update" => "staffarr.person.assignment_updated",
            "org_assignment.status_update" => "staffarr.person.assignment_status_updated",
            "person.site_changed" => "staffarr.person.site_changed",
            "person.department_changed" => "staffarr.person.department_changed",
            "person.team_changed" => "staffarr.person.team_changed",
            "person.position_changed" => "staffarr.person.position_changed",
            "org_unit.create" => ResolveOrgUnitEventKind(auditEvent, orgUnitsById, "created"),
            "org_unit.update" or "org_unit.status_update" => ResolveOrgUnitEventKind(auditEvent, orgUnitsById, "updated"),
            "person_role_assignment.create" => "staffarr.permission.assigned",
            "person_role_assignment.status_update" => ResolvePermissionAssignmentEventKind(auditEvent, roleAssignmentsById),
            "readiness_override.grant" => "staffarr.override.created",
            "readiness_override.clear" => ResolveOverrideEventKind(auditEvent, overridesById),
            "incident.status_update" => ResolveIncidentStatusEventKind(auditEvent, incidentsById),
            "incident.intake" or "incident.product_intake" or "incident.self_report.submitted" => "staffarr.incident.created",
            "incident.note.create" => "staffarr.incident.note.created",
            "incident.corrective_action.create" => "staffarr.incident.corrective_action.created",
            "incident.corrective_action.status_update" => ResolveCorrectiveActionEventKind(auditEvent),
            "incident.attachment.upload" => "staffarr.incident.attachment.uploaded",
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
            return "staffarr.person.activated";
        }

        return "staffarr.person.deactivated";
    }

    private static string ResolveOrgUnitEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById,
        string suffix)
    {
        if (TryGetTargetId(auditEvent, out var orgUnitId)
            && orgUnitsById.TryGetValue(orgUnitId, out var orgUnit))
        {
            return $"staffarr.{orgUnit.UnitType}.{suffix}";
        }

        return $"staffarr.org_unit.{suffix}";
    }

    private static string ResolvePermissionAssignmentEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, PersonRoleAssignment> roleAssignmentsById)
    {
        if (TryGetTargetId(auditEvent, out var assignmentId)
            && roleAssignmentsById.TryGetValue(assignmentId, out var assignment)
            && string.Equals(assignment.Status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            return "staffarr.permission.revoked";
        }

        return "staffarr.permission.assigned";
    }

    private static string ResolveOverrideEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, PersonReadinessOverride> overridesById)
    {
        if (TryGetTargetId(auditEvent, out var overrideId)
            && overridesById.TryGetValue(overrideId, out var readinessOverride)
            && string.Equals(readinessOverride.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return "staffarr.override.created";
        }

        return "staffarr.override.revoked";
    }

    private static string ResolveIncidentStatusEventKind(
        StaffArrAuditEvent auditEvent,
        IReadOnlyDictionary<Guid, PersonnelIncident> incidentsById)
    {
        if (!TryGetTargetId(auditEvent, out var incidentId)
            || !incidentsById.TryGetValue(incidentId, out var incident))
        {
            return "staffarr.incident.status_updated";
        }

        var status = string.IsNullOrWhiteSpace(auditEvent.ReasonCode)
            ? incident.Status
            : auditEvent.ReasonCode!;

        if (string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return "staffarr.incident.closed";
        }

        if (string.Equals(status, "open", StringComparison.OrdinalIgnoreCase))
        {
            return "staffarr.incident.reopened";
        }

        return "staffarr.incident.status_updated";
    }

    private static string ResolveCorrectiveActionEventKind(StaffArrAuditEvent auditEvent)
    {
        if (string.Equals(auditEvent.ReasonCode, "completed", StringComparison.OrdinalIgnoreCase))
        {
            return "staffarr.incident.corrective_action.completed";
        }

        return "staffarr.incident.corrective_action.reopened";
    }

    private static bool TryGetTargetId(StaffArrAuditEvent auditEvent, out Guid targetId) =>
        Guid.TryParse(auditEvent.TargetId, out targetId);
}
