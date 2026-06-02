using MaintainArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api.Services;

public sealed record MaintainArrStaffArrSite(Guid OrgUnitId, string Name);

public sealed class StaffArrSiteReferenceService(
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<IReadOnlyList<MaintainArrStaffArrSite>> ListActiveSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var sites = await client.ListAsync(tenantId, options.Value.ServiceToken, cancellationToken);
        return sites
            .Select(site => new MaintainArrStaffArrSite(site.OrgUnitId, site.Name))
            .ToList();
    }

    public async Task<MaintainArrStaffArrSite?> ResolveOptionalSiteAsync(
        Guid tenantId,
        string? legacySiteAlias,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(legacySiteAlias))
        {
            return null;
        }

        if (!Guid.TryParse(legacySiteAlias.Trim(), out var siteId))
        {
            throw new StlApiException(
                "assets.staffarr_site_invalid",
                "Asset site must be a StaffArr site org unit id.",
                400);
        }

        return await RequireActiveSiteAsync(tenantId, siteId, cancellationToken);
    }

    public async Task<MaintainArrStaffArrSite> RequireActiveSiteAsync(
        Guid tenantId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var site = await client.GetAsync(tenantId, siteId, options.Value.ServiceToken, cancellationToken);
        if (site is null)
        {
            throw new StlApiException(
                "assets.staffarr_site_not_found",
                "Active StaffArr site was not found.",
                404);
        }

        return new MaintainArrStaffArrSite(site.OrgUnitId, site.Name);
    }
}
