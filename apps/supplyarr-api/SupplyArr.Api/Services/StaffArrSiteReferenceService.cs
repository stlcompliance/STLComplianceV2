using Microsoft.Extensions.Options;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api.Services;

public sealed record StaffArrResolvedSite(
    Guid OrgUnitId,
    string Name,
    string ResolutionStatus);

public sealed class StaffArrSiteReferenceService(
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public async Task<StaffArrResolvedSite> RequireActiveSiteAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        CancellationToken cancellationToken = default)
    {
        if (siteOrgUnitId is not Guid requestedSiteId)
        {
            throw new StlApiException(
                "staffarr.sites.required",
                "A StaffArr site org unit id is required.",
                400);
        }

        var site = await client.GetAsync(
            tenantId,
            requestedSiteId,
            options.Value.ServiceToken,
            cancellationToken: cancellationToken);

        if (site is null)
        {
            throw new StlApiException(
                "staffarr.sites.not_found",
                "Active StaffArr site was not found.",
                404);
        }

        return new StaffArrResolvedSite(
            site.OrgUnitId,
            site.Name,
            InventoryLocationSiteResolutionStatuses.Active);
    }
}

public static class InventoryLocationMovementSafety
{
    public static void EnsureMovementSafe(InventoryLocation? location)
    {
        if (location is null)
        {
            throw new StlApiException(
                "inventory.locations.not_found",
                "Inventory location was not found.",
                404);
        }

        if (!string.Equals(location.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inventory.locations.inactive",
                "Inventory location must be active for stock movement.",
                409);
        }

        if (location.StaffarrSiteOrgUnitId is null)
        {
            throw new StlApiException(
                "inventory.locations.staffarr_site_required",
                "Inventory location must be assigned to an active StaffArr site before stock can move.",
                409);
        }

        if (!InventoryLocationSiteResolutionStatuses.MovementSafe.Contains(location.StaffarrSiteResolutionStatus))
        {
            throw new StlApiException(
                "inventory.locations.staffarr_site_unresolved",
                "Inventory location StaffArr site must resolve as active before stock can move.",
                409);
        }
    }
}
