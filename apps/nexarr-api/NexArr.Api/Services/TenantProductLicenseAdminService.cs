using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class TenantProductLicenseAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<TenantProductLicensesResponse> ListAsync(
        ClaimsPrincipal principal,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var query = db.TenantProductLicenses.AsNoTracking();
        if (tenantId is Guid tid)
        {
            query = query.Where(x => x.TenantId == tid);
        }

        var items = await query
            .Join(db.ProductCatalog.AsNoTracking(), l => l.ProductKey, p => p.ProductKey, (l, p) => new { l, p })
            .OrderBy(x => x.l.TenantId)
            .ThenBy(x => x.p.SortOrder)
            .Select(x => MapResponse(x.l, x.p.DisplayName))
            .ToListAsync(cancellationToken);

        return new TenantProductLicensesResponse(items);
    }

    public async Task<TenantProductLicenseResponse> UpsertAsync(
        ClaimsPrincipal principal,
        UpsertTenantProductLicenseRequest request,
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
            .FirstOrDefaultAsync(p => p.ProductKey == productKey, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Product was not found.", 404);

        var status = NormalizeLicenseStatus(request.Status);
        ValidateLicenseDates(request.ValidFrom, request.ValidTo);

        var entity = await db.TenantProductLicenses
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.ProductKey == productKey,
                cancellationToken);

        if (entity is null)
        {
            entity = new TenantProductLicense
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ProductKey = productKey,
                CreatedAt = now,
            };
            db.TenantProductLicenses.Add(entity);
        }

        entity.Status = status;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;
        entity.ExternalReference = NormalizeExternalReference(request.ExternalReference);
        entity.ModifiedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "tenant_license.upsert",
            "tenant_product_license",
            entity.Id.ToString(),
            "Success",
            tenantId: tenant.Id,
            actorUserId: actorUserId,
            cancellationToken: cancellationToken);

        return MapResponse(entity, product.DisplayName);
    }

    private static string NormalizeLicenseStatus(string status)
    {
        var normalized = status.Trim();
        if (string.Equals(normalized, LicenseStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return LicenseStatuses.Active;
        }

        if (string.Equals(normalized, LicenseStatuses.Expired, StringComparison.OrdinalIgnoreCase))
        {
            return LicenseStatuses.Expired;
        }

        if (string.Equals(normalized, LicenseStatuses.Revoked, StringComparison.OrdinalIgnoreCase))
        {
            return LicenseStatuses.Revoked;
        }

        throw new StlApiException("license.invalid_status", "License status must be Active, Expired, or Revoked.", 400);
    }

    private static void ValidateLicenseDates(DateTimeOffset validFrom, DateTimeOffset? validTo)
    {
        if (validTo is DateTimeOffset to && to <= validFrom)
        {
            throw new StlApiException("license.invalid_dates", "ValidTo must be after ValidFrom.", 400);
        }
    }

    private static string? NormalizeExternalReference(string? externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
        {
            return null;
        }

        var trimmed = externalReference.Trim();
        return trimmed.Length <= 128 ? trimmed : trimmed[..128];
    }

    private static TenantProductLicenseResponse MapResponse(TenantProductLicense entity, string displayName) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.ProductKey,
            displayName,
            entity.Status,
            entity.ValidFrom,
            entity.ValidTo,
            entity.ExternalReference,
            entity.ModifiedAt);
}
