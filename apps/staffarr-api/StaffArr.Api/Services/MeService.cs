using System.Security.Claims;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Services;

public sealed class MeService(
    StaffArrDbContext db,
    PersonProvisioningService provisioning)
{
    private const string ProductKey = StlProductKeys.StaffArr;

    public async Task<StaffArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var person = await EnsurePersonForPrincipalAsync(principal, cancellationToken);
        return new StaffArrSessionBootstrapResponse(
            userId,
            person.Id,
            tenantId,
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            StaffArrSuiteLaunchCatalog.OrdinaryProductKeys);
    }

    public async Task<StaffArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var person = await EnsurePersonForPrincipalAsync(principal, cancellationToken);
        return new StaffArrMeResponse(
            principal.GetUserId(),
            person.Id,
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            person.PrimaryOrgUnit?.Name,
            person.JobTitle,
            StaffArrSuiteLaunchCatalog.OrdinaryProductKeys);
    }

    private async Task<Entities.StaffPerson> EnsurePersonForPrincipalAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var email = principal.FindFirst("email")?.Value ?? string.Empty;
        var displayName = principal.FindFirst("name")?.Value ?? string.Empty;

        var personFromClaim = principal.GetPersonId();
        var byId = await db.People
            .Include(p => p.PrimaryOrgUnit)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == personFromClaim, cancellationToken);
        if (byId is not null)
        {
            return byId;
        }

        return await provisioning.EnsurePersonAsync(tenantId, userId, email, displayName, cancellationToken);
    }
}

