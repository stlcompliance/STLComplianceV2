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
    IPlatformAuditService audit,
    PlatformOutboxEnqueueService outboxEnqueue)
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
        var products = await query
            .OrderBy(p => p.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = products.Select(ToResponse).ToList();

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

        return ToResponse(product);
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
            ProductCategory = "operations",
            ProductOwner = "STL Compliance",
            ProductStatus = request.IsActive ? "available" : "disabled",
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CanonicalCallbackPath = "/auth/nexarr/callback",
            ServiceAudience = $"stl:{productKey}:api",
            MarketingUrl = $"https://stlcompliance.com/products/{productKey}",
            DocumentationUrl = $"https://stlcompliance.com/docs/{productKey}",
            SupportUrl = "https://stlcompliance.com/support",
            EnvironmentKey = "local",
            EntitlementDependencyRules = "tenant-product-entitlement-required"
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

        await EnqueueProductEventAsync(
            PlatformOutboxEventKinds.ProductCreated,
            product,
            principal.GetUserId(),
            previousStatus: null,
            cancellationToken);

        return ToResponse(product);
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

        var previousStatus = product.ProductStatus;
        product.DisplayName = request.DisplayName.Trim();
        product.SortOrder = request.SortOrder;
        product.IsActive = request.IsActive;
        product.ProductStatus = request.IsActive ? "available" : "disabled";
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "product.update",
            "product",
            product.ProductKey,
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await EnqueueProductEventAsync(
            PlatformOutboxEventKinds.ProductUpdated,
            product,
            principal.GetUserId(),
            previousStatus,
            cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductDetailResponse> SetActiveAsync(
        ClaimsPrincipal principal,
        string productKey,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var product = await db.ProductCatalog.FirstOrDefaultAsync(p => p.ProductKey == productKey, cancellationToken)
            ?? throw new StlApiException("product.not_found", "Product was not found.", 404);

        var previousStatus = product.ProductStatus;
        product.IsActive = isActive;
        product.ProductStatus = isActive ? "available" : "disabled";
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            isActive ? "product.enable" : "product.disable",
            "product",
            product.ProductKey,
            "Success",
            actorUserId: principal.GetUserId(),
            cancellationToken: cancellationToken);

        await EnqueueProductEventAsync(
            isActive ? PlatformOutboxEventKinds.ProductEnabled : PlatformOutboxEventKinds.ProductDisabled,
            product,
            principal.GetUserId(),
            previousStatus,
            cancellationToken);

        return ToResponse(product);
    }

    private static ProductDetailResponse ToResponse(ProductCatalogItem product) =>
        new(
            product.ProductKey,
            product.DisplayName,
            product.SortOrder,
            product.IsActive,
            product.ProductCategory,
            product.ProductOwner,
            product.ProductStatus,
            product.CanonicalCallbackPath,
            product.ApiBaseUrl,
            product.HealthUrl,
            product.ServiceAudience,
            product.MarketingUrl,
            product.DocumentationUrl,
            product.SupportUrl,
            product.EnvironmentKey,
            product.EntitlementDependencyRules);

    private Task<Guid?> EnqueueProductEventAsync(
        string eventType,
        ProductCatalogItem product,
        Guid actorUserId,
        string? previousStatus,
        CancellationToken cancellationToken) =>
        outboxEnqueue.TryEnqueueAsync(
            eventType,
            "product",
            product.ProductKey,
            $"{product.ProductStatus}:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            new PlatformOutboxPayload(
                PlatformOutboxRules.DefaultSchemaVersion,
                TenantId: null,
                ActorPersonId: actorUserId,
                TargetType: "product",
                TargetId: product.ProductKey,
                Summary: $"Product {eventType}: {product.ProductKey}",
                Metadata: new Dictionary<string, string>
                {
                    ["productCode"] = product.ProductKey,
                    ["displayName"] = product.DisplayName,
                    ["status"] = product.ProductStatus,
                    ["previousStatus"] = previousStatus ?? string.Empty,
                    ["isActive"] = product.IsActive.ToString().ToLowerInvariant(),
                }),
            cancellationToken: cancellationToken);
}
