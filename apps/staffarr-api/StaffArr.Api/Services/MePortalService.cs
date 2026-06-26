using System.Security.Claims;
using StaffArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.Operations;

namespace StaffArr.Api.Services;

public sealed class MePortalService(
    MeService meService,
    PersonLookupService personLookupService,
    ReadinessService readinessService,
    CertificationService certificationService,
    PermissionProjectionService permissionProjectionService,
    WorkforceOnboardingJourneyService onboardingJourneyService,
    ManagerHierarchyService managerHierarchyService)
{
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(30);

    public async Task<MePortalSummaryResponse> GetSummaryAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var personId = principal.GetPersonId();
        var actorUserId = principal.GetUserId();

        var session = await meService.GetMeAsync(principal, cancellationToken);
        var profile = await personLookupService.GetByPersonIdAsync(tenantId, personId, cancellationToken);
        var readiness = await readinessService.GetPersonReadinessAsync(tenantId, personId, cancellationToken);
        var certifications = await certificationService.ListPersonCertificationsAsync(
            tenantId,
            personId,
            cancellationToken);
        var permissions = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
            tenantId,
            personId,
            cancellationToken);
        var subordinates = await managerHierarchyService.GetSubordinatesAsync(
            tenantId,
            personId,
            includeIndirect: false,
            limit: 5,
            cancellationToken);

        MePortalOnboardingSummaryResponse? onboarding = null;
        try
        {
            var journey = await onboardingJourneyService.GetForPersonAsync(
                tenantId,
                actorUserId,
                personId,
                cancellationToken);
            var completed = journey.Steps.Count(x => x.Status is "complete");
            var blocked = journey.Steps.Count(x => x.Status is "blocked");
            onboarding = new MePortalOnboardingSummaryResponse(
                journey.OverallStatus,
                completed,
                journey.Steps.Count,
                blocked);
        }
        catch (StlApiException)
        {
            onboarding = null;
        }

        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.Add(ExpiringSoonWindow);
        var activeCerts = certifications
            .Where(x => x.EffectiveStatus is "active" or "valid")
            .ToList();
        var expiringSoon = activeCerts.Count(x =>
            x.ExpiresAt is DateTimeOffset expiresAt
            && expiresAt > now
            && expiresAt <= expiringThreshold);
        var missingCount = readiness.Requirements.Count(x =>
            x.RequirementStatus is "missing" or "expired" or "revoked");

        return new MePortalSummaryResponse(
            session,
            profile,
            new MePortalReadinessSummaryResponse(
                readiness.ReadinessStatus,
                readiness.ReadinessBasis,
                readiness.Blockers.Select(x => x.Message).ToList()),
            new MePortalCertificationSummaryResponse(
                activeCerts.Count,
                expiringSoon,
                missingCount,
                certifications
                    .OrderByDescending(x => x.GrantedAt)
                    .Take(6)
                    .ToList()),
            BuildPermissionSummary(permissions),
            onboarding,
            subordinates.Count,
            subordinates,
            BuildProductAccess(principal));
    }

    private static MePortalPermissionSummaryResponse BuildPermissionSummary(
        EffectivePermissionProjectionResponse projection)
    {
        var summaries = projection.Permissions
            .OrderBy(x => x.PermissionName, StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .Select(permission =>
            {
                var scope = string.IsNullOrWhiteSpace(permission.ScopeValue)
                    ? permission.ScopeType
                    : $"{permission.ScopeType}:{permission.ScopeValue}";
                return $"{permission.PermissionName} ({permission.PermissionKey}, {scope})";
            })
            .ToList();

        return new MePortalPermissionSummaryResponse(projection.Permissions.Count, summaries);
    }

    private static IReadOnlyList<string> BuildProductAccess(ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return StlMasterWorkflowCatalog.Sections
                .Where(section => section.Kind is StlMasterWorkflowSectionKind.ProductWorkflow)
                .Select(section => section.PrimaryOwnerProductKey)
                .OfType<string>()
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        return StlMasterWorkflowCatalog.Sections
            .Where(section => section.Kind is StlMasterWorkflowSectionKind.ProductWorkflow)
            .Select(section => section.PrimaryOwnerProductKey)
            .OfType<string>()
            .Where(productKey => !string.Equals(productKey, StlProductKeys.ComplianceCore, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
