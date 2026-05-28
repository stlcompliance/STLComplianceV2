using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;

namespace StaffArr.Api.Services;

public static class PersonTimelineBuilder
{
    public static async Task<List<PersonTimelineEntryResponse>> BuildTimelineEntriesAsync(
        StaffArrDbContext db,
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var entries = new List<PersonTimelineEntryResponse>();

        var incidents = await db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var incident in incidents)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"incident:{incident.Id}:reported",
                personId,
                "incident",
                "incident_reported",
                $"Incident reported: {incident.Title}",
                $"{incident.ReasonCategoryKey} · {incident.Severity} · {incident.Status}",
                incident.ReportedAt,
                incident.ReportedByUserId,
                "personnel_incident",
                incident.Id.ToString(),
                null));
        }

        var routings = await (
            from routing in db.IncidentTrainarrRoutings.AsNoTracking()
            join incident in db.PersonnelIncidents.AsNoTracking()
                on routing.IncidentId equals incident.Id
            where routing.TenantId == tenantId
                && incident.TenantId == tenantId
                && incident.PersonId == personId
            select new
            {
                routing.IncidentId,
                routing.TrainarrRemediationId,
                routing.RoutedAt,
                routing.RoutedByUserId,
                routing.RoutingStatus,
                incident.Title
            }).ToListAsync(cancellationToken);

        foreach (var routing in routings)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"incident_routing:{routing.IncidentId}:routed",
                personId,
                "incident_routing",
                "incident_routed_trainarr",
                $"Incident routed to TrainArr: {routing.Title}",
                routing.RoutingStatus,
                routing.RoutedAt,
                routing.RoutedByUserId,
                "personnel_incident",
                routing.IncidentId.ToString(),
                routing.TrainarrRemediationId.ToString()));
        }

        var overrides = await db.PersonReadinessOverrides
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var overrideEntry in overrides)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"readiness_override:{overrideEntry.Id}:granted",
                personId,
                "readiness",
                "readiness_override_granted",
                "Readiness override granted",
                overrideEntry.Reason,
                overrideEntry.GrantedAt,
                overrideEntry.GrantedByUserId,
                "person_readiness_override",
                overrideEntry.Id.ToString(),
                null));

            if (overrideEntry.ClearedAt is DateTimeOffset clearedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"readiness_override:{overrideEntry.Id}:cleared",
                    personId,
                    "readiness",
                    "readiness_override_cleared",
                    "Readiness override cleared",
                    overrideEntry.Reason,
                    clearedAt,
                    overrideEntry.ClearedByUserId,
                    "person_readiness_override",
                    overrideEntry.Id.ToString(),
                    null));
            }
        }

        var certifications = await (
            from cert in db.PersonCertifications.AsNoTracking()
            join definition in db.CertificationDefinitions.AsNoTracking()
                on cert.CertificationDefinitionId equals definition.Id
            where cert.TenantId == tenantId
                && cert.PersonId == personId
                && definition.TenantId == tenantId
            select new
            {
                cert.Id,
                cert.GrantedAt,
                cert.GrantedByUserId,
                cert.Status,
                cert.SourceType,
                cert.ExternalPublicationId,
                definition.Name,
                definition.CertificationKey
            }).ToListAsync(cancellationToken);

        foreach (var certification in certifications)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"certification:{certification.Id}:granted",
                personId,
                "certification",
                "certification_granted",
                $"Certification granted: {certification.Name}",
                $"{certification.CertificationKey} · {certification.Status} · {certification.SourceType}",
                certification.GrantedAt,
                certification.GrantedByUserId,
                "person_certification",
                certification.Id.ToString(),
                certification.ExternalPublicationId?.ToString()));
        }

        var permissionEvents = await db.PermissionHistoryEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var permissionEvent in permissionEvents)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"permission:{permissionEvent.Id}",
                personId,
                "permission",
                permissionEvent.EventType,
                $"Permission change: {permissionEvent.PermissionName}",
                $"{permissionEvent.RoleName} ({permissionEvent.RoleKey}) · {permissionEvent.AssignmentStatus} · {permissionEvent.ScopeType}",
                permissionEvent.OccurredAt,
                permissionEvent.ActorUserId,
                "permission_history_event",
                permissionEvent.Id.ToString(),
                permissionEvent.AssignmentId.ToString()));
        }

        var trainingBlockers = await db.PersonTrainingBlockers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var blocker in trainingBlockers)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"training_blocker:{blocker.Id}:published",
                personId,
                "training_blocker",
                "training_blocker_published",
                $"Training blocker published: {blocker.QualificationName}",
                $"{blocker.BlockerType} · {blocker.Message}",
                blocker.PublishedAt,
                null,
                "person_training_blocker",
                blocker.Id.ToString(),
                blocker.TrainarrPublicationId.ToString()));

            if (blocker.ClearedAt is DateTimeOffset clearedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"training_blocker:{blocker.Id}:cleared",
                    personId,
                    "training_blocker",
                    "training_blocker_cleared",
                    $"Training blocker cleared: {blocker.QualificationName}",
                    blocker.Message,
                    clearedAt,
                    null,
                    "person_training_blocker",
                    blocker.Id.ToString(),
                    blocker.TrainarrPublicationId.ToString()));
            }
        }

        var personnelNotes = await db.PersonnelNotes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .ToListAsync(cancellationToken);

        foreach (var note in personnelNotes)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"personnel_note:{note.Id}:created",
                personId,
                "personnel_note",
                "personnel_note_created",
                $"Personnel note: {note.Subject}",
                $"{note.CategoryKey} · {note.VisibilityKey}",
                note.CreatedAt,
                note.CreatedByUserId,
                "personnel_note",
                note.Id.ToString(),
                null));
        }

        var personnelDocuments = await db.PersonnelDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .ToListAsync(cancellationToken);

        foreach (var document in personnelDocuments)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"personnel_document:{document.Id}:uploaded",
                personId,
                "personnel_document",
                "personnel_document_uploaded",
                $"Personnel document uploaded: {document.Title}",
                $"{document.DocumentTypeKey} · {document.FileName}",
                document.CreatedAt,
                document.UploadedByUserId,
                "personnel_document",
                document.Id.ToString(),
                null));
        }

        return entries;
    }
}
