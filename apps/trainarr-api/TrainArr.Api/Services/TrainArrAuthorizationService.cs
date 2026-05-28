using System.Security.Claims;
using TrainArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainArrAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireTrainArrEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("trainarr"))
        {
            throw new StlApiException("auth.not_entitled", "TrainArr entitlement is required.", 403);
        }
    }

    public void RequireAssignmentsRead(ClaimsPrincipal principal, Guid? staffarrPersonId = null)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (MatchesRole(roleKey, "tenant_admin", "trainarr_admin", "trainarr_trainer"))
        {
            return;
        }

        if (staffarrPersonId is Guid requestedPersonId
            && MatchesRole(roleKey, "tenant_member")
            && principal.GetPersonId() == requestedPersonId)
        {
            return;
        }

        if (staffarrPersonId is null && MatchesRole(roleKey, "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training assignment read access requires trainarr.assignments.create scope or self access.",
            403);
    }

    public void RequireAssignmentsCreate(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training assignment create access requires trainarr.assignments.create scope.",
            403);
    }

    public void RequireAssignmentsComplete(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (MatchesRole(roleKey, "tenant_admin", "trainarr_admin", "trainarr_trainer"))
        {
            return;
        }

        if (MatchesRole(roleKey, "tenant_member") && principal.GetPersonId() == staffarrPersonId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training assignment completion requires trainarr.assignments.complete scope or self access.",
            403);
    }

    public void RequireTrainingDefinitionsRead(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin", "trainarr_trainer", "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training definition read access requires TrainArr entitlement.",
            403);
    }

    public void RequireTrainingDefinitionsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training definition management requires trainarr.programs.create scope.",
            403);
    }

    public void RequireIncidentRemediationsRead(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin", "trainarr_trainer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Incident remediation read access requires trainarr.assignments.create scope.",
            403);
    }

    public void RequireTrainingProgramsRead(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin", "trainarr_trainer", "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training program read access requires TrainArr entitlement.",
            403);
    }

    public void RequireTrainingProgramsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training program management requires trainarr.programs.create scope.",
            403);
    }

    public void RequireEvidenceRead(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireAssignmentsRead(principal, staffarrPersonId);
    }

    public void RequireEvidenceUpload(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        if (MatchesRole(roleKey, "tenant_admin", "trainarr_admin", "trainarr_trainer"))
        {
            return;
        }

        if (MatchesRole(roleKey, "tenant_member") && principal.GetPersonId() == staffarrPersonId)
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training evidence upload requires trainarr.evidence.upload scope or self access.",
            403);
    }

    public void RequireEvaluationsRead(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireAssignmentsRead(principal, staffarrPersonId);
    }

    public void RequireEvaluationSubmit(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin", "trainarr_trainer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training evaluation submit requires trainarr.evaluations.signoff scope.",
            403);
    }

    public void RequireSignoffsRead(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireAssignmentsRead(principal, staffarrPersonId);
    }

    public void RequireQualificationChecks(ClaimsPrincipal principal, Guid staffarrPersonId)
    {
        RequireAssignmentsRead(principal, staffarrPersonId);
    }

    public void RequireBatchQualificationChecks(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "trainarr_admin",
                "trainarr_trainer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Batch qualification checks require supervisor or trainer scope.",
            403);
    }

    public void RequireCitationsRead(ClaimsPrincipal principal, string entityType)
    {
        switch (entityType)
        {
            case TrainingCitationEntityTypes.TrainingDefinition:
                RequireTrainingDefinitionsRead(principal);
                return;
            case TrainingCitationEntityTypes.TrainingProgram:
                RequireTrainingProgramsRead(principal);
                return;
            case TrainingCitationEntityTypes.TrainingAssignment:
                RequireAssignmentsRead(principal);
                return;
            default:
                throw new StlApiException(
                    "citations.entity_type_invalid",
                    "Entity type must be training_definition, training_program, or training_assignment.",
                    400);
        }
    }

    public void RequireCitationsManage(ClaimsPrincipal principal, string entityType)
    {
        switch (entityType)
        {
            case TrainingCitationEntityTypes.TrainingDefinition:
                RequireTrainingDefinitionsManage(principal);
                return;
            case TrainingCitationEntityTypes.TrainingProgram:
                RequireTrainingProgramsManage(principal);
                return;
            case TrainingCitationEntityTypes.TrainingAssignment:
                RequireAssignmentsCreate(principal);
                return;
            default:
                throw new StlApiException(
                    "citations.entity_type_invalid",
                    "Entity type must be training_definition, training_program, or training_assignment.",
                    400);
        }
    }

    public void RequireRulePackRequirementsRead(ClaimsPrincipal principal, string entityType)
    {
        switch (entityType)
        {
            case TrainingRulePackRequirementEntityTypes.TrainingDefinition:
                RequireTrainingDefinitionsRead(principal);
                return;
            case TrainingRulePackRequirementEntityTypes.TrainingProgram:
                RequireTrainingProgramsRead(principal);
                return;
            default:
                throw new StlApiException(
                    "rule_pack_requirements.entity_type_invalid",
                    "Entity type must be training_definition or training_program.",
                    400);
        }
    }

    public void RequireRulePackRequirementsManage(ClaimsPrincipal principal, string entityType)
    {
        switch (entityType)
        {
            case TrainingRulePackRequirementEntityTypes.TrainingDefinition:
                RequireTrainingDefinitionsManage(principal);
                return;
            case TrainingRulePackRequirementEntityTypes.TrainingProgram:
                RequireTrainingProgramsManage(principal);
                return;
            default:
                throw new StlApiException(
                    "rule_pack_requirements.entity_type_invalid",
                    "Entity type must be training_definition or training_program.",
                    400);
        }
    }

    public void RequireRulePackImpactRead(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Rule pack impact assessment requires trainarr admin access.",
            403);
    }

    public void RequireQualificationsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Qualification lifecycle management requires trainarr.qualifications.manage scope.",
            403);
    }

    public void RequireNotificationSettingsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Training notification settings require trainarr admin access.",
            403);
    }

    public void RequireRecertificationSettingsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Recertification settings require trainarr admin access.",
            403);
    }

    public void RequireQualificationRecalculationSettingsManage(ClaimsPrincipal principal)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "trainarr_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Qualification recalculation settings require trainarr admin access.",
            403);
    }

    public void RequireSignoffSubmit(ClaimsPrincipal principal, Guid staffarrPersonId, string signoffRole)
    {
        RequireTrainArrEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        var roleKey = principal.GetTenantRoleKey();
        var normalizedRole = signoffRole.Trim().ToLowerInvariant();
        if (string.Equals(normalizedRole, "trainee", StringComparison.Ordinal))
        {
            if (MatchesRole(roleKey, "tenant_member") && principal.GetPersonId() == staffarrPersonId)
            {
                return;
            }

            throw new StlApiException(
                "auth.forbidden",
                "Trainee signoff requires the assignment subject to sign for themselves.",
                403);
        }

        if (string.Equals(normalizedRole, "trainer", StringComparison.Ordinal))
        {
            if (MatchesRole(roleKey, "tenant_admin", "trainarr_admin", "trainarr_trainer"))
            {
                return;
            }

            throw new StlApiException(
                "auth.forbidden",
                "Trainer signoff requires trainarr.evaluations.signoff scope.",
                403);
        }

        throw new StlApiException(
            "auth.forbidden",
            "Signoff role is not recognized for authorization.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
