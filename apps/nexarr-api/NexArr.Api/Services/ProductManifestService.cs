using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class ProductManifestService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PagedResult<ProductManifestResponse>> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId = null,
        string? productKey = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var productsQuery = db.ProductCatalog.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedKey = productKey.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(p => p.ProductKey == normalizedKey);
        }

        var total = await productsQuery.CountAsync(cancellationToken);
        var products = await productsQuery
            .OrderBy(p => p.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var productKeys = products.Select(p => p.ProductKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var launchProfiles = await db.LaunchProfiles.AsNoTracking()
            .Where(p => productKeys.Contains(p.ProductKey))
            .ToListAsync(cancellationToken);

        var allowlistQuery = db.CallbackAllowlist.AsNoTracking()
            .Where(a => productKeys.Contains(a.ProductKey));
        if (tenantId is Guid tid)
        {
            allowlistQuery = allowlistQuery.Where(a => a.TenantId == null || a.TenantId == tid);
        }

        var allowlist = await allowlistQuery
            .OrderBy(a => a.ProductKey)
            .ThenBy(a => a.TenantId == null ? 0 : 1)
            .ThenBy(a => a.UrlPattern)
            .ToListAsync(cancellationToken);

        var dataPlaneQuery = db.DataPlaneProfiles.AsNoTracking()
            .Where(p => productKeys.Contains(p.ProductKey));
        if (tenantId is Guid tenantFilter)
        {
            dataPlaneQuery = dataPlaneQuery.Where(p => p.TenantId == tenantFilter);
        }

        var dataPlaneProfiles = await dataPlaneQuery
            .OrderBy(p => p.ProductKey)
            .ThenBy(p => p.TenantId)
            .ToListAsync(cancellationToken);

        var items = products.Select(product =>
        {
            var profile = launchProfiles.FirstOrDefault(
                p => string.Equals(p.ProductKey, product.ProductKey, StringComparison.OrdinalIgnoreCase));
            var launchUrl = profile is null
                ? null
                : ComposeLaunchUrl(profile.BaseUrl, profile.LaunchPath);

            return new ProductManifestResponse(
                product.ProductKey,
                product.DisplayName,
                product.ProductCategory,
                product.ProductOwner,
                product.ProductStatus,
                product.IsActive,
                product.EnvironmentKey,
                product.CanonicalCallbackPath,
                profile?.BaseUrl,
                profile?.LaunchPath,
                launchUrl,
                product.ApiBaseUrl,
                product.HealthUrl,
                ResolveServiceAudience(product),
                product.MarketingUrl,
                product.DocumentationUrl,
                product.SupportUrl,
                product.EntitlementDependencyRules,
                product.ProductDependencyMetadata,
                profile?.ModifiedAt,
                allowlist
                    .Where(a => string.Equals(a.ProductKey, product.ProductKey, StringComparison.OrdinalIgnoreCase))
                    .Select(a => new ProductManifestCallbackAllowlistResponse(
                        a.Id,
                        a.TenantId,
                        a.UrlPattern,
                        a.PatternType,
                        a.IsActive))
                    .ToList(),
                dataPlaneProfiles
                    .Where(p => string.Equals(p.ProductKey, product.ProductKey, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new ProductManifestDataPlaneProfileResponse(
                        p.Id,
                        p.TenantId,
                        p.DeploymentMode,
                        p.TrustStatus,
                        p.DataEndpointUrl))
                    .ToList());
        }).ToList();

        await audit.WriteAsync(
            "product_manifest.read",
            "product_manifest",
            string.IsNullOrWhiteSpace(productKey) ? "all" : productKey.Trim().ToLowerInvariant(),
            "Success",
            tenantId: tenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new PagedResult<ProductManifestResponse>(
            items,
            page,
            pageSize,
            total,
            page * pageSize < total);
    }

    private static string ResolveServiceAudience(ProductCatalogItem product)
    {
        if (!string.IsNullOrWhiteSpace(product.ServiceAudience))
        {
            return product.ServiceAudience;
        }

        return $"stl:{product.ProductKey}:api";
    }

    private static string ComposeLaunchUrl(string baseUrl, string launchPath)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var path = launchPath.StartsWith('/') ? launchPath : $"/{launchPath}";
        return $"{trimmedBase}{path}";
    }
}
