using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

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
        var person = await db.People
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken);
        var orgUnitsById = await db.OrgUnits
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var roleTemplatesById = await db.RoleTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var roleAssignments = await db.PersonRoleAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var orgAssignments = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var leaveRequests = await db.TimekeepingLeaveRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var attendanceEvents = await db.TimekeepingAttendanceEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var availabilityBlocks = await db.TimekeepingAvailabilityBlocks
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var reviewCycles = await db.PerformanceReviewCycles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var performanceGoals = await db.PerformanceGoals
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var competencyAssessments = await db.PerformanceCompetencyAssessments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var feedbackEntries = await db.PerformanceFeedbackEntries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var improvementPlans = await db.PerformanceImprovementPlans
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var benefitEnrollments = await db.BenefitEnrollments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var benefitDependents = await db.BenefitDependents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var benefitBeneficiaries = await db.BenefitBeneficiaries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var compensationProfiles = await db.CompensationProfiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var compensationChangeRequests = await db.CompensationChangeRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var recruitingCandidates = await db.RecruitingCandidates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);
        var recruitingCandidateIds = recruitingCandidates.Select(x => x.Id).ToArray();
        var recruitingRequisitionIds = recruitingCandidates
            .Where(x => x.RecruitingRequisitionId.HasValue)
            .Select(x => x.RecruitingRequisitionId!.Value)
            .Distinct()
            .ToArray();
        var recruitingRequisitions = recruitingRequisitionIds.Length == 0
            ? []
            : await db.RecruitingRequisitions
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && recruitingRequisitionIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
        var recruitingInterviewStages = recruitingCandidateIds.Length == 0
            ? []
            : await db.RecruitingInterviewStages
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && recruitingCandidateIds.Contains(x.RecruitingCandidateId))
                .ToListAsync(cancellationToken);
        var recruitingOffers = recruitingCandidateIds.Length == 0
            ? []
            : await db.RecruitingOffers
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && recruitingCandidateIds.Contains(x.RecruitingCandidateId))
                .ToListAsync(cancellationToken);
        var applicationSubmissions = await db.EmploymentApplicationSubmissions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CreatedPersonId == personId)
            .ToListAsync(cancellationToken);

        AddEmploymentLifecycleEntries(entries, person);
        AddPlacementEntries(entries, orgAssignments, orgUnitsById, personId);
        AddRoleAssignmentEntries(entries, roleAssignments, roleTemplatesById, personId);
        AddLeaveEntries(entries, leaveRequests, personId);
        AddAttendanceEntries(entries, attendanceEvents, personId);
        AddAvailabilityEntries(entries, availabilityBlocks, personId);
        AddPerformanceCycleEntries(entries, reviewCycles, personId);
        AddPerformanceGoalEntries(entries, performanceGoals, personId);
        AddPerformanceCompetencyEntries(entries, competencyAssessments, personId);
        AddPerformanceFeedbackEntries(entries, feedbackEntries, personId);
        AddPerformancePlanEntries(entries, improvementPlans, personId);
        AddBenefitEntries(entries, benefitEnrollments, benefitDependents, benefitBeneficiaries, personId);
        AddCompensationEntries(entries, compensationProfiles, compensationChangeRequests, personId);
        AddRecruitingEntries(entries, recruitingCandidates, recruitingRequisitions, recruitingInterviewStages, recruitingOffers, applicationSubmissions, personId);

        var incidents = await db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var incident in incidents)
        {
            var sourceSuffix = string.IsNullOrWhiteSpace(incident.SourceProduct)
                ? string.Empty
                : $" · source: {incident.SourceProduct}";

            entries.Add(new PersonTimelineEntryResponse(
                $"incident:{incident.Id}:reported",
                personId,
                "incident",
                "incident_reported",
                $"Incident reported: {incident.Title}",
                $"{incident.ReasonCategoryKey} · {incident.Severity} · {incident.Status}{sourceSuffix}",
                incident.ReportedAt,
                incident.ReportedByUserId,
                "personnel_incident",
                incident.Id.ToString(),
                incident.SourceIncidentId?.ToString()));
        }

        var incidentIdStrings = incidents.Select(x => x.Id.ToString()).ToArray();
        var incidentStatusUpdates = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Action == "incident.status_update"
                && incidentIdStrings.Contains(x.TargetId))
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var statusUpdate in incidentStatusUpdates)
        {
            if (!Guid.TryParse(statusUpdate.TargetId, out var incidentId))
            {
                continue;
            }

            var incident = incidents.FirstOrDefault(x => x.Id == incidentId);
            if (incident is null)
            {
                continue;
            }

            var status = string.IsNullOrWhiteSpace(statusUpdate.ReasonCode)
                ? incident.Status
                : statusUpdate.ReasonCode!;
            var eventType = string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase)
                ? "incident_closed"
                : string.Equals(status, "open", StringComparison.OrdinalIgnoreCase)
                    ? "incident_reopened"
                    : "incident_status_updated";

            entries.Add(new PersonTimelineEntryResponse(
                $"incident:{incident.Id}:status:{statusUpdate.Id}",
                personId,
                "incident",
                eventType,
                string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase)
                    ? $"Incident closed: {incident.Title}"
                    : string.Equals(status, "open", StringComparison.OrdinalIgnoreCase)
                        ? $"Incident reopened: {incident.Title}"
                        : $"Incident status updated: {incident.Title}",
                $"{incident.ReasonCategoryKey} · {incident.Severity} · {status}",
                statusUpdate.OccurredAt,
                statusUpdate.ActorUserId,
                "personnel_incident",
                incident.Id.ToString(),
                incident.SourceIncidentId?.ToString()));
        }

        var incidentIds = incidents.Select(x => x.Id).ToArray();
        var incidentNotes = await db.IncidentNotes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && incidentIds.Contains(x.IncidentId))
            .ToListAsync(cancellationToken);

        foreach (var note in incidentNotes)
        {
            var noteEventType = string.Equals(note.NoteTypeKey, "corrective_action", StringComparison.OrdinalIgnoreCase)
                ? "incident_corrective_action_added"
                : "incident_note_added";
            entries.Add(new PersonTimelineEntryResponse(
                $"incident:{note.IncidentId}:note:{note.Id}:created",
                personId,
                "incident",
                noteEventType,
                string.Equals(note.NoteTypeKey, "corrective_action", StringComparison.OrdinalIgnoreCase)
                    ? $"Corrective action added: {note.Subject}"
                    : $"Incident note added: {note.Subject}",
                $"{note.NoteTypeKey} · {note.Status}",
                note.CreatedAt,
                note.CreatedByUserId,
                "personnel_incident",
                note.IncidentId.ToString(),
                note.Id.ToString()));

            if (string.Equals(note.NoteTypeKey, "corrective_action", StringComparison.OrdinalIgnoreCase)
                && note.CompletedAt is DateTimeOffset completedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"incident:{note.IncidentId}:note:{note.Id}:completed",
                    personId,
                    "incident",
                    "incident_corrective_action_completed",
                    $"Corrective action completed: {note.Subject}",
                    $"{note.NoteTypeKey} · completed",
                    completedAt,
                    note.CreatedByUserId,
                    "personnel_incident",
                    note.IncidentId.ToString(),
                    note.Id.ToString()));
            }
        }

        var incidentAttachments = await db.IncidentAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && incidentIds.Contains(x.IncidentId))
            .ToListAsync(cancellationToken);

        foreach (var attachment in incidentAttachments)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"incident:{attachment.IncidentId}:attachment:{attachment.Id}",
                personId,
                "incident",
                "incident_attachment_uploaded",
                $"Incident attachment uploaded: {attachment.Title}",
                $"{attachment.FileName} · {attachment.ContentType}",
                attachment.CreatedAt,
                attachment.UploadedByUserId,
                "personnel_incident",
                attachment.IncidentId.ToString(),
                attachment.Id.ToString()));
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

        var offboardingRecords = await db.PersonOffboardingRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .ToListAsync(cancellationToken);

        foreach (var offboarding in offboardingRecords)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"offboarding:{offboarding.Id}:started",
                personId,
                "offboarding",
                "offboarding_started",
                "Workforce offboarding started",
                offboarding.SeparationReason ?? offboarding.TargetEmploymentStatus,
                offboarding.StartedAt,
                offboarding.StartedByUserId,
                "person_offboarding_record",
                offboarding.Id.ToString(),
                null));

            if (offboarding.CompletedAt is DateTimeOffset completedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"offboarding:{offboarding.Id}:completed",
                    personId,
                    "offboarding",
                    "offboarding_completed",
                    "Workforce offboarding completed",
                    offboarding.TargetEmploymentStatus,
                    completedAt,
                    offboarding.CompletedByUserId,
                    "person_offboarding_record",
                    offboarding.Id.ToString(),
                    null));
            }
        }

        return entries;
    }

    private static void AddEmploymentLifecycleEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        StaffPerson person)
    {
        var lifecycleAt = person.CurrentEmploymentActionAt
            ?? person.StartDate
            ?? person.CreatedAt;
        var action = NormalizeEmploymentAction(person.CurrentEmploymentAction, person.EmploymentStatus);

        entries.Add(new PersonTimelineEntryResponse(
            $"employment:{person.Id}:status:{person.EmploymentStatus}",
            person.Id,
            "employment",
            $"employment_{person.EmploymentStatus}",
            $"Employment status: {Labelize(person.EmploymentStatus)}",
            BuildEmploymentSnapshotSummary(person),
            person.UpdatedAt,
            null,
            "staff_person",
            person.Id.ToString(),
            null));

        entries.Add(new PersonTimelineEntryResponse(
            $"employment:{person.Id}:action:{action}:{lifecycleAt.UtcTicks}",
            person.Id,
            "employment",
            $"employment_{action}",
            LabelizeEmploymentAction(action),
            BuildEmploymentActionDetail(person, action),
            lifecycleAt,
            null,
            "staff_person",
            person.Id.ToString(),
            null));

        if (!string.IsNullOrWhiteSpace(person.WorkerCategory) || !string.IsNullOrWhiteSpace(person.FlsaStatus) || !string.IsNullOrWhiteSpace(person.EmploymentType))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"employment:{person.Id}:classification",
                person.Id,
                "classification",
                "classification_snapshot",
                "Employment classification snapshot",
                string.Join(" · ", new[]
                {
                    person.WorkerCategory,
                    person.FlsaStatus,
                    person.EmploymentType,
                    person.PositionNumber,
                }.Where(x => !string.IsNullOrWhiteSpace(x))),
                person.UpdatedAt,
                null,
                "staff_person",
                person.Id.ToString(),
                null));
        }

        if (!person.EligibleForRehire)
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"employment:{person.Id}:rehire_eligibility",
                person.Id,
                "employment",
                "rehire_eligibility_updated",
                "Rehire eligibility changed",
                "Eligible for rehire: No",
                person.UpdatedAt,
                null,
                "staff_person",
                person.Id.ToString(),
                null));
        }
    }

    private static void AddPlacementEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<OrgUnitAssignment> assignments,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById,
        Guid personId)
    {
        foreach (var assignment in assignments
                     .OrderBy(x => x.EffectiveAt)
                     .ThenBy(x => x.CreatedAt))
        {
            var eventType = assignment.Status switch
            {
                "planned" => "placement_planned",
                "active" => "placement_activated",
                "ended" => "placement_ended",
                "canceled" => "placement_canceled",
                _ => "placement_changed",
            };

            var summary = BuildPlacementSummary(assignment, orgUnitsById);
            entries.Add(new PersonTimelineEntryResponse(
                $"placement:{assignment.Id}:{assignment.Status}",
                personId,
                "placement",
                eventType,
                $"Org placement {Labelize(assignment.Status)}",
                summary,
                assignment.EffectiveAt,
                null,
                "org_assignment",
                assignment.Id.ToString(),
                null));

            if (assignment.EndsAt is DateTimeOffset endsAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"placement:{assignment.Id}:ended",
                    personId,
                    "placement",
                    "placement_effective_end",
                    "Org placement ended",
                    summary,
                    endsAt,
                    null,
                    "org_assignment",
                    assignment.Id.ToString(),
                    null));
            }
        }
    }

    private static void AddRoleAssignmentEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PersonRoleAssignment> assignments,
        IReadOnlyDictionary<Guid, RoleTemplate> roleTemplatesById,
        Guid personId)
    {
        foreach (var assignment in assignments.OrderBy(x => x.CreatedAt))
        {
            roleTemplatesById.TryGetValue(assignment.RoleTemplateId, out var roleTemplate);
            var title = assignment.Status switch
            {
                "active" => "Role assignment activated",
                "inactive" => "Role assignment inactivated",
                _ => "Role assignment updated",
            };

            entries.Add(new PersonTimelineEntryResponse(
                $"role:{assignment.Id}:{assignment.Status}:{assignment.UpdatedAt.UtcTicks}",
                personId,
                "permission",
                $"role_assignment_{assignment.Status}",
                title,
                $"{roleTemplate?.Name ?? roleTemplate?.RoleKey ?? assignment.RoleTemplateId.ToString()} · {assignment.ScopeType} · {assignment.Status}",
                assignment.UpdatedAt,
                null,
                "person_role_assignment",
                assignment.Id.ToString(),
                null));
        }
    }

    private static string BuildEmploymentSnapshotSummary(StaffPerson person)
    {
        var pieces = new List<string>();
        if (!string.IsNullOrWhiteSpace(person.WorkRelationshipType))
        {
            pieces.Add(person.WorkRelationshipType!);
        }

        if (!string.IsNullOrWhiteSpace(person.WorkerCategory))
        {
            pieces.Add(person.WorkerCategory!);
        }

        if (!string.IsNullOrWhiteSpace(person.FlsaStatus))
        {
            pieces.Add(person.FlsaStatus!);
        }

        if (!string.IsNullOrWhiteSpace(person.LeaveStatus))
        {
            pieces.Add($"leave:{person.LeaveStatus}");
        }

        if (person.EligibleForRehire)
        {
            pieces.Add("rehire:eligible");
        }
        else
        {
            pieces.Add("rehire:not_eligible");
        }

        return pieces.Count == 0 ? "Employment snapshot updated" : string.Join(" · ", pieces);
    }

    private static string BuildEmploymentActionDetail(StaffPerson person, string action)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(person.PositionNumber))
        {
            parts.Add($"Position {person.PositionNumber}");
        }

        if (person.PrimaryOrgUnitId is Guid primaryOrgUnitId)
        {
            parts.Add($"Org unit {primaryOrgUnitId:N}");
        }

        if (person.HomeBaseLocationId is Guid locationId)
        {
            parts.Add($"Location {locationId:N}");
        }

        return parts.Count == 0
            ? $"{LabelizeEmploymentAction(action)} recorded."
            : string.Join(" · ", parts);
    }

    private static string BuildPlacementSummary(
        OrgUnitAssignment assignment,
        IReadOnlyDictionary<Guid, OrgUnit> orgUnitsById)
    {
        var pieces = new List<string>
        {
            $"status:{assignment.Status}",
        };

        if (orgUnitsById.TryGetValue(assignment.SiteOrgUnitId, out var site))
        {
            pieces.Add($"site:{site.Name}");
        }

        if (orgUnitsById.TryGetValue(assignment.DepartmentOrgUnitId, out var department))
        {
            pieces.Add($"department:{department.Name}");
        }

        if (orgUnitsById.TryGetValue(assignment.TeamOrgUnitId, out var team))
        {
            pieces.Add($"team:{team.Name}");
        }

        if (orgUnitsById.TryGetValue(assignment.PositionOrgUnitId, out var position))
        {
            pieces.Add($"position:{position.Name}");
        }

        if (assignment.IsPrimary)
        {
            pieces.Add("primary");
        }

        if (!string.IsNullOrWhiteSpace(assignment.Reason))
        {
            pieces.Add(assignment.Reason!);
        }

        return string.Join(" · ", pieces);
    }

    private static void AddLeaveEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<LeaveRequest> leaveRequests,
        Guid personId)
    {
        foreach (var leave in leaveRequests.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RequestedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"leave:{leave.Id}:{leave.Status}",
                personId,
                "timekeeping_leave",
                $"leave_request_{leave.Status}",
                $"Leave request {Labelize(leave.Status)}",
                BuildLeaveSummary(leave),
                leave.RequestedAt,
                leave.RequestedByPersonId,
                "timekeeping_leave_request",
                leave.Id.ToString(),
                leave.SourceRef));

            if (leave.ReviewedAt is DateTimeOffset reviewedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"leave:{leave.Id}:reviewed:{leave.Status}",
                    personId,
                    "timekeeping_leave",
                    $"leave_request_{leave.Status}_reviewed",
                    $"Leave request reviewed: {Labelize(leave.Status)}",
                    leave.ReviewNotes ?? BuildLeaveSummary(leave),
                    reviewedAt,
                    leave.ApprovedByPersonId,
                    "timekeeping_leave_request",
                    leave.Id.ToString(),
                    leave.SourceRef));
            }
        }
    }

    private static void AddAttendanceEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<AttendanceEvent> attendanceEvents,
        Guid personId)
    {
        foreach (var attendance in attendanceEvents.OrderByDescending(x => x.OccurredAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"attendance:{attendance.Id}:{attendance.Status}",
                personId,
                "attendance",
                $"attendance_{attendance.EventType}",
                $"Attendance event: {Labelize(attendance.EventType)}",
                BuildAttendanceSummary(attendance),
                attendance.OccurredAt,
                attendance.ReviewedByPersonId,
                "attendance_event",
                attendance.Id.ToString(),
                attendance.SourceRef));

            if (attendance.ReviewedAt is DateTimeOffset reviewedAt)
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"attendance:{attendance.Id}:reviewed",
                    personId,
                    "attendance",
                    "attendance_event_reviewed",
                    $"Attendance event reviewed: {Labelize(attendance.EventType)}",
                    attendance.ResolutionNotes ?? BuildAttendanceSummary(attendance),
                    reviewedAt,
                    attendance.ReviewedByPersonId,
                    "attendance_event",
                    attendance.Id.ToString(),
                    attendance.SourceRef));
            }
        }
    }

    private static void AddAvailabilityEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<AvailabilityBlock> availabilityBlocks,
        Guid personId)
    {
        foreach (var availability in availabilityBlocks.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"availability:{availability.Id}:{availability.Status}",
                personId,
                "availability",
                "availability_block_updated",
                $"Availability {Labelize(availability.Status)}",
                BuildAvailabilitySummary(availability),
                availability.UpdatedAt,
                null,
                "availability_block",
                availability.Id.ToString(),
                null));
        }
    }

    private static string BuildLeaveSummary(LeaveRequest leave)
    {
        var parts = new List<string>
        {
            leave.LeaveType,
            $"{leave.StartDate:yyyy-MM-dd} to {leave.EndDate:yyyy-MM-dd}",
        };

        if (leave.IsIntermittent)
        {
            parts.Add("intermittent");
        }

        parts.Add(leave.IsPaid ? "paid" : "unpaid");

        if (!string.IsNullOrWhiteSpace(leave.Reason))
        {
            parts.Add(leave.Reason!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildAttendanceSummary(AttendanceEvent attendance)
    {
        var parts = new List<string>
        {
            attendance.Severity,
            $"{attendance.PointValue} points",
            attendance.Status,
        };

        if (!string.IsNullOrWhiteSpace(attendance.Notes))
        {
            parts.Add(attendance.Notes!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildAvailabilitySummary(AvailabilityBlock availability)
    {
        var parts = new List<string>
        {
            availability.AvailabilityType,
            availability.DayOfWeekMaskCsv,
            $"{availability.StartLocalTime:HH\\:mm}-{availability.EndLocalTime:HH\\:mm}",
        };

        if (availability.EffectiveEndDate is DateOnly endDate)
        {
            parts.Add($"{availability.EffectiveStartDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        }
        else
        {
            parts.Add($"from {availability.EffectiveStartDate:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(availability.Notes))
        {
            parts.Add(availability.Notes!);
        }

        return string.Join(" · ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static void AddPerformanceCycleEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PerformanceReviewCycle> cycles,
        Guid personId)
    {
        foreach (var cycle in cycles.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"performance_cycle:{cycle.Id}:{cycle.Status}",
                personId,
                "performance",
                $"performance_cycle_{cycle.Status}",
                $"Performance cycle {Labelize(cycle.Status)}",
                BuildPerformanceCycleSummary(cycle),
                cycle.UpdatedAt,
                cycle.ManagerPersonId,
                "performance_review_cycle",
                cycle.Id.ToString(),
                cycle.SourceRef));
        }
    }

    private static void AddPerformanceGoalEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PerformanceGoal> goals,
        Guid personId)
    {
        foreach (var goal in goals.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"performance_goal:{goal.Id}:{goal.Status}",
                personId,
                "performance",
                $"performance_goal_{goal.Status}",
                $"Performance goal {Labelize(goal.Status)}",
                BuildPerformanceGoalSummary(goal),
                goal.UpdatedAt,
                goal.OwnerPersonId,
                "performance_goal",
                goal.Id.ToString(),
                null));
        }
    }

    private static void AddPerformanceCompetencyEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PerformanceCompetencyAssessment> assessments,
        Guid personId)
    {
        foreach (var assessment in assessments.OrderBy(x => x.CompetencyName))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"performance_competency:{assessment.Id}:{assessment.Status}",
                personId,
                "performance",
                $"performance_competency_{assessment.Status}",
                $"Competency assessment: {assessment.CompetencyName}",
                BuildPerformanceCompetencySummary(assessment),
                assessment.UpdatedAt,
                assessment.AssessedByPersonId,
                "performance_competency_assessment",
                assessment.Id.ToString(),
                null));
        }
    }

    private static void AddPerformanceFeedbackEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PerformanceFeedbackEntry> feedbackEntries,
        Guid personId)
    {
        foreach (var feedback in feedbackEntries.OrderByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"performance_feedback:{feedback.Id}",
                personId,
                "performance",
                $"performance_feedback_{feedback.FeedbackType}",
                $"Performance feedback: {feedback.Subject}",
                BuildPerformanceFeedbackSummary(feedback),
                feedback.CreatedAt,
                feedback.AuthorPersonId,
                "performance_feedback_entry",
                feedback.Id.ToString(),
                null));
        }
    }

    private static void AddPerformancePlanEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<PerformanceImprovementPlan> plans,
        Guid personId)
    {
        foreach (var plan in plans.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"performance_plan:{plan.Id}:{plan.Status}",
                personId,
                "performance",
                $"performance_plan_{plan.Status}",
                $"Performance plan {Labelize(plan.Status)}",
                BuildPerformancePlanSummary(plan),
                plan.UpdatedAt,
                plan.ManagerPersonId ?? plan.HrOwnerPersonId,
                "performance_improvement_plan",
                plan.Id.ToString(),
                plan.SourceRef));
        }
    }

    private static string BuildPerformanceCycleSummary(PerformanceReviewCycle cycle)
    {
        var parts = new List<string>
        {
            cycle.CycleType,
            $"{cycle.StartDate:yyyy-MM-dd} to {cycle.EndDate:yyyy-MM-dd}",
        };

        if (!string.IsNullOrWhiteSpace(cycle.OverallRating))
        {
            parts.Add(cycle.OverallRating!);
        }

        if (cycle.PromotionReady)
        {
            parts.Add("promotion-ready");
        }

        if (cycle.SuccessionReady)
        {
            parts.Add("succession-ready");
        }

        if (!string.IsNullOrWhiteSpace(cycle.Summary))
        {
            parts.Add(cycle.Summary!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildPerformanceGoalSummary(PerformanceGoal goal)
    {
        var parts = new List<string>
        {
            goal.GoalType,
            goal.Priority,
            $"{goal.ProgressPercent:0.#}%",
        };

        if (goal.TargetDate is DateOnly targetDate)
        {
            parts.Add($"target {targetDate:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(goal.SuccessMetric))
        {
            parts.Add(goal.SuccessMetric!);
        }

        if (!string.IsNullOrWhiteSpace(goal.ResultSummary))
        {
            parts.Add(goal.ResultSummary!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildPerformanceCompetencySummary(PerformanceCompetencyAssessment assessment)
    {
        var parts = new List<string>
        {
            assessment.ExpectedLevel,
            assessment.CurrentLevel,
            assessment.Rating,
        };

        if (!string.IsNullOrWhiteSpace(assessment.Notes))
        {
            parts.Add(assessment.Notes!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildPerformanceFeedbackSummary(PerformanceFeedbackEntry feedback)
    {
        var parts = new List<string>
        {
            feedback.FeedbackType,
            feedback.Visibility,
        };

        if (!string.IsNullOrWhiteSpace(feedback.Sentiment))
        {
            parts.Add(feedback.Sentiment!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildPerformancePlanSummary(PerformanceImprovementPlan plan)
    {
        var parts = new List<string>
        {
            $"{plan.StartDate:yyyy-MM-dd}",
            plan.Status,
        };

        if (plan.TargetDate is DateOnly targetDate)
        {
            parts.Add($"target {targetDate:yyyy-MM-dd}");
        }

        if (!string.IsNullOrWhiteSpace(plan.CheckInCadence))
        {
            parts.Add(plan.CheckInCadence!);
        }

        if (!string.IsNullOrWhiteSpace(plan.Expectations))
        {
            parts.Add(plan.Expectations!);
        }

        return string.Join(" · ", parts);
    }

    private static void AddBenefitEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<BenefitEnrollment> enrollments,
        IEnumerable<BenefitDependent> dependents,
        IEnumerable<BenefitBeneficiary> beneficiaries,
        Guid personId)
    {
        foreach (var enrollment in enrollments.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"benefit_enrollment:{enrollment.Id}:{enrollment.EnrollmentStatus}",
                personId,
                "benefits",
                $"benefit_enrollment_{enrollment.EnrollmentStatus}",
                $"Benefit enrollment {Labelize(enrollment.EnrollmentStatus)}",
                BuildBenefitEnrollmentSummary(enrollment),
                enrollment.UpdatedAt,
                null,
                "benefit_enrollment",
                enrollment.Id.ToString(),
                enrollment.SourceRef));
        }

        foreach (var dependent in dependents.OrderByDescending(x => x.UpdatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"benefit_dependent:{dependent.Id}:{dependent.CoverageStatus}",
                personId,
                "benefits",
                $"benefit_dependent_{dependent.CoverageStatus}",
                $"Benefit dependent {dependent.FirstName} {dependent.LastName}",
                BuildBenefitDependentSummary(dependent),
                dependent.UpdatedAt,
                null,
                "benefit_dependent",
                dependent.Id.ToString(),
                null));
        }

        foreach (var beneficiary in beneficiaries.OrderByDescending(x => x.UpdatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"benefit_beneficiary:{beneficiary.Id}:{beneficiary.Status}",
                personId,
                "benefits",
                $"benefit_beneficiary_{beneficiary.Status}",
                $"Benefit beneficiary {beneficiary.FirstName} {beneficiary.LastName}",
                BuildBenefitBeneficiarySummary(beneficiary),
                beneficiary.UpdatedAt,
                null,
                "benefit_beneficiary",
                beneficiary.Id.ToString(),
                null));
        }
    }

    private static void AddCompensationEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<CompensationProfile> profiles,
        IEnumerable<CompensationChangeRequest> requests,
        Guid personId)
    {
        foreach (var profile in profiles.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"compensation_profile:{profile.Id}:{profile.Status}",
                personId,
                "compensation",
                $"compensation_profile_{profile.Status}",
                $"Compensation profile {Labelize(profile.Status)}",
                BuildCompensationProfileSummary(profile),
                profile.UpdatedAt,
                null,
                "compensation_profile",
                profile.Id.ToString(),
                profile.SourceRef));
        }

        foreach (var request in requests.OrderByDescending(x => x.CreatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"compensation_request:{request.Id}:{request.Status}",
                personId,
                "compensation",
                $"compensation_request_{request.Status}",
                $"Compensation change request {Labelize(request.Status)}",
                BuildCompensationChangeSummary(request),
                request.UpdatedAt,
                request.ApprovedByPersonId ?? request.RequestedByPersonId,
                "compensation_change_request",
                request.Id.ToString(),
                request.SourceRef));
        }
    }

    private static void AddRecruitingEntries(
        ICollection<PersonTimelineEntryResponse> entries,
        IEnumerable<RecruitingCandidate> candidates,
        IEnumerable<RecruitingRequisition> requisitions,
        IEnumerable<RecruitingInterviewStage> interviewStages,
        IEnumerable<RecruitingOffer> offers,
        IEnumerable<EmploymentApplicationSubmission> submissions,
        Guid personId)
    {
        var requisitionsById = requisitions.ToDictionary(x => x.Id);
        var candidatesById = candidates.ToDictionary(x => x.Id);

        foreach (var submission in submissions.OrderByDescending(x => x.SubmittedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"recruiting_submission:{submission.Id}:received",
                personId,
                "recruiting",
                "recruiting_application_submitted",
                $"Application submitted: {submission.ApplicantDisplayName}",
                BuildRecruitingSubmissionSummary(submission),
                submission.SubmittedAt,
                null,
                "employment_application_submission",
                submission.Id.ToString(),
                submission.CreatedCandidateId?.ToString()));

            if (submission.CreatedCandidateId is Guid candidateId && candidatesById.TryGetValue(candidateId, out var candidate))
            {
                entries.Add(new PersonTimelineEntryResponse(
                    $"recruiting_submission:{submission.Id}:candidate",
                    personId,
                    "recruiting",
                    "recruiting_candidate_created",
                    $"Candidate created: {candidate.CandidateName}",
                    BuildRecruitingCandidateSummary(candidate, requisitionsById),
                    candidate.UpdatedAt,
                    null,
                    "recruiting_candidate",
                    candidate.Id.ToString(),
                    submission.Id.ToString()));
            }
        }

        foreach (var candidate in candidates.OrderByDescending(x => x.UpdatedAt))
        {
            entries.Add(new PersonTimelineEntryResponse(
                $"recruiting_candidate:{candidate.Id}:{candidate.Stage}",
                personId,
                "recruiting",
                $"recruiting_candidate_{candidate.Stage}",
                $"Candidate {candidate.CandidateName}",
                BuildRecruitingCandidateSummary(candidate, requisitionsById),
                candidate.UpdatedAt,
                candidate.PersonId,
                "recruiting_candidate",
                candidate.Id.ToString(),
                candidate.EmploymentApplicationSubmissionId?.ToString()));
        }

        foreach (var stage in interviewStages.OrderByDescending(x => x.CreatedAt))
        {
            var candidate = candidatesById.TryGetValue(stage.RecruitingCandidateId, out var recruitingCandidate)
                ? recruitingCandidate
                : null;

            entries.Add(new PersonTimelineEntryResponse(
                $"recruiting_stage:{stage.Id}:{stage.Status}",
                personId,
                "recruiting",
                $"recruiting_interview_stage_{stage.Status}",
                $"Interview stage: {stage.StageName}",
                BuildRecruitingStageSummary(stage, candidate, requisitionsById),
                stage.UpdatedAt,
                stage.InterviewerPersonId,
                "recruiting_interview_stage",
                stage.Id.ToString(),
                stage.RecruitingCandidateId.ToString()));
        }

        foreach (var offer in offers.OrderByDescending(x => x.CreatedAt))
        {
            var candidate = candidatesById.TryGetValue(offer.RecruitingCandidateId, out var recruitingCandidate)
                ? recruitingCandidate
                : null;

            entries.Add(new PersonTimelineEntryResponse(
                $"recruiting_offer:{offer.Id}:{offer.Status}",
                personId,
                "recruiting",
                $"recruiting_offer_{offer.Status}",
                $"Offer {Labelize(offer.Status)}",
                BuildRecruitingOfferSummary(offer, candidate, requisitionsById),
                offer.UpdatedAt,
                offer.ApprovedByPersonId,
                "recruiting_offer",
                offer.Id.ToString(),
                offer.RecruitingCandidateId.ToString()));
        }
    }

    private static string BuildRecruitingSubmissionSummary(EmploymentApplicationSubmission submission)
    {
        var parts = new List<string>
        {
            submission.TemplateKey,
            submission.Status,
        };

        if (!string.IsNullOrWhiteSpace(submission.ReviewerNotes))
        {
            parts.Add(submission.ReviewerNotes!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildRecruitingCandidateSummary(
        RecruitingCandidate candidate,
        IReadOnlyDictionary<Guid, RecruitingRequisition> requisitionsById)
    {
        var parts = new List<string>
        {
            candidate.SourceType,
            candidate.Status,
            candidate.Stage,
        };

        if (candidate.RecruitingRequisitionId is Guid requisitionId && requisitionsById.TryGetValue(requisitionId, out var requisition))
        {
            parts.Add(requisition.Title);
        }

        if (candidate.Score is decimal score)
        {
            parts.Add($"score:{score:0.##}");
        }

        if (!string.IsNullOrWhiteSpace(candidate.Notes))
        {
            parts.Add(candidate.Notes!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildRecruitingStageSummary(
        RecruitingInterviewStage stage,
        RecruitingCandidate? candidate,
        IReadOnlyDictionary<Guid, RecruitingRequisition> requisitionsById)
    {
        var parts = new List<string>
        {
            stage.Status,
        };

        if (candidate is not null)
        {
            parts.Add(candidate.CandidateName);
            if (candidate.RecruitingRequisitionId is Guid requisitionId && requisitionsById.TryGetValue(requisitionId, out var requisition))
            {
                parts.Add(requisition.Title);
            }
        }

        if (stage.Score is decimal score)
        {
            parts.Add($"score:{score:0.##}");
        }

        if (!string.IsNullOrWhiteSpace(stage.Recommendation))
        {
            parts.Add(stage.Recommendation!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildRecruitingOfferSummary(
        RecruitingOffer offer,
        RecruitingCandidate? candidate,
        IReadOnlyDictionary<Guid, RecruitingRequisition> requisitionsById)
    {
        var parts = new List<string>
        {
            offer.PayBasis,
            offer.Status,
        };

        if (candidate is not null)
        {
            parts.Add(candidate.CandidateName);
            if (candidate.RecruitingRequisitionId is Guid requisitionId && requisitionsById.TryGetValue(requisitionId, out var requisition))
            {
                parts.Add(requisition.Title);
            }
        }

        if (offer.AnnualSalary is decimal salary)
        {
            parts.Add($"salary:{salary:0.##}");
        }

        if (offer.HourlyRate is decimal rate)
        {
            parts.Add($"rate:{rate:0.####}");
        }

        if (offer.StartDate is DateOnly startDate)
        {
            parts.Add(startDate.ToString("yyyy-MM-dd"));
        }

        return string.Join(" · ", parts);
    }

    private static string BuildBenefitEnrollmentSummary(BenefitEnrollment enrollment)
    {
        var parts = new List<string>
        {
            enrollment.BenefitType,
            enrollment.PlanName,
            enrollment.BenefitClass,
            enrollment.CoverageLevel,
            enrollment.EligibilityStatus,
        };

        if (!string.IsNullOrWhiteSpace(enrollment.CarrierExportStatus))
        {
            parts.Add($"carrier:{enrollment.CarrierExportStatus}");
        }

        if (enrollment.EffectiveEndDate is DateOnly endDate)
        {
            parts.Add($"{enrollment.EffectiveStartDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        }
        else
        {
            parts.Add($"from {enrollment.EffectiveStartDate:yyyy-MM-dd}");
        }

        return string.Join(" · ", parts);
    }

    private static string BuildBenefitDependentSummary(BenefitDependent dependent)
    {
        var parts = new List<string>
        {
            dependent.Relationship,
            dependent.CoverageStatus,
        };

        if (dependent.DateOfBirth is DateOnly dob)
        {
            parts.Add(dob.ToString("yyyy-MM-dd"));
        }

        if (dependent.IsStudent)
        {
            parts.Add("student");
        }

        if (dependent.IsDisabled)
        {
            parts.Add("disabled");
        }

        return string.Join(" · ", parts);
    }

    private static string BuildBenefitBeneficiarySummary(BenefitBeneficiary beneficiary)
    {
        var parts = new List<string>
        {
            beneficiary.Relationship,
            $"{beneficiary.AllocationPercent:0.#}%",
            beneficiary.Status,
        };

        if (!string.IsNullOrWhiteSpace(beneficiary.DesignationType))
        {
            parts.Add(beneficiary.DesignationType!);
        }

        return string.Join(" · ", parts);
    }

    private static string BuildCompensationProfileSummary(CompensationProfile profile)
    {
        var parts = new List<string>
        {
            profile.PayBasis,
            profile.PayGrade,
            profile.PayBand,
        };

        if (profile.BaseRate is decimal rate)
        {
            parts.Add($"rate:{rate:0.####}");
        }

        if (profile.AnnualSalary is decimal salary)
        {
            parts.Add($"salary:{salary:0.##}");
        }

        if (profile.OvertimeEligible)
        {
            parts.Add("overtime-eligible");
        }

        if (profile.BonusEligible)
        {
            parts.Add("bonus-eligible");
        }

        return string.Join(" · ", parts);
    }

    private static string BuildCompensationChangeSummary(CompensationChangeRequest request)
    {
        var parts = new List<string>
        {
            request.RequestType,
            request.ReasonCode,
            request.Status,
        };

        if (request.EffectiveDate is DateOnly effectiveDate)
        {
            parts.Add(effectiveDate.ToString("yyyy-MM-dd"));
        }

        return string.Join(" · ", parts);
    }

    private static string NormalizeEmploymentAction(string? action, string employmentStatus)
    {
        if (!string.IsNullOrWhiteSpace(action))
        {
            return action.Trim().ToLowerInvariant();
        }

        return employmentStatus switch
        {
            "active" => "hire",
            "leave" => "leave_start",
            "suspended" => "suspension",
            "terminated" => "termination",
            "inactive" => "termination",
            _ => "status_update",
        };
    }

    private static string LabelizeEmploymentAction(string action) =>
        action switch
        {
            "hire" => "Hire",
            "rehire" => "Rehire",
            "transfer" => "Transfer",
            "promotion" => "Promotion",
            "demotion" => "Demotion",
            "supervisor_change" => "Supervisor change",
            "location_change" => "Location change",
            "job_change" => "Job / position change",
            "leave_start" => "Leave started",
            "leave_return" => "Leave returned",
            "suspension" => "Suspension",
            "termination" => "Termination",
            _ => "Employment action",
        };

    private static string Labelize(string value) =>
        value.Replace('_', ' ');
}
