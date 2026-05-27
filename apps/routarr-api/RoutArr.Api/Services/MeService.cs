using System.Security.Claims;
using RoutArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "routarr";

    public Task<RoutArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new RoutArrSessionBootstrapResponse(
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

    public Task<RoutArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var entitlements = principal.GetEntitlements();
        return Task.FromResult(new RoutArrMeResponse(
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
