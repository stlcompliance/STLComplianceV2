using System.Security.Claims;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Services;

public sealed class MeService(
    StaffArrDbContext db,
    PersonProvisioningService provisioning)
{
    private const string ProductKey = "staffarr";

    public async Task<StaffArrSessionBootstrapResponse> GetSessionBootstrapAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var userId = principal.GetUserId();
        var person = await EnsurePersonForPrincipalAsync(principal, cancellationToken);
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return new StaffArrSessionBootstrapResponse(
            userId,
            person.Id,
            tenantId,
            principal.GetSessionId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            true,
            launchableProductKeys);
    }

    public async Task<StaffArrMeResponse> GetMeAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var person = await EnsurePersonForPrincipalAsync(principal, cancellationToken);
        var launchableProductKeys = principal.GetLaunchableProductKeys();
        return new StaffArrMeResponse(
            principal.GetUserId(),
            person.Id,
            principal.FindFirst("email")?.Value ?? string.Empty,
            principal.FindFirst("name")?.Value ?? string.Empty,
            principal.GetTenantId(),
            principal.GetTenantRoleKey(),
            principal.IsPlatformAdmin(),
            ProductKey,
            true,
            person.PrimaryOrgUnit?.Name,
            person.JobTitle,
            launchableProductKeys);
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

