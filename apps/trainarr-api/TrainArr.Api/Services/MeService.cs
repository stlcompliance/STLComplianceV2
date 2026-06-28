using System.Security.Claims;
using TrainArr.Api.Contracts;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace TrainArr.Api.Services;

public sealed class MeService
{
    private const string ProductKey = StlProductKeys.TrainArr;

    public Task<TrainArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TrainArrSessionBootstrapResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.GetTenantId(),
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            TrainArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }

    public Task<TrainArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TrainArrMeResponse(
            principal.GetUserId(),
            principal.GetPersonId(),
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            TrainArrSuiteLaunchCatalog.OrdinaryProductKeys));
    }
}

