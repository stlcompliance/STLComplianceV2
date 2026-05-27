using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class TenantAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PagedResult<TenantDetailResponse>> ListAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Tenant> query = db.Tenants.AsNoTracking();

        if (!principal.IsPlatformAdmin())
        {
            var tenantId = principal.GetTenantId();
            await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);
            query = query.Where(t => t.Id == tenantId);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantDetailResponse(t.Id, t.Slug, t.DisplayName, t.Status, t.CreatedAt, t.ModifiedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantDetailResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<TenantDetailResponse> GetAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireTenantAccessAsync(principal, tenantId, allowTenantAdmin: true, cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        return new TenantDetailResponse(tenant.Id, tenant.Slug, tenant.DisplayName, tenant.Status, tenant.CreatedAt, tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var slug = NormalizeSlug(request.Slug);
        if (await db.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            throw new StlApiException("tenant.slug_conflict", "A tenant with this slug already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            DisplayName = request.DisplayName.Trim(),
            Status = TenantStatuses.Active,
            CreatedAt = now,
            ModifiedAt = now
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.create",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(tenant.Id, tenant.Slug, tenant.DisplayName, tenant.Status, tenant.CreatedAt, tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> UpdateAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        UpdateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        tenant.DisplayName = request.DisplayName.Trim();
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.update",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(tenant.Id, tenant.Slug, tenant.DisplayName, tenant.Status, tenant.CreatedAt, tenant.ModifiedAt);
    }

    public async Task<TenantDetailResponse> UpdateStatusAsync(
        ClaimsPrincipal principal,
        Guid tenantId,
        UpdateTenantStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        if (request.Status is not TenantStatuses.Active and not TenantStatuses.Suspended)
        {
            throw new StlApiException("tenant.invalid_status", "Status must be Active or Suspended.", 400);
        }

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);

        tenant.Status = request.Status;
        tenant.ModifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant.status_change",
            "tenant",
            tenant.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new TenantDetailResponse(tenant.Id, tenant.Slug, tenant.DisplayName, tenant.Status, tenant.CreatedAt, tenant.ModifiedAt);
    }

    private static string NormalizeSlug(string slug) =>
        slug.Trim().ToLowerInvariant().Replace(' ', '-');
}
