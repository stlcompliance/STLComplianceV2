using ComplianceCore.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace ComplianceCore.Api.Services;

public sealed record ComplianceCoreStaffArrSite(Guid OrgUnitId, string Name);

public sealed class StaffArrSiteReferenceService(
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<ComplianceCoreStaffArrSite> RequireActiveSiteAsync(
        Guid tenantId,
        Guid? staffarrSiteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        if (staffarrSiteOrgUnitId is not Guid siteId)
        {
            throw new StlApiException(
                "hazcom.staffarr_site_required",
                "StaffArr site org unit id is required for internal HazCom site references.",
                400);
        }

        var site = await client.GetAsync(tenantId, siteId, options.Value.ServiceToken, cancellationToken);
        if (site is null)
        {
            throw new StlApiException(
                "hazcom.staffarr_site_not_found",
                "Active StaffArr site was not found.",
                404);
        }

        return new ComplianceCoreStaffArrSite(site.OrgUnitId, site.Name);
    }
}
