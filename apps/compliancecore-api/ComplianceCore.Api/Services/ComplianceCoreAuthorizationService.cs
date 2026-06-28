using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ComplianceCoreAuthorizationService
{
    public static class ImportPermissions
    {
        public const string Create = "compliancecore.import.create";
        public const string Read = "compliancecore.import.read";
        public const string Validate = "compliancecore.import.validate";
        public const string Map = "compliancecore.import.map";
        public const string Override = "compliancecore.import.override";
        public const string Commit = "compliancecore.import.commit";
        public const string Reject = "compliancecore.import.reject";
    }

    public static class SimulationPermissions
    {
        public const string Create = "compliancecore.simulation.create";
        public const string Read = "compliancecore.simulation.read";
        public const string Evaluate = "compliancecore.simulation.evaluate";
        public const string TemplateCreate = "compliancecore.simulation.template.create";
        public const string TemplateManage = "compliancecore.simulation.template.manage";
    }

    public void RequireAuthenticated(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new StlApiException("auth.unauthorized", "Unauthorized.", 401);
        }
    }

    public void RequireComplianceCoreRuntimeAccess(ClaimsPrincipal principal)
    {
        RequireAuthenticated(principal);
    }

    public void RequireComplianceCoreRuntimeContext(ClaimsPrincipal principal) =>
        RequireComplianceCoreRuntimeAccess(principal);

    public void RequireVocabularyRead(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
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
            "Vocabulary read access requires Compliance Core read permission.",
            403);
    }

    public void RequireVocabularyManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
            "Rule evaluation requires Compliance Core read permission.",
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
        RequireComplianceCoreRuntimeContext(principal);
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

    public void RequireImportCreate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Import creation requires {ImportPermissions.Create} or delegated Compliance Core admin validation.",
            403);
    }

    public void RequireImportRead(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() ||
            MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer", "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Import read access requires {ImportPermissions.Read}.",
            403);
    }

    public void RequireImportValidate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Import validation requires {ImportPermissions.Validate}.",
            403);
    }

    public void RequireImportMap(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() ||
            MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Import mapping requires {ImportPermissions.Map}.",
            403);
    }

    public void RequireImportOverride(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Force-map override requires {ImportPermissions.Override}.",
            403);
    }

    public void RequireImportCommit(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Import commit requires {ImportPermissions.Commit}.",
            403);
    }

    public void RequireImportReject(ClaimsPrincipal principal)
    {
        RequireImportCommit(principal);
    }

    public void RequireSimulationCreate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() ||
            MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Theoretical situation creation requires {SimulationPermissions.Create}.",
            403);
    }

    public void RequireSimulationRead(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() ||
            MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer", "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Theoretical situation read access requires {SimulationPermissions.Read}.",
            403);
    }

    public void RequireSimulationEvaluate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() ||
            MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Theoretical situation evaluation requires {SimulationPermissions.Evaluate}.",
            403);
    }

    public void RequireSimulationTemplateCreate(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Template creation requires {SimulationPermissions.TemplateCreate}.",
            403);
    }

    public void RequireSimulationTemplateManage(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            $"Template management requires {SimulationPermissions.TemplateManage}.",
            403);
    }

    public void RequirePlatformAdmin(ClaimsPrincipal principal, string message = "Platform administrator access is required.")
    {
        RequireComplianceCoreRuntimeContext(principal);
        if (principal.IsPlatformAdmin())
        {
            return;
        }

        throw new StlApiException("auth.platform_admin_required", message, 403);
    }

    public bool CanManageVocabulary(ClaimsPrincipal principal) =>
        principal.IsPlatformAdmin() || MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin");

    public bool CanExportAuditPackage(ClaimsPrincipal principal) =>
        principal.IsPlatformAdmin() ||
        MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer");

    public bool CanEvaluateRiskScores(ClaimsPrincipal principal) =>
        CanExportAuditPackage(principal);

    public bool CanEvaluateMissingEvidenceWarnings(ClaimsPrincipal principal) =>
        CanEvaluateRiskScores(principal);

    public bool CanEvaluateControlEffectiveness(ClaimsPrincipal principal) =>
        CanEvaluateRiskScores(principal);

    public bool CanEvaluateReadinessForecast(ClaimsPrincipal principal) =>
        CanEvaluateRiskScores(principal);

    public bool CanReadReports(ClaimsPrincipal principal) =>
        principal.IsPlatformAdmin() ||
        MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "compliance_admin", "compliance_reviewer", "tenant_member");

    public bool CanExportReports(ClaimsPrincipal principal) =>
        CanExportAuditPackage(principal);

    public void RequireAuditPackageRead(ClaimsPrincipal principal)
    {
        RequireFindingsRead(principal);
    }

    public void RequireAuditPackageExport(ClaimsPrincipal principal)
    {
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
        RequireComplianceCoreRuntimeContext(principal);
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
