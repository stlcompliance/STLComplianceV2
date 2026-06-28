using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Data;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api.Services;

public sealed class SupplyArrItemReferenceLookupService(SupplyArrDbContext db)
{
    public async Task<IReadOnlyList<SupplyArrItemReferenceLookupResponse>> ListAsync(
        Guid tenantId,
        string? query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = Normalize(query);
        var parts = db.Parts
            .AsNoTracking()
            .Where(part => part.TenantId == tenantId
                && part.IsStocked
                && part.Status == "active");

        if (normalizedQuery is not null)
        {
            parts = parts.Where(part =>
                part.PartKey.ToLower().Contains(normalizedQuery)
                || part.DisplayName.ToLower().Contains(normalizedQuery)
                || part.CategoryKey.ToLower().Contains(normalizedQuery)
                || part.ManufacturerName.ToLower().Contains(normalizedQuery)
                || part.ManufacturerPartNumber.ToLower().Contains(normalizedQuery));
        }

        return await parts
            .OrderBy(part => part.PartKey)
            .Select(part => new SupplyArrItemReferenceLookupResponse(
                part.TenantId,
                part.PartKey,
                part.DisplayName,
                part.UnitOfMeasure,
                part.CategoryKey,
                part.Status,
                part.RequiresSerialLotTracking,
                part.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }
}
