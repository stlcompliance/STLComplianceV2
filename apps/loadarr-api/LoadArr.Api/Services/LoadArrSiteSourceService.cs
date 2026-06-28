using LoadArr.Api.Endpoints;
using LoadArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LoadArr.Api.Services;

public interface ILoadArrSiteSourceService
{
    Task<IReadOnlyCollection<LoadArrSiteSourceResponse>> ListSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}

public sealed class LoadArrSiteSourceService(
    StaffArrSiteLookupClient siteClient,
    IOptions<StaffArrClientOptions> options) : ILoadArrSiteSourceService
{
    public async Task<IReadOnlyCollection<LoadArrSiteSourceResponse>> ListSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var sites = await siteClient.ListAsync(
                tenantId,
                options.Value.ServiceToken,
                includeArchived: true,
                cancellationToken);

            return sites
                .OrderBy(site => site.Name, StringComparer.OrdinalIgnoreCase)
                .Select(MapSite)
                .ToArray();
        }
        catch (StlApiException ex)
        {
            throw new StlApiException(
                "loadarr.site_sources.unavailable",
                "LoadArr site references are unavailable because StaffArr site metadata could not be synchronized.",
                ex.StatusCode);
        }
    }

    private static LoadArrSiteSourceResponse MapSite(StaffArrSiteLookupResponse site)
    {
        var active = LoadArrLocationReferenceService.IsActiveStatus(site.Status);
        return new LoadArrSiteSourceResponse(
            LoadArrLocationReferenceService.ResolveSiteReference(site),
            site.Name,
            site.Status,
            active,
            active
                ? "StaffArr owns this site reference. Location utilization remains unavailable until the warehouse read model is ready."
                : $"StaffArr marks this site as {site.Status}. LoadArr keeps it visible only as reference metadata until warehouse read models are ready.");
    }
}
