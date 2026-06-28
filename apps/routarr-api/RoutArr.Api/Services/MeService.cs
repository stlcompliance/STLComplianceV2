using System.Security.Claims;
using RoutArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace RoutArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = StlProductKeys.RoutArr;

    public Task<RoutArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RoutArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            RoutArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }

    public Task<RoutArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RoutArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            RoutArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }
}

