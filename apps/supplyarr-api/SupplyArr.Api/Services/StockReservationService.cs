using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class StockReservationService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<StockReservationResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? partId = null,
        Guid? binId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.PartStockReservations
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (partId is not null)
        {
            query = query.Where(x => x.PartId == partId);
        }

        if (binId is not null)
        {
            query = query.Where(x => x.InventoryBinId == binId);
        }

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<StockReservationResponse> GetAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, reservationId, cancellationToken);
        return Map(entity);
    }

    public async Task<StockReservationResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateStockReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var quantity = NormalizeQuantity(request.Quantity);
        var reservationKey = NormalizeReservationKey(request.ReservationKey);
        await EnsureUniqueKeyAsync(tenantId, reservationKey, cancellationToken);

        var part = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.PartId,
            cancellationToken)
            ?? throw new StlApiException("parts.not_found", "Part was not found.", 404);

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.BinId,
                cancellationToken)
            ?? throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);

        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inventory.reservation.inactive_bin",
                "Stock cannot be reserved from an inactive bin.",
                400);
        }

        InventoryLocationMovementSafety.EnsureMovementSafe(bin.InventoryLocation);

        var stockLevel = await db.PartStockLevels.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PartId == request.PartId && x.InventoryBinId == request.BinId,
            cancellationToken)
            ?? throw new StlApiException(
                "inventory.reservation.no_stock",
                "No stock level exists for this part in the selected bin.",
                409);

        var available = stockLevel.QuantityOnHand - stockLevel.QuantityReserved;
        if (quantity > available)
        {
            throw new StlApiException(
                "inventory.reservation.insufficient",
                "Insufficient available stock to reserve the requested quantity.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PartStockReservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReservationKey = reservationKey,
            Status = StockReservationStatuses.Active,
            SourceType = NormalizeSourceType(request.SourceType),
            SourceReferenceId = request.SourceReferenceId,
            PartId = request.PartId,
            InventoryBinId = request.BinId,
            PartStockLevelId = stockLevel.Id,
            QuantityReserved = quantity,
            Notes = NormalizeNotes(request.Notes),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        stockLevel.QuantityReserved += quantity;
        stockLevel.UpdatedAt = now;

        db.PartStockReservations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "stock_reservation.create",
            tenantId,
            actorUserId,
            "stock_reservation",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplyArrInventoryReserved,
            "stock_reservation",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Inventory reserved: {part.PartKey} ({quantity})"),
            cancellationToken: cancellationToken);

        entity.Part = part;
        entity.InventoryBin = bin;
        return Map(entity);
    }

    public async Task<StockReservationResponse> ReleaseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid reservationId,
        ReleaseStockReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, reservationId, cancellationToken);
        EnsureActive(entity);

        var stockLevel = await db.PartStockLevels.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == entity.PartStockLevelId,
            cancellationToken)
            ?? throw new StlApiException(
                "inventory.reservation.stock_level_missing",
                "Linked stock level was not found.",
                409);

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == entity.InventoryBinId, cancellationToken)
            ?? throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        InventoryLocationMovementSafety.EnsureMovementSafe(bin.InventoryLocation);

        var now = DateTimeOffset.UtcNow;
        stockLevel.QuantityReserved -= entity.QuantityReserved;
        if (stockLevel.QuantityReserved < 0)
        {
            stockLevel.QuantityReserved = 0;
        }

        stockLevel.UpdatedAt = now;
        entity.Status = StockReservationStatuses.Released;
        entity.ReleasedByUserId = actorUserId;
        entity.ReleasedAt = now;
        entity.ReleaseReason = NormalizeReleaseReason(request.Reason);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "stock_reservation.release",
            tenantId,
            actorUserId,
            "stock_reservation",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<StockReservationResponse> FulfillAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, reservationId, cancellationToken);
        EnsureActive(entity);

        var stockLevel = await db.PartStockLevels
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == entity.PartStockLevelId,
                cancellationToken)
            ?? throw new StlApiException(
                "inventory.reservation.stock_level_missing",
                "Linked stock level was not found.",
                409);

        InventoryLocationMovementSafety.EnsureMovementSafe(stockLevel.InventoryBin?.InventoryLocation);

        if (entity.QuantityReserved > stockLevel.QuantityOnHand)
        {
            throw new StlApiException(
                "inventory.reservation.insufficient_on_hand",
                "Insufficient stock on hand to fulfill this reservation.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        stockLevel.QuantityOnHand -= entity.QuantityReserved;
        stockLevel.QuantityReserved -= entity.QuantityReserved;
        if (stockLevel.QuantityReserved < 0)
        {
            stockLevel.QuantityReserved = 0;
        }

        stockLevel.UpdatedAt = now;
        entity.Status = StockReservationStatuses.Fulfilled;
        entity.FulfilledByUserId = actorUserId;
        entity.FulfilledAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "stock_reservation.fulfill",
            tenantId,
            actorUserId,
            "stock_reservation",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        entity.Part = stockLevel.Part;
        entity.InventoryBin = stockLevel.InventoryBin;
        return Map(entity);
    }

    private async Task<PartStockReservation> LoadAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return await db.PartStockReservations
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == reservationId,
                cancellationToken)
            ?? throw new StlApiException(
                "inventory.reservation.not_found",
                "Stock reservation was not found.",
                404);
    }

    private async Task<PartStockReservation> LoadTrackedAsync(
        Guid tenantId,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return await db.PartStockReservations
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == reservationId,
                cancellationToken)
            ?? throw new StlApiException(
                "inventory.reservation.not_found",
                "Stock reservation was not found.",
                404);
    }

    private async Task EnsureUniqueKeyAsync(
        Guid tenantId,
        string reservationKey,
        CancellationToken cancellationToken)
    {
        var exists = await db.PartStockReservations.AnyAsync(
            x => x.TenantId == tenantId && x.ReservationKey == reservationKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "inventory.reservation.duplicate_key",
                "A stock reservation with this key already exists.",
                409);
        }
    }

    private static void EnsureActive(PartStockReservation entity)
    {
        if (!string.Equals(entity.Status, StockReservationStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "inventory.reservation.not_active",
                "Only active stock reservations can be released or fulfilled.",
                409);
        }
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "inventory.reservation.invalid_quantity",
                "Reservation quantity must be greater than zero.",
                400);
        }

        return quantity;
    }

    private static string NormalizeReservationKey(string reservationKey)
    {
        var normalized = reservationKey?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            throw new StlApiException(
                "inventory.reservation.invalid_key",
                "Reservation key is required.",
                400);
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "inventory.reservation.invalid_key",
                "Reservation key cannot exceed 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSourceType(string? sourceType)
    {
        var normalized = string.IsNullOrWhiteSpace(sourceType) ? "manual" : sourceType.Trim().ToLowerInvariant();
        return normalized.Length > 32 ? normalized[..32] : normalized;
    }

    private static string NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? string.Empty : notes.Trim()[..Math.Min(notes.Trim().Length, 1024)];

    private static string NormalizeReleaseReason(string? reason) =>
        string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim()[..Math.Min(reason.Trim().Length, 512)];

    private static StockReservationResponse Map(PartStockReservation entity) =>
        new(
            entity.Id,
            entity.ReservationKey,
            entity.Status,
            entity.SourceType,
            entity.SourceReferenceId,
            entity.PartId,
            entity.Part?.PartKey ?? string.Empty,
            entity.Part?.DisplayName ?? string.Empty,
            entity.InventoryBinId,
            entity.InventoryBin?.BinKey ?? string.Empty,
            entity.InventoryBin?.Name ?? string.Empty,
            entity.InventoryBin?.InventoryLocationId ?? Guid.Empty,
            entity.InventoryBin?.InventoryLocation?.LocationKey ?? string.Empty,
            entity.InventoryBin?.InventoryLocation?.Name ?? string.Empty,
            entity.PartStockLevelId,
            entity.QuantityReserved,
            entity.Notes,
            entity.CreatedByUserId,
            entity.FulfilledByUserId,
            entity.FulfilledAt,
            entity.ReleasedByUserId,
            entity.ReleasedAt,
            entity.ReleaseReason,
            entity.CreatedAt,
            entity.UpdatedAt);
}
