using System.Security.Claims;
using StaffArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "staffarr";

    public StaffArrSessionBootstrapResponse GetSessionBootstrap(ClaimsPrincipal principal)
    {
        var entitlements = principal.GetEntitlements();
        return new StaffArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            ProductKey,
            principal.HasProductEntitlement(ProductKey),
            entitlements);
    }

    public StaffArrMeResponse GetMe(ClaimsPrincipal principal)
    {
        var entitlements = principal.GetEntitlements();
        return new StaffArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            ProductKey,
            principal.HasProductEntitlement(ProductKey),
            entitlements);
    }
}
