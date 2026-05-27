using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class ProductCatalogService(
    NexArrDbContext db,
    PlatformAuthorizationService authorization,
    IPlatformAuditService audit)
{
    public async Task<PagedResult<ProductDetailResponse>> ListAsync(
        ClaimsPrincipal principal,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.ProductCatalog.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDetailResponse(p.ProductKey, p.DisplayName, p.SortOrder, p.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDetailResponse>(items, page, pageSize, total, page * pageSize < total);
    }

    public async Task<ProductDetailResponse> GetAsync(
        ClaimsPrincipal principal,
        string productKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);

        var product = await db.ProductCatalog.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductKey == productKey, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Product was not found.", 404);

        return new ProductDetailResponse(product.ProductKey, product.DisplayName, product.SortOrder, product.IsActive);
    }

    public async Task<ProductDetailResponse> CreateAsync(
        ClaimsPrincipal principal,
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var productKey = request.ProductKey.Trim().ToLowerInvariant();
        if (await db.ProductCatalog.AnyAsync(p => p.ProductKey == productKey, cancellationToken))
        {
            throw new StlApiException("product.key_conflict", "A product with this key already exists.", 409);
        }

        var product = new ProductCatalogItem
        {
            ProductKey = productKey,
            DisplayName = request.DisplayName.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        db.ProductCatalog.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "product.create",
            "product",
            product.ProductKey,
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new ProductDetailResponse(product.ProductKey, product.DisplayName, product.SortOrder, product.IsActive);
    }

    public async Task<ProductDetailResponse> UpdateAsync(
        ClaimsPrincipal principal,
        string productKey,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var product = await db.ProductCatalog.FirstOrDefaultAsync(p => p.ProductKey == productKey, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Product was not found.", 404);

        product.DisplayName = request.DisplayName.Trim();
        product.SortOrder = request.SortOrder;
        product.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "product.update",
            "product",
            product.ProductKey,
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        return new ProductDetailResponse(product.ProductKey, product.DisplayName, product.SortOrder, product.IsActive);
    }
}
