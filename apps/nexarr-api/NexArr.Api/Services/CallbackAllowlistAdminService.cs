using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class CallbackAllowlistAdminService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<IReadOnlyList<CallbackAllowlistEntryResponse>> ListAsync(
        ClaimsPrincipal principal,
        string productKey,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var normalizedKey = productKey.Trim().ToLowerInvariant();
        if (!principal.IsPlatformAdmin())
        {
            var effectiveTenantId = tenantId ?? principal.GetTenantId();
            await authorization.RequireTenantAccessAsync(principal, effectiveTenantId, allowTenantAdmin: true, cancellationToken);
            tenantId = effectiveTenantId;
        }

        var query = db.CallbackAllowlist.AsNoTracking().Where(e => e.ProductKey == normalizedKey);
        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(e => e.TenantId == null || e.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(e => e.TenantId)
            .ThenBy(e => e.UrlPattern)
            .Select(e => ToResponse(e))
            .ToListAsync(cancellationToken);
    }

    public async Task<CallbackAllowlistEntryResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateCallbackAllowlistEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        var productExists = await db.ProductCatalog.AnyAsync(p => p.ProductKey == productKey, cancellationToken);
        if (!productExists)
        {
            throw new StlApiException("product.not_found", "Product was not found.", 404);
        }

        if (request.TenantId is Guid tenantId)
        {
            var tenantExists = await db.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
            if (!tenantExists)
            {
                throw new StlApiException("tenant.not_found", "Tenant was not found.", 404);
            }
        }

        var patternType = NormalizePatternType(request.PatternType);
        ValidatePattern(request.UrlPattern, patternType);

        var now = DateTimeOffset.UtcNow;
        var entry = new ProductCallbackAllowlistEntry
        {
            Id = Guid.NewGuid(),
            ProductKey = productKey,
            TenantId = request.TenantId,
            UrlPattern = request.UrlPattern.Trim(),
            PatternType = patternType,
            IsActive = true,
            CreatedAt = now,
            ModifiedAt = now
        };

        db.CallbackAllowlist.Add(entry);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "launch.callback_allowlist.create",
            "callback_allowlist",
            entry.Id.ToString(),
            "Success",
            tenantId: request.TenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return ToResponse(entry);
    }

    public async Task DeleteAsync(
        ClaimsPrincipal principal,
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var entry = await db.CallbackAllowlist.FirstOrDefaultAsync(e => e.Id == entryId, cancellationToken)
            ?? throw new StlApiException("launch.callback_allowlist_not_found", "Callback allowlist entry was not found.", 404);

        db.CallbackAllowlist.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "launch.callback_allowlist.delete",
            "callback_allowlist",
            entryId.ToString(),
            "Success",
            tenantId: entry.TenantId,
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);
    }

    private static string NormalizePatternType(string patternType)
    {
        var normalized = patternType.Trim().ToLowerInvariant();
        if (normalized is not (CallbackPatternTypes.Origin or CallbackPatternTypes.Prefix))
        {
            throw new StlApiException("launch.invalid_pattern_type", "Pattern type must be 'origin' or 'prefix'.", 400);
        }

        return normalized;
    }

    private static void ValidatePattern(string urlPattern, string patternType)
    {
        if (string.IsNullOrWhiteSpace(urlPattern))
        {
            throw new StlApiException("launch.invalid_pattern", "URL pattern is required.", 400);
        }

        if (string.Equals(patternType, CallbackPatternTypes.Origin, StringComparison.Ordinal))
        {
            if (!Uri.TryCreate(urlPattern.Trim(), UriKind.Absolute, out var uri))
            {
                throw new StlApiException("launch.invalid_pattern", "Origin patterns must be absolute URLs.", 400);
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException("launch.invalid_pattern", "Origin patterns must use http or https.", 400);
            }
        }
    }

    private static CallbackAllowlistEntryResponse ToResponse(ProductCallbackAllowlistEntry entry) =>
        new(entry.Id, entry.ProductKey, entry.TenantId, entry.UrlPattern, entry.PatternType, entry.IsActive, entry.CreatedAt);
}
