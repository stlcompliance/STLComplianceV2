using System.Security.Claims;
using MaintainArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = StlProductKeys.MaintainArr;

    public Task<MaintainArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new MaintainArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            MaintainArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }

    public Task<MaintainArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new MaintainArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            MaintainArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }
}

