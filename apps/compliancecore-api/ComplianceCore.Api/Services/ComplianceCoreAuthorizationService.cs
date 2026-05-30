using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceCoreAuthorizationService
{
    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireComplianceCoreEntitlement(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
        if (!principal.HasProductEntitlement("compliancecore"))
        {
            throw new StlApiException("auth.not_entitled", "Compliance Core entitlement is required.", 403);
        }
    }

    public void RequireVocabularyRead(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Vocabulary read access requires Compliance Core entitlement.",
            403);
    }

    public void RequireVocabularyManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Vocabulary management requires compliancecore.vocabulary.manage scope.",
            403);
    }

    public void RequireKeysRead(ClaimsPrincipal principal)
    {
        RequireVocabularyRead(principal);
    }

    public void RequireKeysManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Key management requires compliancecore.keys.manage scope.",
            403);
    }

    public void RequireRegulatoryRead(ClaimsPrincipal principal)
    {
        RequireVocabularyRead(principal);
    }

    public void RequireRegulatoryManage(ClaimsPrincipal principal)
    {
        RequireVocabularyManage(principal);
    }

    public void RequireRulePacksRead(ClaimsPrincipal principal)
    {
        RequireVocabularyRead(principal);
    }

    public void RequireRulePacksCreate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Rule pack creation requires compliancecore.rulepacks.create scope.",
            403);
    }

    public void RequireRulePacksPublish(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Rule pack publication requires compliancecore.rulepacks.publish scope.",
            403);
    }

    public void RequireRuleEvaluation(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Rule evaluation requires Compliance Core entitlement.",
            403);
    }

    public void RequireFindingsRead(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireOperatorDashboardRead(ClaimsPrincipal principal)
    {
        RequireFindingsRead(principal);
    }

    public void RequireFindingsManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Findings management requires compliancecore.findings.manage scope.",
            403);
    }

    public void RequireWorkflowGateCheck(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireWorkflowGatesManage(ClaimsPrincipal principal)
    {
        RequireRulePacksCreate(principal);
    }

    public void RequireCsvBundleRead(ClaimsPrincipal principal)
    {
        RequireVocabularyRead(principal);
    }

    public void RequireCsvBundleManage(ClaimsPrincipal principal)
    {
        RequirePlatformAdmin(principal, "CSV bundle import and rule-pack publication require server-side platform-admin validation.");
    }

    public void RequirePlatformAdmin(ClaimsPrincipal principal, string message = "Platform administrator access is required.")
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        throw new StlApiException("auth.platform_admin_required", message, 403);
    }

    public void RequireAuditPackageRead(ClaimsPrincipal principal)
    {
        RequireFindingsRead(principal);
    }

    public void RequireAuditPackageExport(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Audit package export requires compliancecore.audit_packages.export scope.",
            403);
    }

    public void RequireSourceIngestionRead(ClaimsPrincipal principal)
    {
        RequireRegulatoryRead(principal);
    }

    public void RequireSourceIngestionManage(ClaimsPrincipal principal)
    {
        RequireRegulatoryManage(principal);
    }

    public void RequireRuleChangeMonitoringRead(ClaimsPrincipal principal)
    {
        RequireRegulatoryRead(principal);
    }

    public void RequireRiskScoringRead(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireRiskScoringEvaluate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Risk scoring evaluation requires compliancecore.risk_scores.evaluate scope.",
            403);
    }

    public void RequireMissingEvidenceWarningRead(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireMissingEvidenceWarningEvaluate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Missing evidence warning evaluation requires compliance admin or reviewer role.",
            403);
    }

    public void RequireControlEffectivenessRead(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireControlEffectivenessEvaluate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Control effectiveness evaluation requires compliance admin or reviewer role.",
            403);
    }

    public void RequireReadinessForecastRead(ClaimsPrincipal principal)
    {
        RequireRuleEvaluation(principal);
    }

    public void RequireReadinessForecastEvaluate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Readiness forecast evaluation requires compliance admin or reviewer role.",
            403);
    }

    public void RequireM12AnalyticsWorkerSettingsManage(ClaimsPrincipal principal)
    {
        RequireAuditDeliveryOrchestrationManage(principal);
    }

    public void RequireFactSourceSyncWorkerSettingsManage(ClaimsPrincipal principal)
    {
        RequireAuditDeliveryOrchestrationManage(principal);
    }

    public void RequireFactSourceSyncHealthRead(ClaimsPrincipal principal)
    {
        RequireOperatorDashboardRead(principal);
    }

    public void RequireAuditDeliveryOrchestrationRead(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Audit delivery orchestration requires compliance admin or reviewer role.",
            403);
    }

    public void RequireAuditDeliveryOrchestrationManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Audit delivery orchestration triggers require tenant admin or compliance admin role.",
            403);
    }

    public void RequireFindingsReportRead(ClaimsPrincipal principal)
    {
        RequireFindingsRead(principal);
    }

    public void RequireFindingsReportExport(ClaimsPrincipal principal)
    {
        RequireAuditPackageExport(principal);
    }

    public void RequireOperatorReportRead(ClaimsPrincipal principal)
    {
        RequireOperatorDashboardRead(principal);
    }

    public void RequireOperatorReportExport(ClaimsPrincipal principal)
    {
        RequireAuditPackageExport(principal);
    }

    public void RequireWaiverRead(ClaimsPrincipal principal)
    {
        RequireFindingsRead(principal);
    }

    public void RequireWaiverManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Compliance waiver management requires tenant admin or compliance admin role.",
            403);
    }

    public void RequireWaiverApprove(ClaimsPrincipal principal)
    {
        RequireComplianceCoreEntitlement(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        if (MatchesRole(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "compliance_admin",
                "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "Compliance waiver approval requires compliance admin or reviewer role.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] candidates) =>
        candidates.Any(candidate => string.Equals(roleKey, candidate, StringComparison.OrdinalIgnoreCase));
}
