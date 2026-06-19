using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class WorkforceOnboardingJourneyService(
    StaffArrDbContext db,
    PeopleService peopleService,
    RoleManagementService roleManagementService,
    ReadinessService readinessService,
    TrainarrPersonTrainingHistoryService trainarrHistoryService)
{
    public const string JourneyKey = "new_employee_to_qualified_worker";
    public const string ReadAction = "staffarr.workforce_onboarding_journey.read";

    public async Task<WorkforceOnboardingJourneyResponse> GetForPersonAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        var person = await peopleService.GetByIdAsync(tenantId, personId, cancellationToken);
        var orgPlacementState = await GetOrgPlacementStateAsync(
            tenantId,
            personId,
            person.PrimaryOrgUnitId,
            cancellationToken);
        var permissionProjection = await roleManagementService.ComputeEffectivePermissionProjectionAsync(
            tenantId,
            personId,
            cancellationToken);
        var hasPermissions = permissionProjection.Permissions.Count > 0;

        IReadOnlyList<TrainarrPersonTrainingHistoryEntryItem>? historyItems = null;
        string? trainarrIntegrationNote = null;
        try
        {
            var history = await trainarrHistoryService.GetForPersonAsync(
                tenantId,
                actorUserId,
                personId,
                limit: 100,
                cancellationToken);
            historyItems = history.Items;
        }
        catch (StlApiException ex) when (ex.StatusCode is >= 500 and <= 599)
        {
            trainarrIntegrationNote =
                "TrainArr training history is temporarily unavailable; training steps cannot be evaluated.";
        }

        var readiness = await readinessService.GetPersonReadinessAsync(tenantId, personId, cancellationToken);

        var hasAssignment = HistoryContains(historyItems, "assignment_created");
        var trainingComplete = HistoryContains(historyItems, "assignment_completed")
            || HistoryContains(historyItems, "qualification_issued");
        var qualificationIssued = HistoryContains(historyItems, "qualification_issued");
        var employmentActive = string.Equals(person.EmploymentStatus, "active", StringComparison.OrdinalIgnoreCase);
        var readinessReady = string.Equals(readiness.ReadinessStatus, "ready", StringComparison.OrdinalIgnoreCase);

        var steps = new List<WorkforceOnboardingJourneyStepResponse>
        {
            BuildStep(
                "workforce_profile",
                "Workforce profile",
                "StaffArr person record with active employment (docs/23 step 2).",
                employmentActive ? "complete" : "blocked",
                employmentActive
                    ? null
                    : $"Employment status is {person.EmploymentStatus}; activate the person before operational assignment."),
            BuildStep(
                "org_placement",
                "Org placement",
                "Primary org unit or active site/department/team assignment.",
                !employmentActive
                    ? "blocked"
                    : orgPlacementState.HasActivePlacement || orgPlacementState.HasLegacyPrimarySnapshot
                        ? "complete"
                        : "pending",
                !employmentActive
                    ? "Resolve employment status first."
                    : orgPlacementState.HasActivePlacement || orgPlacementState.HasLegacyPrimarySnapshot
                        ? null
                        : orgPlacementState.HasPlannedPlacement
                            ? "A planned placement exists; activate it before operational assignment."
                            : "Assign a primary org unit or create an org assignment."),
            BuildStep(
                "permissions_assigned",
                "Permissions assigned",
                "Active staff role assignment with effective product permissions (docs/23 step 3).",
                !employmentActive
                    ? "blocked"
                    : hasPermissions
                        ? "complete"
                        : "pending",
                !employmentActive
                    ? "Resolve employment status first."
                    : hasPermissions
                        ? null
                        : "Create an active staff role assignment."),
            BuildTrainarrStep(
                "trainarr_training_assigned",
                "TrainArr programs assigned",
                "TrainArr assigned training programs for role/site/task requirements (docs/23 steps 4–5).",
                historyItems,
                trainarrIntegrationNote,
                hasAssignment,
                "No training assignment recorded yet."),
            BuildTrainarrStep(
                "trainarr_training_completed",
                "Training completed",
                "Evidence, evaluations, and signoffs completed; qualification issued when required (docs/23 steps 6–8).",
                historyItems,
                trainarrIntegrationNote,
                trainingComplete,
                hasAssignment
                    ? "Training is assigned but not completed or qualified yet."
                    : "Assign training before completion can be recorded."),
            BuildStep(
                "staffarr_readiness_ready",
                "StaffArr readiness",
                "Readiness recalculated after TrainArr publication to StaffArr (docs/23 steps 9–10).",
                readinessReady ? "complete" : readiness.Blockers.Count > 0 ? "blocked" : "pending",
                readinessReady
                    ? null
                    : readiness.Blockers.Count > 0
                        ? string.Join("; ", readiness.Blockers.Select(b => b.Message).Take(4))
                        : "Readiness is not ready yet."),
            BuildStep(
                "operational_clearance",
                "Operational clearance",
                "MaintainArr, RoutArr, and SupplyArr may gate work on StaffArr readiness and TrainArr qualifications (docs/23 step 11).",
                readinessReady && (trainingComplete || qualificationIssued)
                    ? "complete"
                    : readiness.Blockers.Count > 0
                        ? "blocked"
                        : "pending",
                readinessReady && (trainingComplete || qualificationIssued)
                    ? null
                    : readiness.Blockers.Count > 0
                        ? string.Join("; ", readiness.Blockers.Select(b => b.Message).Take(4))
                        : "Complete training and readiness before safety-critical assignment."),
        };

        var overallStatus = DeriveOverallStatus(steps);
        var overallSummary = overallStatus switch
        {
            "qualified" => "Worker meets the docs/23 new-employee-to-qualified-worker journey for operational assignment.",
            "blocked" => "Journey is blocked; resolve highlighted steps and readiness blockers.",
            "in_progress" => "Onboarding is in progress; continue role, training, and readiness steps.",
            _ => "Journey has not started; create the workforce profile and org placement.",
        };

        return new WorkforceOnboardingJourneyResponse(
            personId,
            JourneyKey,
            overallStatus,
            overallSummary,
            steps,
            trainarrIntegrationNote);
    }

    private static string DeriveOverallStatus(IReadOnlyList<WorkforceOnboardingJourneyStepResponse> steps)
    {
        if (steps.Any(x => x.StepKey == "operational_clearance" && x.Status == "complete"))
        {
            return "qualified";
        }

        if (steps.Any(x => x.Status is "blocked"))
        {
            return "blocked";
        }

        if (steps.Any(x => x.Status is "complete" or "in_progress"))
        {
            return "in_progress";
        }

        return "not_started";
    }

    private static WorkforceOnboardingJourneyStepResponse BuildTrainarrStep(
        string stepKey,
        string title,
        string detail,
        IReadOnlyList<TrainarrPersonTrainingHistoryEntryItem>? historyItems,
        string? trainarrIntegrationNote,
        bool isComplete,
        string pendingReason)
    {
        if (trainarrIntegrationNote is not null)
        {
            return BuildStep(stepKey, title, detail, "unavailable", trainarrIntegrationNote);
        }

        if (historyItems is null)
        {
            return BuildStep(stepKey, title, detail, "pending", pendingReason);
        }

        return BuildStep(stepKey, title, detail, isComplete ? "complete" : "pending", isComplete ? null : pendingReason);
    }

    private static WorkforceOnboardingJourneyStepResponse BuildStep(
        string stepKey,
        string title,
        string detail,
        string status,
        string? statusReason) =>
        new(stepKey, title, detail, status, statusReason);

    private static bool HistoryContains(
        IReadOnlyList<TrainarrPersonTrainingHistoryEntryItem>? items,
        string eventKind) =>
        items?.Any(x => string.Equals(x.EventKind, eventKind, StringComparison.OrdinalIgnoreCase)) == true;

    private async Task<OrgPlacementState> GetOrgPlacementStateAsync(
        Guid tenantId,
        Guid personId,
        Guid? primaryOrgUnitId,
        CancellationToken cancellationToken)
    {
        var selectableStatuses = await db.OrgUnitAssignments
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.PersonId == personId
                && (x.Status == "planned" || x.Status == "active"))
            .OrderByDescending(x => x.IsPrimary)
            .ThenByDescending(x => x.Status == "active")
            .ThenByDescending(x => x.EffectiveAt)
            .Select(x => x.Status)
            .ToListAsync(cancellationToken);

        if (selectableStatuses.Contains("active"))
        {
            return new OrgPlacementState(
                HasActivePlacement: true,
                HasPlannedPlacement: selectableStatuses.Contains("planned"),
                HasLegacyPrimarySnapshot: primaryOrgUnitId.HasValue);
        }

        if (selectableStatuses.Contains("planned"))
        {
            return new OrgPlacementState(
                HasActivePlacement: false,
                HasPlannedPlacement: true,
                HasLegacyPrimarySnapshot: false);
        }

        return new OrgPlacementState(
            HasActivePlacement: false,
            HasPlannedPlacement: false,
            HasLegacyPrimarySnapshot: primaryOrgUnitId.HasValue);
    }

    private sealed record OrgPlacementState(
        bool HasActivePlacement,
        bool HasPlannedPlacement,
        bool HasLegacyPrimarySnapshot);
}
