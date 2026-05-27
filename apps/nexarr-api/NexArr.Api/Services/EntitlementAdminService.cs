using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class EntitlementAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PagedResult<EntitlementDetailResponse>> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var effectiveTenantId = tenantId ?? principal.GetTenantId();
        await authorization.RequireTenantAccessAsync(principal, effectiveTenantId, allowTenantAdmin: true, cancellationToken);

        var query = db.Entitlements.AsNoTracking().Where(e => e.TenantId == effectiveTenantId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Join(db.ProductCatalog.AsNoTracking(), e => e.ProductKey, p => p.ProductKey, (e, p) => new { e, p })
            .OrderBy(x => x.p.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EntitlementDetailResponse(
                x.e.Id,
                x.e.TenantId,
                x.e.ProductKey,
                x.p.DisplayName,
                x.e.Status,
                x.e.GrantedAt,
                x.e.RevokedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<EntitlementDetailResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<EntitlementDetailResponse> GrantAsync(
        ClaimsPrincipal principal,
        GrantEntitlementRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, request.TenantId, allowTenantAdmin: true, cancellationToken);

        if (!principal.IsPlatformAdmin())
        {
            var jwtTenantId = principal.GetTenantId();
            if (jwtTenantId != request.TenantId)
            {
                throw new StlApiException("auth.tenant_forbidden", "Tenant administrators may only grant entitlements for their active tenant.", 403);
            }
        }

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey && p.IsActive, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Active product was not found.", 404);

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        if (tenant.Status != TenantStatuses.Active)
        {
            throw new StlApiException("tenant.suspended", "Cannot grant entitlements to a suspended tenant.", 403);
        }

        var existing = await db.Entitlements
            .FirstOrDefaultAsync(e => e.TenantId == request.TenantId && e.ProductKey == productKey, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new TenantProductEntitlement
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ProductKey = productKey,
                Status = EntitlementStatuses.Active,
                GrantedAt = now
            };
            db.Entitlements.Add(existing);
        }
        else
        {
            existing.Status = EntitlementStatuses.Active;
            existing.GrantedAt = now;
            existing.RevokedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "entitlement.grant",
            "entitlement",
            existing.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new EntitlementDetailResponse(
            existing.Id,
            existing.TenantId,
            existing.ProductKey,
            product.DisplayName,
            existing.Status,
            existing.GrantedAt,
            existing.RevokedAt);
    }

    public async Task<EntitlementDetailResponse> RevokeAsync(
        ClaimsPrincipal principal,
        Guid entitlementId,
        CancellationToken cancellationToken = default)
    {
        var entitlement = await db.Entitlements
            .FirstOrDefaultAsync(e => e.Id == entitlementId, cancellationToken)
            ?? throw new StlApiException("entitlement.not_found", "Entitlement was not found.", 404);

        await authorization.RequireTenantAccessAsync(principal, entitlement.TenantId, allowTenantAdmin: true, cancellationToken);

        if (entitlement.Status == EntitlementStatuses.Revoked)
        {
            var product = await db.ProductCatalog.AsNoTracking()
                .FirstAsync(p => p.ProductKey == entitlement.ProductKey, cancellationToken);
            return new EntitlementDetailResponse(
                entitlement.Id,
                entitlement.TenantId,
                entitlement.ProductKey,
                product.DisplayName,
                entitlement.Status,
                entitlement.GrantedAt,
                entitlement.RevokedAt);
        }

        entitlement.Status = EntitlementStatuses.Revoked;
        entitlement.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "entitlement.revoke",
            "entitlement",
            entitlement.Id.ToString(),
            "Success",
            tenantId: entitlement.TenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        var displayName = await db.ProductCatalog.AsNoTracking()
            .Where(p => p.ProductKey == entitlement.ProductKey)
            .Select(p => p.DisplayName)
            .FirstAsync(cancellationToken);

        return new EntitlementDetailResponse(
            entitlement.Id,
            entitlement.TenantId,
            entitlement.ProductKey,
            displayName,
            entitlement.Status,
            entitlement.GrantedAt,
            entitlement.RevokedAt);
    }
}
