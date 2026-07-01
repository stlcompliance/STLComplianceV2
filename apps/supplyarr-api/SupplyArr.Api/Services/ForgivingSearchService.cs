using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ForgivingSearchService(SupplyArrDbContext db)
{
    private const int MinQueryLength = 2;
    private const int DefaultLimit = 25;
    private const int MaxLimit = 50;
    private const int CandidateCap = 250;

    public async Task<ForgivingSearchResponse> SearchAsync(
        Guid tenantId,
        string query,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var trimmed = query.Trim();
        if (trimmed.Length < MinQueryLength)
        {
            throw new StlApiException(
                "search.query_too_short",
                $"Search query must be at least {MinQueryLength} characters.",
                400);
        }

        var effectiveLimit = Math.Clamp(limit ?? DefaultLimit, 1, MaxLimit);
        var normalizedQuery = ForgivingSearchNormalizer.Normalize(trimmed);
        var results = new List<ForgivingSearchResultItemResponse>();

        results.AddRange(await SearchSuppliersAsync(tenantId, trimmed, cancellationToken));
        results.AddRange(await SearchPartsAsync(tenantId, trimmed, cancellationToken));
        results.AddRange(await SearchSupplierSkusAsync(tenantId, trimmed, cancellationToken));
        results.AddRange(await SearchPurchaseRequestsAsync(tenantId, trimmed, cancellationToken));
        results.AddRange(await SearchPurchaseOrdersAsync(tenantId, trimmed, cancellationToken));
        results.AddRange(await SearchComplianceDocumentsAsync(tenantId, trimmed, cancellationToken));

        var ranked = results
            .Where(x => x.MatchScore > 0)
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(x => x.Title)
            .Take(effectiveLimit)
            .ToList();

        return new ForgivingSearchResponse(trimmed, normalizedQuery, ranked.Count, ranked);
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchSuppliersAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayName)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return suppliers
            .Select(supplier =>
            {
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    supplier.SupplierKey,
                    supplier.DisplayName,
                    supplier.LegalName,
                    supplier.UnitKind,
                    supplier.TaxIdentifier);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "supplier",
                    supplier.Id,
                    supplier.SupplierKey,
                    supplier.DisplayName,
                    $"{(supplier.UnitKind == "sub_unit" ? "supplier sub-unit" : "supplier identity")} · {supplier.ApprovalStatus}",
                    "/suppliers",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchPartsAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var parts = await db.Parts
            .AsNoTracking()
            .Include(x => x.ManufacturerAliases)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.DisplayName)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return parts
            .Select(part =>
            {
                var aliasHaystack = string.Join(
                    ' ',
                    part.ManufacturerAliases.Select(alias =>
                        $"{alias.ManufacturerName} {alias.ManufacturerPartNumber} {alias.AliasKey}"));
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    part.PartKey,
                    part.DisplayName,
                    part.Description,
                    part.ManufacturerName,
                    part.ManufacturerPartNumber,
                    part.CategoryKey,
                    aliasHaystack);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "part",
                    part.Id,
                    part.PartKey,
                    part.DisplayName,
                    $"{part.ManufacturerPartNumber} · {part.CategoryKey}",
                    "/catalog",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchSupplierSkusAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var links = await db.PartSupplierLinks
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.SupplierPartNumber)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return links
            .Select(link =>
            {
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    link.SupplierPartNumber,
                    link.Part.PartKey,
                    link.Part.DisplayName,
                    link.Part.ManufacturerPartNumber,
                    link.Supplier.SupplierKey,
                    link.Supplier.DisplayName);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "supplier_sku",
                    link.Id,
                    link.SupplierPartNumber,
                    $"{link.Part.PartKey} @ {link.Supplier.DisplayName}",
                    $"Supplier SKU · {link.Supplier.SupplierKey}",
                    "/catalog",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchPurchaseRequestsAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var requests = await db.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return requests
            .Select(request =>
            {
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    request.RequestKey,
                    request.Title,
                    request.Status,
                    request.Supplier?.DisplayName,
                    request.Supplier?.SupplierKey);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "purchase_request",
                    request.Id,
                    request.RequestKey,
                    request.Title,
                    $"PR · {request.Status}",
                    "/purchasing",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchPurchaseOrdersAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var orders = await db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return orders
            .Select(order =>
            {
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    order.OrderKey,
                    order.Title,
                    order.Status,
                    order.Supplier.DisplayName,
                    order.Supplier.SupplierKey);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "purchase_order",
                    order.Id,
                    order.OrderKey,
                    order.Title,
                    $"PO · {order.Status}",
                    "/purchasing",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }

    private async Task<IReadOnlyList<ForgivingSearchResultItemResponse>> SearchComplianceDocumentsAsync(
        Guid tenantId,
        string query,
        CancellationToken cancellationToken)
    {
        var documents = await db.SupplierComplianceDocuments
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        return documents
            .Select(document =>
            {
                var haystack = ForgivingSearchNormalizer.BuildHaystack(
                    document.DocumentKey,
                    document.DocumentTypeKey,
                    document.Title,
                    document.ReviewStatus,
                    document.Supplier.SupplierKey,
                    document.Supplier.DisplayName);
                var score = ForgivingSearchNormalizer.ScoreMatch(haystack, query);
                if (score == 0)
                {
                    return null;
                }

                return new ForgivingSearchResultItemResponse(
                    "compliance_document",
                    document.Id,
                    document.DocumentKey,
                    document.Title,
                    $"{document.Supplier.DisplayName} · {document.DocumentTypeKey}",
                    "/suppliers",
                    score);
            })
            .Where(x => x is not null)
            .Cast<ForgivingSearchResultItemResponse>()
            .ToList();
    }
}

