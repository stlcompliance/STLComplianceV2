using System.Security.Claims;
using SupplyArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = "supplyarr";

    public Task<SupplyArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return Task.FromResult(new SupplyArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            true,
            launchableProductKeys));
    }

    public Task<SupplyArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return Task.FromResult(new SupplyArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            true,
            launchableProductKeys));
    }
}

