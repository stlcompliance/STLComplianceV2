using LoadArr.Api.Endpoints;
using LoadArr.Api.Options;
using Microsoft.Extensions.Options;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace LoadArr.Api.Services;

public interface ILoadArrLocationReferenceService
{
    Task<IReadOnlyCollection<LoadArrLocationResponse>> ListLocationsAsync(
        Guid tenantId,
        string? siteReference,
        string? locationType,
        bool? active,
        CancellationToken cancellationToken);

    Task<LoadArrLocationResponse?> GetLocationAsync(
        Guid tenantId,
        string id,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<LoadArrLocationTreeNodeResponse>> GetLocationTreeAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}

public sealed class LoadArrLocationReferenceService(
    StaffArrLocationLookupClient locationClient,
    StaffArrSiteLookupClient siteClient,
    IOptions<StaffArrClientOptions> options) : ILoadArrLocationReferenceService
{
    public async Task<IReadOnlyCollection<LoadArrLocationResponse>> ListLocationsAsync(
        Guid tenantId,
        string? siteReference,
        string? locationType,
        bool? active,
        CancellationToken cancellationToken)
    {
        var normalizedSiteReference = Normalize(siteReference);
        var sites = await ListSitesAsync(tenantId, cancellationToken);
        var siteLookup = BuildSiteLookup(sites);
        var siteOrgUnitId = ResolveSiteOrgUnitId(sites, normalizedSiteReference);

        if (normalizedSiteReference is not null && siteOrgUnitId is null)
        {
            return [];
        }

        var locations = await ListStaffArrLocationsAsync(
            tenantId,
            siteOrgUnitId,
            Normalize(locationType),
            includeArchived: active is not true,
            cancellationToken);

        return locations
            .Where(location => MatchesActive(location, active))
            .Select(location => MapLocation(location, siteLookup))
            .OrderBy(location => location.StaffarrSiteNameSnapshot, StringComparer.OrdinalIgnoreCase)
            .ThenBy(location => location.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<LoadArrLocationResponse?> GetLocationAsync(
        Guid tenantId,
        string id,
        CancellationToken cancellationToken)
    {
        var normalizedId = Normalize(id);
        if (normalizedId is null)
        {
            return null;
        }

        var sites = await ListSitesAsync(tenantId, cancellationToken);
        var siteLookup = BuildSiteLookup(sites);
        var locations = await ListStaffArrLocationsAsync(
            tenantId,
            siteOrgUnitId: null,
            locationType: null,
            includeArchived: true,
            cancellationToken);

        var location = locations.FirstOrDefault(candidate =>
            string.Equals(candidate.LocationNumber, normalizedId, StringComparison.OrdinalIgnoreCase));

        return location is null ? null : MapLocation(location, siteLookup);
    }

    public async Task<IReadOnlyCollection<LoadArrLocationTreeNodeResponse>> GetLocationTreeAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var sites = await ListSitesAsync(tenantId, cancellationToken);
        var siteLookup = BuildSiteLookup(sites);
        var locations = await ListStaffArrLocationsAsync(
            tenantId,
            siteOrgUnitId: null,
            locationType: null,
            includeArchived: true,
            cancellationToken);

        var resolved = locations
            .Select(location => ResolveLocation(location, siteLookup))
            .OrderBy(location => location.SiteName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(location => location.Location.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var locationIds = resolved.Select(location => location.InternalLocationId).ToHashSet();
        var childrenByParent = resolved.ToLookup(location => location.ParentLocationId);

        return resolved
            .GroupBy(location => location.SiteReference, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.First().SiteName, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var roots = group
                    .Where(location => location.ParentLocationId is null || !locationIds.Contains(location.ParentLocationId.Value))
                    .OrderBy(location => location.Location.Path, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(location => location.Location.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(location => BuildTreeNode(location, childrenByParent))
                    .ToArray();

                return new LoadArrLocationTreeNodeResponse(
                    $"site:{group.Key}",
                    group.First().SiteName,
                    "site",
                    null,
                    roots);
            })
            .ToArray();
    }

    private async Task<IReadOnlyList<StaffArrSiteLookupResponse>> ListSitesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await siteClient.ListAsync(
                tenantId,
                options.Value.ServiceToken,
                includeArchived: true,
                cancellationToken);
        }
        catch (StlApiException ex)
        {
            throw TranslateLookupFailure(ex, "LoadArr location references are unavailable because StaffArr site metadata could not be synchronized.");
        }
    }

    private async Task<IReadOnlyList<StaffArrLocationLookupResponse>> ListStaffArrLocationsAsync(
        Guid tenantId,
        Guid? siteOrgUnitId,
        string? locationType,
        bool includeArchived,
        CancellationToken cancellationToken)
    {
        try
        {
            return await locationClient.ListAsync(
                tenantId,
                options.Value.ServiceToken,
                siteOrgUnitId: siteOrgUnitId,
                type: locationType,
                includeArchived: includeArchived,
                cancellationToken: cancellationToken);
        }
        catch (StlApiException ex)
        {
            throw TranslateLookupFailure(ex, "LoadArr location references are unavailable because StaffArr location synchronization is not available right now.");
        }
    }

    private static StlApiException TranslateLookupFailure(StlApiException ex, string message) =>
        new(
            "loadarr.location_references.unavailable",
            message,
            ex.StatusCode);

    private static Dictionary<Guid, SiteReferenceInfo> BuildSiteLookup(
        IReadOnlyCollection<StaffArrSiteLookupResponse> sites) =>
        sites.ToDictionary(
            site => site.OrgUnitId,
            site => new SiteReferenceInfo(
                site.Name,
                ResolveSiteReference(site)),
            EqualityComparer<Guid>.Default);

    private static Guid? ResolveSiteOrgUnitId(
        IEnumerable<StaffArrSiteLookupResponse> sites,
        string? siteReference)
    {
        if (siteReference is null)
        {
            return null;
        }

        var match = sites.FirstOrDefault(site =>
            string.Equals(ResolveSiteReference(site), siteReference, StringComparison.OrdinalIgnoreCase));

        return match?.OrgUnitId;
    }

    internal static string ResolveSiteReference(StaffArrSiteLookupResponse site)
    {
        var code = Normalize(site.Code);
        return code ?? Slugify(site.Name);
    }

    private static ResolvedLocation ResolveLocation(
        StaffArrLocationLookupResponse location,
        IReadOnlyDictionary<Guid, SiteReferenceInfo> siteLookup)
    {
        siteLookup.TryGetValue(location.SiteOrgUnitId ?? Guid.Empty, out var site);
        var mapped = new LoadArrLocationResponse(
            location.LocationNumber,
            site?.Name ?? location.SiteNameSnapshot,
            site?.Reference ?? Slugify(location.SiteNameSnapshot),
            location.Name,
            location.LocationType,
            BuildPath(site?.Name ?? location.SiteNameSnapshot, location.ParentPathSnapshot, location.Name),
            IsActiveStatus(location.Status),
            BuildComplianceRestrictions(location),
            0,
            string.IsNullOrWhiteSpace(location.Description)
                ? "StaffArr owns this location reference. Capacity and inventory utilization remain unavailable until the warehouse read model is ready."
                : location.Description);

        return new ResolvedLocation(
            location.LocationId,
            location.ParentLocationId,
            site?.Reference ?? Slugify(location.SiteNameSnapshot),
            site?.Name ?? location.SiteNameSnapshot,
            mapped);
    }

    private static LoadArrLocationResponse MapLocation(
        StaffArrLocationLookupResponse location,
        IReadOnlyDictionary<Guid, SiteReferenceInfo> siteLookup) =>
        ResolveLocation(location, siteLookup).Location;

    private static LoadArrLocationTreeNodeResponse BuildTreeNode(
        ResolvedLocation location,
        ILookup<Guid?, ResolvedLocation> childrenByParent)
    {
        return new LoadArrLocationTreeNodeResponse(
            $"location:{location.Location.Id}",
            location.Location.Name,
            location.Location.LocationType,
            location.Location.Id,
            childrenByParent[location.InternalLocationId]
                .Select(child => BuildTreeNode(child, childrenByParent))
                .ToArray());
    }

    private static IReadOnlyCollection<string> BuildComplianceRestrictions(StaffArrLocationLookupResponse location)
    {
        var restrictions = new List<string>();
        if (!string.IsNullOrWhiteSpace(location.AllowedProductUsage)
            && !string.Equals(location.AllowedProductUsage, "all", StringComparison.OrdinalIgnoreCase))
        {
            restrictions.Add(location.AllowedProductUsage);
        }

        if (!IsActiveStatus(location.Status))
        {
            restrictions.Add(location.Status);
        }

        return restrictions;
    }

    private static bool MatchesActive(StaffArrLocationLookupResponse location, bool? active) =>
        active switch
        {
            true => IsActiveStatus(location.Status),
            false => !IsActiveStatus(location.Status),
            _ => true
        };

    internal static bool IsActiveStatus(string? status) =>
        string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);

    private static string BuildPath(string siteName, string? parentPathSnapshot, string locationName)
    {
        var segments = new List<string>();
        AddSegments(segments, siteName);
        AddSegments(segments, parentPathSnapshot);
        AddSegments(segments, locationName);
        return string.Join(" / ", CollapseDuplicateSegments(segments));
    }

    private static void AddSegments(List<string> segments, string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return;
        }

        foreach (var segment in rawValue.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(segment))
            {
                segments.Add(segment);
            }
        }
    }

    private static IEnumerable<string> CollapseDuplicateSegments(IEnumerable<string> segments)
    {
        string? previous = null;
        foreach (var segment in segments)
        {
            if (string.Equals(previous, segment, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            previous = segment;
            yield return segment;
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Slugify(string value)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[normalized.Length];
        var index = 0;
        var previousDash = false;
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                buffer[index++] = char.ToLowerInvariant(character);
                previousDash = false;
                continue;
            }

            if (!previousDash)
            {
                buffer[index++] = '-';
                previousDash = true;
            }
        }

        var slug = new string(buffer[..index]).Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "site" : slug;
    }

    private sealed record SiteReferenceInfo(
        string Name,
        string Reference);

    private sealed record ResolvedLocation(
        Guid InternalLocationId,
        Guid? ParentLocationId,
        string SiteReference,
        string SiteName,
        LoadArrLocationResponse Location);
}
