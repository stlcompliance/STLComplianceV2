using MaintainArr.Api.Data;
using MaintainArr.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api.Services;

public sealed record MaintainArrStaffArrSite(Guid OrgUnitId, string Name);

public sealed class StaffArrSiteReferenceService(
    MaintainArrDbContext db,
    StaffArrSiteLookupClient client,
    IOptions<StaffArrClientOptions> options)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.ServiceToken);

    public async Task<IReadOnlyList<MaintainArrStaffArrSite>> ListActiveSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return await LoadCachedSitesAsync(tenantId, cancellationToken);
        }

        try
        {
            var sites = await client.ListAsync(tenantId, options.Value.ServiceToken, cancellationToken);
            return sites
                .Select(site => new MaintainArrStaffArrSite(site.OrgUnitId, site.Name))
                .ToList();
        }
        catch (StlApiException ex) when (CanFallbackToCachedSites(ex))
        {
            return await LoadCachedSitesAsync(tenantId, cancellationToken);
        }
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
        if (!IsConfigured)
        {
            return await LoadRequiredCachedSiteAsync(tenantId, siteId, cancellationToken);
        }

        try
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
        catch (StlApiException ex) when (CanFallbackToCachedSites(ex))
        {
            return await LoadRequiredCachedSiteAsync(tenantId, siteId, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<MaintainArrStaffArrSite>> LoadCachedSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var entries = await db.ReferenceCacheEntries.AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.SourceOfTruth == "StaffArr"
                && x.ReferenceKey == "sites"
                && x.IsActive)
            .OrderBy(x => x.Label)
            .ToListAsync(cancellationToken);

        return entries
            .Select(TryMapCachedSite)
            .Where(site => site is not null)
            .Select(site => site!)
            .ToList();
    }

    private async Task<MaintainArrStaffArrSite> LoadRequiredCachedSiteAsync(
        Guid tenantId,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var cachedSite = (await LoadCachedSitesAsync(tenantId, cancellationToken))
            .FirstOrDefault(site => site.OrgUnitId == siteId);
        if (cachedSite is not null)
        {
            return cachedSite;
        }

        if (!IsConfigured)
        {
            throw new StlApiException(
                "staffarr.service_token_missing",
                "StaffArr service token is not configured.",
                500);
        }

        throw new StlApiException(
            "assets.staffarr_site_not_found",
            "Active StaffArr site was not found.",
            404);
    }

    private static MaintainArrStaffArrSite? TryMapCachedSite(MaintainArr.Api.Entities.ReferenceCacheEntry entry)
    {
        if (!TryParseCachedSiteId(entry, out var siteId))
        {
            return null;
        }

        return new MaintainArrStaffArrSite(siteId, entry.Label);
    }

    private static bool TryParseCachedSiteId(MaintainArr.Api.Entities.ReferenceCacheEntry entry, out Guid siteId)
    {
        var preferred = entry.ExternalId?.Trim();
        if (!string.IsNullOrWhiteSpace(preferred) && Guid.TryParse(preferred, out siteId))
        {
            return true;
        }

        var fallback = entry.ExternalKey.Trim();
        if (Guid.TryParse(fallback, out siteId))
        {
            return true;
        }

        siteId = Guid.Empty;
        return false;
    }

    private static bool CanFallbackToCachedSites(StlApiException exception) =>
        string.Equals(exception.Code, "staffarr.service_token_missing", StringComparison.OrdinalIgnoreCase)
        || string.Equals(exception.Code, "staffarr.sites.lookup_failed", StringComparison.OrdinalIgnoreCase);
}
