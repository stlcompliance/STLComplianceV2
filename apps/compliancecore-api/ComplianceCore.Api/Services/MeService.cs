using System.Security.Claims;
using ComplianceCore.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "compliancecore";

    public Task<ComplianceCoreSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new ComplianceCoreSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            principal.HasProductEntitlement(ProductKey),
            entitlements));
    }

    public Task<ComplianceCoreMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new ComplianceCoreMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            principal.HasProductEntitlement(ProductKey),
            entitlements));
    }
}
