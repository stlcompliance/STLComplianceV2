using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonOffboardingService(
    StaffArrDbContext db,
    RoleTemplateService roleTemplateService,
    OrgUnitAssignmentService orgUnitAssignmentService,
    NexArrLoginDisableClient nexArrLoginDisableClient,
    StaffArrMaintainArrTechnicianRefSyncService maintainarrTechnicianRefSync,
    IStaffArrAuditService audit)
{
    public const string ManageAction = "staffarr.offboarding.manage";

    public async Task<PersonOffboardingResponse> StartAsync(
        Guid tenantId,
        Guid actorUserId,
        StartPersonOffboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        OffboardingRules.ValidateSeparationDate(request.SeparationDate);
        var targetStatus = OffboardingRules.NormalizeTargetEmploymentStatus(request.TargetEmploymentStatus);
        var separationReason = OffboardingRules.NormalizeSeparationReason(request.SeparationReason);

        var person = await db.People.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.PersonId,
            cancellationToken);
        if (person is null)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        if (!string.Equals(person.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "offboarding.invalid_person",
                "Offboarding can only be started for active people.",
                409);
        }

        var activeOffboarding = await db.PersonOffboardingRecords.AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId
                    && x.PersonId == request.PersonId
                    && x.Status == OffboardingStatuses.InProgress,
                cancellationToken);
        if (activeOffboarding)
        {
            throw new StlApiException(
                "offboarding.already_in_progress",
                "An offboarding workflow is already in progress for this person.",
                409);
        }

        if (request.NewManagerPersonIdForReports is Guid managerId)
        {
            await ValidateManagerForReportsAsync(tenantId, request.PersonId, managerId, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var record = new PersonOffboardingRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            Status = OffboardingStatuses.InProgress,
            SeparationDate = request.SeparationDate,
            SeparationReason = separationReason,
            TargetEmploymentStatus = targetStatus,
            DisableLoginRequested = request.DisableLoginRequested,
            NewManagerPersonIdForReports = request.NewManagerPersonIdForReports,
            StartedByUserId = actorUserId,
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var context = await BuildContextAsync(tenantId, request.PersonId, cancellationToken);
        record.Steps = BuildInitialSteps(tenantId, record.Id, context, request.NewManagerPersonIdForReports);

        db.PersonOffboardingRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "offboarding.start",
            tenantId,
            actorUserId,
            "person_offboarding_record",
            record.Id.ToString(),
            OffboardingStatuses.InProgress,
            reasonCode: separationReason,
            cancellationToken: cancellationToken);

        return await MapAsync(tenantId, record.Id, cancellationToken);
    }

    public async Task<PersonOffboardingResponse?> GetActiveForPersonAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var recordId = await db.PersonOffboardingRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.StartedAt)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return recordId == Guid.Empty
            ? null
            : await MapAsync(tenantId, recordId, cancellationToken);
    }

    public async Task<PersonOffboardingResponse> GetByIdAsync(
        Guid tenantId,
        Guid offboardingId,
        CancellationToken cancellationToken = default) =>
        await MapAsync(tenantId, offboardingId, cancellationToken);

    public async Task<PersonOffboardingResponse> ExecuteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid offboardingId,
        ExecutePersonOffboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        var record = await db.PersonOffboardingRecords
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == offboardingId, cancellationToken);
        if (record is null)
        {
            throw new StlApiException("offboarding.not_found", "Offboarding record was not found.", 404);
        }

        if (!string.Equals(record.Status, OffboardingStatuses.InProgress, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "offboarding.invalid_state",
                "Only in-progress offboarding records can be executed.",
                409);
        }

        if (request.NewManagerPersonIdForReports is Guid managerId)
        {
            await ValidateManagerForReportsAsync(tenantId, record.PersonId, managerId, cancellationToken);
            record.NewManagerPersonIdForReports = managerId;
        }

        var context = await BuildContextAsync(tenantId, record.PersonId, cancellationToken);
        if (context.ActiveDirectReportCount > 0 && record.NewManagerPersonIdForReports is null)
        {
            throw new StlApiException(
                "offboarding.reassign_reports_required",
                "Provide a replacement manager before executing offboarding for a person with active direct reports.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var person = await db.People.FirstAsync(
            x => x.TenantId == tenantId && x.Id == record.PersonId,
            cancellationToken);

        await CompleteStepAsync(record, OffboardingStepKeys.ReviewAccess, actorUserId, now, cancellationToken);

        if (context.ActiveDirectReportCount > 0)
        {
            var newManagerId = record.NewManagerPersonIdForReports!.Value;
            var subordinates = await db.People
                .Where(x => x.TenantId == tenantId
                    && x.ManagerPersonId == record.PersonId
                    && x.EmploymentStatus == "active")
                .ToListAsync(cancellationToken);
            foreach (var subordinate in subordinates)
            {
                subordinate.ManagerPersonId = newManagerId;
                subordinate.UpdatedAt = now;
            }

            await CompleteStepAsync(
                record,
                OffboardingStepKeys.ReassignDirectReports,
                actorUserId,
                now,
                $"Reassigned {subordinates.Count} direct report(s).",
                cancellationToken);
        }
        else
        {
            await SkipStepAsync(
                record,
                OffboardingStepKeys.ReassignDirectReports,
                actorUserId,
                now,
                "No active direct reports require reassignment.",
                cancellationToken);
        }

        var selectableOrgAssignments = await db.OrgUnitAssignments
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == record.PersonId
                && (x.Status == "planned" || x.Status == "active"))
            .ToListAsync(cancellationToken);
        foreach (var assignment in selectableOrgAssignments)
        {
            var terminalStatus = string.Equals(assignment.Status, "planned", StringComparison.OrdinalIgnoreCase)
                ? "canceled"
                : "ended";
            var endsAt = string.Equals(assignment.Status, "planned", StringComparison.OrdinalIgnoreCase)
                ? assignment.EffectiveAt > now ? assignment.EffectiveAt : now
                : now;

            await orgUnitAssignmentService.UpdateStatusAsync(
                tenantId,
                actorUserId,
                record.PersonId,
                assignment.Id,
                new UpdateOrgUnitAssignmentStatusRequest(terminalStatus, endsAt, "Workforce offboarding"),
                cancellationToken);
        }

        await CompleteStepAsync(
            record,
            OffboardingStepKeys.EndOrgAssignments,
            actorUserId,
            now,
            $"Closed {selectableOrgAssignments.Count} planned/active org assignment(s).",
            cancellationToken);

        var activeRoleAssignments = await db.PersonRoleAssignments
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == record.PersonId
                && x.Status == "active"
                && (x.ExpiresAt == null || x.ExpiresAt > now))
            .ToListAsync(cancellationToken);
        foreach (var assignment in activeRoleAssignments)
        {
            await roleTemplateService.UpdatePersonRoleAssignmentStatusAsync(
                tenantId,
                actorUserId,
                record.PersonId,
                assignment.Id,
                new UpdatePersonRoleAssignmentStatusRequest("inactive"),
                cancellationToken);
        }

        await CompleteStepAsync(
            record,
            OffboardingStepKeys.RevokePermissions,
            actorUserId,
            now,
            $"Revoked {activeRoleAssignments.Count} role assignment(s).",
            cancellationToken);

        if (record.DisableLoginRequested)
        {
            var disableResult = await nexArrLoginDisableClient.TryRequestLoginDisableAsync(
                tenantId,
                record.PersonId,
                person.ExternalUserId,
                record.SeparationReason ?? "Workforce offboarding",
                cancellationToken);
            var disableStep = GetStep(record, OffboardingStepKeys.DisableLogin);
            disableStep.Status = disableResult.Outcome switch
            {
                "requested" or "skipped" => OffboardingStepStatuses.Complete,
                _ => OffboardingStepStatuses.Pending,
            };
            disableStep.BlockerDetail = disableResult.Detail;
            disableStep.CompletedAt = disableStep.Status == OffboardingStepStatuses.Complete ? now : null;
            disableStep.CompletedByUserId =
                disableStep.Status == OffboardingStepStatuses.Complete ? actorUserId : null;
            disableStep.UpdatedAt = now;
        }
        else
        {
            await SkipStepAsync(
                record,
                OffboardingStepKeys.DisableLogin,
                actorUserId,
                now,
                "Login disable was not requested for this offboarding.",
                cancellationToken);
        }

        person.EmploymentStatus = record.TargetEmploymentStatus;
        person.UpdatedAt = now;
        await CompleteStepAsync(
            record,
            OffboardingStepKeys.MarkInactive,
            actorUserId,
            now,
            $"Employment status set to {record.TargetEmploymentStatus}.",
            cancellationToken);

        record.Status = OffboardingStatuses.Completed;
        record.CompletedAt = now;
        record.CompletedByUserId = actorUserId;
        record.UpdatedAt = now;

        await CompleteStepAsync(
            record,
            OffboardingStepKeys.PreserveAudit,
            actorUserId,
            now,
            "Offboarding audit trail preserved.",
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "offboarding.execute",
            tenantId,
            actorUserId,
            "person_offboarding_record",
            record.Id.ToString(),
            OffboardingStatuses.Completed,
            reasonCode: record.TargetEmploymentStatus,
            cancellationToken: cancellationToken);

        await maintainarrTechnicianRefSync.TryPublishPersonChangedAsync(
            person,
            "staffarr.person.offboarded",
            cancellationToken);

        return await MapAsync(tenantId, record.Id, cancellationToken);
    }

    private async Task ValidateManagerForReportsAsync(
        Guid tenantId,
        Guid personId,
        Guid managerId,
        CancellationToken cancellationToken)
    {
        if (managerId == personId)
        {
            throw new StlApiException(
                "offboarding.validation",
                "Replacement manager cannot be the person being offboarded.",
                400);
        }

        var manager = await db.People.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == managerId, cancellationToken);
        if (manager is null)
        {
            throw new StlApiException("people.manager_not_found", "Replacement manager was not found.", 404);
        }

        if (!string.Equals(manager.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "offboarding.validation",
                "Replacement manager must be an active person.",
                409);
        }
    }

    private static List<PersonOffboardingStep> BuildInitialSteps(
        Guid tenantId,
        Guid offboardingRecordId,
        OffboardingContext context,
        Guid? newManagerPersonIdForReports)
    {
        var now = DateTimeOffset.UtcNow;
        var reassignBlocked = context.ActiveDirectReportCount > 0 && newManagerPersonIdForReports is null;

        return
        [
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.ReviewAccess,
                1,
                "Review access and open work",
                $"Active role assignments: {context.ActiveRoleAssignmentCount}. Open incidents: {context.OpenIncidentCount}. Active direct reports: {context.ActiveDirectReportCount}.",
                OffboardingStepStatuses.Pending,
                null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.ReassignDirectReports,
                2,
                "Reassign direct reports",
                reassignBlocked
                    ? $"Assign a replacement manager for {context.ActiveDirectReportCount} active direct report(s) before execution."
                    : context.ActiveDirectReportCount == 0
                        ? "No active direct reports require reassignment."
                        : "Replacement manager recorded; execute offboarding to reassign reports.",
                reassignBlocked ? OffboardingStepStatuses.Blocked : OffboardingStepStatuses.Pending,
                reassignBlocked ? "Replacement manager is required." : null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.EndOrgAssignments,
                3,
                "End org assignments",
                $"Close {context.ActiveOrgAssignmentCount} planned/active site/department/team assignment(s).",
                OffboardingStepStatuses.Pending,
                null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.RevokePermissions,
                4,
                "Revoke product permissions",
                $"Deactivate {context.ActiveRoleAssignmentCount} active role assignment(s).",
                OffboardingStepStatuses.Pending,
                null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.DisableLogin,
                5,
                "Disable platform login",
                "Request NexArr to disable login when appropriate.",
                OffboardingStepStatuses.Pending,
                null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.MarkInactive,
                6,
                "Mark person inactive",
                "Set workforce employment status and remove the person from active operations.",
                OffboardingStepStatuses.Pending,
                null,
                now),
            CreateStep(
                tenantId,
                offboardingRecordId,
                OffboardingStepKeys.PreserveAudit,
                7,
                "Preserve audit record",
                "Retain offboarding history in StaffArr personnel timeline and audit events.",
                OffboardingStepStatuses.Pending,
                null,
                now),
        ];
    }

    private static PersonOffboardingStep CreateStep(
        Guid tenantId,
        Guid offboardingRecordId,
        string stepKey,
        int sortOrder,
        string title,
        string detail,
        string status,
        string? blockerDetail,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OffboardingRecordId = offboardingRecordId,
            StepKey = stepKey,
            Title = title,
            Detail = detail,
            Status = status,
            BlockerDetail = blockerDetail,
            SortOrder = sortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

    private static PersonOffboardingStep GetStep(PersonOffboardingRecord record, string stepKey) =>
        record.Steps.First(x => string.Equals(x.StepKey, stepKey, StringComparison.OrdinalIgnoreCase));

    private static Task CompleteStepAsync(
        PersonOffboardingRecord record,
        string stepKey,
        Guid actorUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        CompleteStepAsync(record, stepKey, actorUserId, now, null, cancellationToken);

    private static Task CompleteStepAsync(
        PersonOffboardingRecord record,
        string stepKey,
        Guid actorUserId,
        DateTimeOffset now,
        string? detail,
        CancellationToken cancellationToken)
    {
        var step = GetStep(record, stepKey);
        step.Status = OffboardingStepStatuses.Complete;
        step.CompletedAt = now;
        step.CompletedByUserId = actorUserId;
        if (!string.IsNullOrWhiteSpace(detail))
        {
            step.Detail = detail;
        }

        step.BlockerDetail = null;
        step.UpdatedAt = now;
        return Task.CompletedTask;
    }

    private static Task SkipStepAsync(
        PersonOffboardingRecord record,
        string stepKey,
        Guid actorUserId,
        DateTimeOffset now,
        string detail,
        CancellationToken cancellationToken)
    {
        var step = GetStep(record, stepKey);
        step.Status = OffboardingStepStatuses.Skipped;
        step.Detail = detail;
        step.CompletedAt = now;
        step.CompletedByUserId = actorUserId;
        step.BlockerDetail = null;
        step.UpdatedAt = now;
        return Task.CompletedTask;
    }

    private async Task<OffboardingContext> BuildContextAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var activeDirectReportCount = await db.People.CountAsync(
            x => x.TenantId == tenantId
                && x.ManagerPersonId == personId
                && x.EmploymentStatus == "active",
            cancellationToken);
        var openIncidentCount = await db.PersonnelIncidents.CountAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "open",
            cancellationToken);
        var activeRoleAssignmentCount = await db.PersonRoleAssignments.CountAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && x.Status == "active"
                && (x.ExpiresAt == null || x.ExpiresAt > now),
            cancellationToken);
        var activeOrgAssignmentCount = await db.OrgUnitAssignments.CountAsync(
            x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (x.Status == "planned" || x.Status == "active"),
            cancellationToken);

        return new OffboardingContext(
            activeDirectReportCount,
            openIncidentCount,
            activeRoleAssignmentCount,
            activeOrgAssignmentCount);
    }

    private async Task<PersonOffboardingResponse> MapAsync(
        Guid tenantId,
        Guid offboardingId,
        CancellationToken cancellationToken)
    {
        var record = await db.PersonOffboardingRecords.AsNoTracking()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == offboardingId, cancellationToken);
        if (record is null)
        {
            throw new StlApiException("offboarding.not_found", "Offboarding record was not found.", 404);
        }

        var context = await BuildContextAsync(tenantId, record.PersonId, cancellationToken);
        var steps = record.Steps
            .OrderBy(x => x.SortOrder)
            .Select(x => new PersonOffboardingStepResponse(
                x.StepKey,
                x.Title,
                x.Detail,
                x.Status,
                x.BlockerDetail,
                x.SortOrder,
                x.CompletedAt))
            .ToList();

        return new PersonOffboardingResponse(
            record.Id,
            record.PersonId,
            record.Status,
            record.SeparationDate,
            record.SeparationReason,
            record.TargetEmploymentStatus,
            record.DisableLoginRequested,
            record.NewManagerPersonIdForReports,
            record.StartedAt,
            record.StartedByUserId,
            record.CompletedAt,
            record.CompletedByUserId,
            steps,
            context.ActiveDirectReportCount,
            context.OpenIncidentCount,
            context.ActiveRoleAssignmentCount,
            context.ActiveOrgAssignmentCount);
    }

    private sealed record OffboardingContext(
        int ActiveDirectReportCount,
        int OpenIncidentCount,
        int ActiveRoleAssignmentCount,
        int ActiveOrgAssignmentCount);
}
