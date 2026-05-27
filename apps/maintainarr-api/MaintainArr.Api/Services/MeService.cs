using System.Security.Claims;
using MaintainArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "maintainarr";

    public Task<MaintainArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new MaintainArrSessionBootstrapResponse(
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

    public Task<MaintainArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new MaintainArrMeResponse(
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
