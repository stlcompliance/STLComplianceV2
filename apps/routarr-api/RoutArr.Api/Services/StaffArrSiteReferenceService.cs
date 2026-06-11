using Microsoft.Extensions.Options;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace RoutArr.Api.Services;

public sealed record StaffArrResolvedSite(Guid OrgUnitId, string Name);

public sealed class StaffArrSiteReferenceService(
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<StaffArrResolvedSite?> ResolveOptionalSiteAsync(
        Guid tenantId,
        Guid? staffarrSiteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        if (staffarrSiteOrgUnitId is not Guid siteId)
        {
            return null;
        }

        var site = await client.GetAsync(
            tenantId,
            siteId,
            options.Value.ServiceToken,
            cancellationToken: cancellationToken);
        if (site is null)
        {
            throw new StlApiException(
                "staffarr.sites.not_found",
                "Active StaffArr site was not found.",
                404);
        }

        return new StaffArrResolvedSite(site.OrgUnitId, site.Name);
    }
}
