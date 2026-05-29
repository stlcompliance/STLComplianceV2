using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class HybridDataPlaneService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PagedResult<DataPlaneProfileResponse>> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        string? productKey,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.DataPlaneProfiles.AsNoTracking()
            .Join(db.Tenants.AsNoTracking(), p => p.TenantId, t => t.Id, (p, t) => new { p, t })
            .Join(db.ProductCatalog.AsNoTracking(), x => x.p.ProductKey, c => c.ProductKey, (x, c) => new { x.p, x.t, c });

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.p.TenantId == scopedTenantId);
        }

        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedProductKey = productKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.p.ProductKey == normalizedProductKey);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.t.DisplayName)
            .ThenBy(x => x.c.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapResponse(x.p, x.t.Slug, x.t.DisplayName, x.c.DisplayName))
            .ToListAsync(cancellationToken);

        return new PagedResult<DataPlaneProfileResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<IReadOnlyList<DataPlaneDefaultProfileResponse>> ListEffectiveAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var tenantExists = await db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
        }

        var overrides = await db.DataPlaneProfiles.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .ToDictionaryAsync(p => p.ProductKey, p => p, cancellationToken);

        var products = await db.ProductCatalog.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return products.Select(product =>
        {
            if (overrides.TryGetValue(product.ProductKey, out var profile))
            {
                return new DataPlaneDefaultProfileResponse(
                    tenantId,
                    product.ProductKey,
                    product.DisplayName,
                    profile.DeploymentMode,
                    profile.TrustStatus);
            }

            return new DataPlaneDefaultProfileResponse(
                tenantId,
                product.ProductKey,
                product.DisplayName,
                DataPlaneDeploymentModes.Hosted,
                DataPlaneTrustStatuses.Trusted);
        }).ToList();
    }

    public async Task<DataPlaneProfileResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertDataPlaneProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var actorUserId = principal.GetUserId();
        var now = DateTimeOffset.UtcNow;

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsActive, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Active product was not found.", 404);

        var deploymentMode = NormalizeDeploymentMode(request.DeploymentMode);
        var trustStatus = NormalizeTrustStatus(request.TrustStatus, deploymentMode);
        var endpointUrl = NormalizeEndpointUrl(request.DataEndpointUrl, deploymentMode);

        var entity = await db.DataPlaneProfiles
            .FirstOrDefaultAsync(p => p.TenantId == request.TenantId && p.ProductKey == productKey, cancellationToken);

        if (entity is null)
        {
            entity = new TenantProductDataPlaneProfile
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ProductKey = productKey,
                CreatedAt = now,
            };
            db.DataPlaneProfiles.Add(entity);
        }

        entity.DeploymentMode = deploymentMode;
        entity.DataEndpointUrl = endpointUrl;
        entity.TrustStatus = trustStatus;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.ModifiedByUserId = actorUserId;
        entity.ModifiedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "data_plane.upsert",
            "data_plane_profile",
            entity.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity, tenant.Slug, tenant.DisplayName, product.DisplayName);
    }

    public async Task DeleteAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        string productKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var normalizedProductKey = productKey.Trim().ToLowerInvariant();
        var entity = await db.DataPlaneProfiles
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.ProductKey == normalizedProductKey, cancellationToken);

        if (entity is null)
        {
            return;
        }

        db.DataPlaneProfiles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "data_plane.delete",
            "data_plane_profile",
            entity.Id.ToString(),
            "Success",
            tenantId: tenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);
    }

    private static DataPlaneProfileResponse MapResponse(
        TenantProductDataPlaneProfile profile,
        string tenantSlug,
        string tenantDisplayName,
        string productDisplayName) =>
        new(
            profile.Id,
            profile.TenantId,
            tenantSlug,
            tenantDisplayName,
            profile.ProductKey,
            productDisplayName,
            profile.DeploymentMode,
            profile.DataEndpointUrl,
            profile.TrustStatus,
            profile.Notes,
            profile.ModifiedAt);

    private static string NormalizeDeploymentMode(string raw)
    {
        var mode = raw.Trim().ToLowerInvariant();
        if (!DataPlaneDeploymentModes.All.Contains(mode))
        {
            throw new StlApiException(
                "data_plane.invalid_mode",
                $"Deployment mode must be one of: {string.Join(", ", DataPlaneDeploymentModes.All)}.",
                400);
        }

        return mode;
    }

    private static string NormalizeTrustStatus(string raw, string deploymentMode)
    {
        var status = raw.Trim().ToLowerInvariant();
        if (!DataPlaneTrustStatuses.All.Contains(status))
        {
            throw new StlApiException(
                "data_plane.invalid_trust_status",
                $"Trust status must be one of: {string.Join(", ", DataPlaneTrustStatuses.All)}.",
                400);
        }

        if (deploymentMode == DataPlaneDeploymentModes.CustomerHosted
            && status == DataPlaneTrustStatuses.Trusted)
        {
            throw new StlApiException(
                "data_plane.customer_hosted_untrusted",
                "Customer-hosted data planes must remain untrusted or pending validation until the owning service validates them.",
                400);
        }

        return status;
    }

    private static string? NormalizeEndpointUrl(string? raw, string deploymentMode)
    {
        if (deploymentMode == DataPlaneDeploymentModes.Hosted)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new StlApiException(
                "data_plane.endpoint_required",
                "Data endpoint URL is required for customer-hosted or hybrid deployment modes.",
                400);
        }

        var trimmed = raw.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new StlApiException(
                "data_plane.invalid_endpoint",
                "Data endpoint URL must be an absolute http or https URL.",
                400);
        }

        return trimmed;
    }
}
