using System.Security.Claims;
using ComplianceCore.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "compliancecore";
    private const bool HasComplianceCoreAccess = true;

    public Task<ComplianceCoreSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        ComplianceCoreAuthorizationService authorization,
        CancellationToken cancellationToken = default)
    {
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return Task.FromResult(new ComplianceCoreSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            HasComplianceCoreAccess,
            launchableProductKeys,
            authorization.CanManageVocabulary(principal),
            authorization.CanExportAuditPackage(principal),
            authorization.CanEvaluateRiskScores(principal),
            authorization.CanEvaluateMissingEvidenceWarnings(principal),
            authorization.CanEvaluateControlEffectiveness(principal),
            authorization.CanEvaluateReadinessForecast(principal),
            authorization.CanReadReports(principal),
            authorization.CanExportReports(principal)));
    }

    public Task<ComplianceCoreMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        ComplianceCoreAuthorizationService authorization,
        CancellationToken cancellationToken = default)
    {
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return Task.FromResult(new ComplianceCoreMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            HasComplianceCoreAccess,
            launchableProductKeys,
            authorization.CanManageVocabulary(principal),
            authorization.CanExportAuditPackage(principal),
            authorization.CanEvaluateRiskScores(principal),
            authorization.CanEvaluateMissingEvidenceWarnings(principal),
            authorization.CanEvaluateControlEffectiveness(principal),
            authorization.CanEvaluateReadinessForecast(principal),
            authorization.CanReadReports(principal),
            authorization.CanExportReports(principal)));
    }
}

