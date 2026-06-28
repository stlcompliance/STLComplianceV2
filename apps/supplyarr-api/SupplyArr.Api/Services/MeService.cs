using System.Security.Claims;
using SupplyArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = StlProductKeys.SupplyArr;

    public Task<SupplyArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SupplyArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            SupplyArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }

    public Task<SupplyArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SupplyArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            SupplyArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }
}

