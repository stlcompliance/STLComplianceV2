using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public sealed class FixedSuiteProductAccessService(NexArrDbContext db)
{
    private const string ComplianceCoreProductKey = "compliancecore";
    private const string WorkerProductStatus = "worker";

    public IQueryable<ProductCatalogItem> QueryAccessibleProducts(
        bool isPlatformAdmin,
        bool includeWorkers = false)
    {
        var query = db.ProductCatalog
            .AsNoTracking()
            .Where(product => product.IsActive);

        if (!includeWorkers)
        {
            query = query.Where(product => product.ProductStatus != WorkerProductStatus);
        }

        if (!isPlatformAdmin)
        {
            query = query.Where(product => product.ProductKey != ComplianceCoreProductKey);
        }

        return query;
    }

    public async Task<IReadOnlyList<string>> ListAccessibleProductKeysAsync(
        bool isPlatformAdmin,
        bool includeWorkers = false,
        CancellationToken cancellationToken = default) =>
        await QueryAccessibleProducts(isPlatformAdmin, includeWorkers)
            .OrderBy(product => product.SortOrder)
            .Select(product => product.ProductKey)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductCatalogItem>> ListAccessibleProductsAsync(
        bool isPlatformAdmin,
        bool includeWorkers = false,
        CancellationToken cancellationToken = default) =>
        await QueryAccessibleProducts(isPlatformAdmin, includeWorkers)
            .OrderBy(product => product.SortOrder)
            .ToListAsync(cancellationToken);
}
