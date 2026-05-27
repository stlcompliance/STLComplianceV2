using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PartStockService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<PartStockLevelResponse>> ListAsync(
        Guid tenantId,
        Guid? locationId = null,
        Guid? binId = null,
        Guid? partId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.PartStockLevels
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .Where(x => x.TenantId == tenantId);

        if (locationId is not null)
        {
            query = query.Where(x => x.InventoryBin!.InventoryLocationId == locationId);
        }

        if (binId is not null)
        {
            query = query.Where(x => x.InventoryBinId == binId);
        }

        if (partId is not null)
        {
            query = query.Where(x => x.PartId == partId);
        }

        var levels = await query
            .OrderBy(x => x.Part!.DisplayName)
            .ThenBy(x => x.InventoryBin!.Name)
            .ToListAsync(cancellationToken);

        return levels.Select(Map).ToList();
    }

    public async Task<PartStockLevelResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPartStockLevelRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.QuantityOnHand < 0)
        {
            throw new StlApiException(
                "inventory.stock.invalid_quantity",
                "Quantity on hand cannot be negative.",
                400);
        }

        var part = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.PartId,
            cancellationToken);
        if (part is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.BinId,
                cancellationToken);
        if (bin is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inventory.stock.inactive_bin",
                "Stock cannot be assigned to an inactive bin.",
                400);
        }

        var existing = await db.PartStockLevels.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PartId == request.PartId && x.InventoryBinId == request.BinId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var isCreate = existing is null;
        if (existing is null)
        {
            existing = new PartStockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartId = request.PartId,
                InventoryBinId = request.BinId,
                QuantityOnHand = request.QuantityOnHand,
                QuantityReserved = 0,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.PartStockLevels.Add(existing);
        }
        else
        {
            existing.QuantityOnHand = request.QuantityOnHand;
            existing.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            isCreate ? "part_stock.create" : "part_stock.update",
            tenantId,
            actorUserId,
            "part_stock",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        existing.Part = part;
        existing.InventoryBin = bin;
        return Map(existing);
    }

    public async Task<PartStockLevelResponse> IncrementOnHandAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        Guid binId,
        decimal quantityDelta,
        CancellationToken cancellationToken = default)
    {
        if (quantityDelta <= 0)
        {
            throw new StlApiException(
                "inventory.stock.invalid_delta",
                "Stock increment must be greater than zero.",
                400);
        }

        var part = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (part is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == binId,
                cancellationToken);
        if (bin is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inventory.stock.inactive_bin",
                "Stock cannot be assigned to an inactive bin.",
                400);
        }

        var existing = await db.PartStockLevels.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.InventoryBinId == binId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var isCreate = existing is null;
        if (existing is null)
        {
            existing = new PartStockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartId = partId,
                InventoryBinId = binId,
                QuantityOnHand = quantityDelta,
                QuantityReserved = 0,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.PartStockLevels.Add(existing);
        }
        else
        {
            existing.QuantityOnHand += quantityDelta;
            existing.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            isCreate ? "part_stock.create" : "part_stock.increment",
            tenantId,
            actorUserId,
            "part_stock",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        existing.Part = part;
        existing.InventoryBin = bin;
        return Map(existing);
    }

    public async Task<PartStockLevelResponse> DecrementOnHandAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        Guid binId,
        decimal quantityDelta,
        CancellationToken cancellationToken = default)
    {
        if (quantityDelta <= 0)
        {
            throw new StlApiException(
                "inventory.stock.invalid_delta",
                "Stock decrement must be greater than zero.",
                400);
        }

        var part = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (part is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == binId,
                cancellationToken);
        if (bin is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        var existing = await db.PartStockLevels.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.InventoryBinId == binId,
            cancellationToken);

        if (existing is null)
        {
            throw new StlApiException(
                "inventory.stock.insufficient",
                "Insufficient stock on hand for this part in the selected bin.",
                409);
        }

        var available = existing.QuantityOnHand - existing.QuantityReserved;
        if (quantityDelta > available)
        {
            throw new StlApiException(
                "inventory.stock.insufficient",
                "Insufficient available stock for this part in the selected bin.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        existing.QuantityOnHand -= quantityDelta;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_stock.decrement",
            tenantId,
            actorUserId,
            "part_stock",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        existing.Part = part;
        existing.InventoryBin = bin;
        return Map(existing);
    }

    private static PartStockLevelResponse Map(PartStockLevel entity)
    {
        var reserved = entity.QuantityReserved;
        var onHand = entity.QuantityOnHand;
        var available = onHand - reserved;
        if (available < 0)
        {
            available = 0;
        }

        return new PartStockLevelResponse(
            entity.Id,
            entity.PartId,
            entity.Part?.PartKey ?? string.Empty,
            entity.Part?.DisplayName ?? string.Empty,
            entity.InventoryBinId,
            entity.InventoryBin?.BinKey ?? string.Empty,
            entity.InventoryBin?.Name ?? string.Empty,
            entity.InventoryBin?.InventoryLocationId ?? Guid.Empty,
            entity.InventoryBin?.InventoryLocation?.LocationKey ?? string.Empty,
            entity.InventoryBin?.InventoryLocation?.Name ?? string.Empty,
            onHand,
            reserved,
            available,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
