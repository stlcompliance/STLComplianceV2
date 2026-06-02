using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class WmsMovementService(
    SupplyArrDbContext db,
    RoutArrShipmentClient routArrShipmentClient,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<WmsStockLedgerEntryResponse>> ListLedgerAsync(
        Guid tenantId,
        Guid? partId = null,
        Guid? binId = null,
        Guid? locationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = LedgerQuery(tenantId);

        if (partId is not null)
        {
            query = query.Where(x => x.PartId == partId);
        }

        if (binId is not null)
        {
            query = query.Where(x => x.InventoryBinId == binId);
        }

        if (locationId is not null)
        {
            query = query.Where(x => x.InventoryBin!.InventoryLocationId == locationId);
        }

        var entries = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(500)
            .ToListAsync(cancellationToken);

        return entries.Select(MapLedger).ToList();
    }

    public async Task<WmsMovementResponse> TransferAsync(
        Guid tenantId,
        Guid actorUserId,
        TransferStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var replay = await ReplayAsync(tenantId, idempotencyKey, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        if (request.FromBinId == request.ToBinId)
        {
            throw new StlApiException("wms.transfer.same_bin", "Transfer bins must be different.", 400);
        }

        var quantity = NormalizeQuantity(request.Quantity);
        var fromStock = await LoadStockAsync(tenantId, request.PartId, request.FromBinId, cancellationToken);
        var toStock = await LoadOrCreateStockAsync(tenantId, request.PartId, request.ToBinId, cancellationToken);
        EnsureMovementSafe(fromStock.InventoryBin);
        EnsureMovementSafe(toStock.InventoryBin);
        EnsureAvailable(fromStock, quantity);

        var now = DateTimeOffset.UtcNow;
        var movementGroupId = Guid.NewGuid();
        fromStock.QuantityOnHand -= quantity;
        fromStock.UpdatedAt = now;
        toStock.QuantityOnHand += quantity;
        toStock.UpdatedAt = now;

        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.TransferOut,
            request.PartId,
            request.FromBinId,
            request.ToBinId,
            -quantity,
            0,
            fromStock,
            actorUserId,
            "manual",
            null,
            request.Notes,
            now);
        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.TransferIn,
            request.PartId,
            request.ToBinId,
            request.FromBinId,
            quantity,
            0,
            toStock,
            actorUserId,
            "manual",
            null,
            request.Notes,
            now);

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("wms.transfer", tenantId, actorUserId, movementGroupId, cancellationToken);
        return (await ReplayAsync(tenantId, idempotencyKey, cancellationToken))!;
    }

    public async Task<WmsMovementResponse> ReserveAsync(
        Guid tenantId,
        Guid actorUserId,
        ReserveStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var replay = await ReplayAsync(tenantId, idempotencyKey, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var quantity = NormalizeQuantity(request.Quantity);
        var stock = await LoadStockAsync(tenantId, request.PartId, request.BinId, cancellationToken);
        EnsureMovementSafe(stock.InventoryBin);
        EnsureAvailable(stock, quantity);

        var now = DateTimeOffset.UtcNow;
        var movementGroupId = Guid.NewGuid();
        stock.QuantityReserved += quantity;
        stock.UpdatedAt = now;
        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.Reserve,
            request.PartId,
            request.BinId,
            null,
            0,
            quantity,
            stock,
            actorUserId,
            NormalizeSourceType(request.SourceType),
            request.SourceReferenceId,
            request.Notes,
            now);

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("wms.reserve", tenantId, actorUserId, movementGroupId, cancellationToken);
        return (await ReplayAsync(tenantId, idempotencyKey, cancellationToken))!;
    }

    public async Task<WmsMovementResponse> PickAsync(
        Guid tenantId,
        Guid actorUserId,
        PickStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var replay = await ReplayAsync(tenantId, idempotencyKey, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var quantity = NormalizeQuantity(request.Quantity);
        var stock = await LoadStockAsync(tenantId, request.PartId, request.BinId, cancellationToken);
        EnsureMovementSafe(stock.InventoryBin);
        if (stock.QuantityReserved < quantity)
        {
            throw new StlApiException("wms.pick.not_reserved", "Picked stock must be reserved first.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var movementGroupId = Guid.NewGuid();
        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.Pick,
            request.PartId,
            request.BinId,
            null,
            0,
            0,
            stock,
            actorUserId,
            "manual",
            request.OutboundShipmentLineId,
            request.Notes,
            now);

        if (request.OutboundShipmentLineId is Guid lineId)
        {
            var line = await LoadShipmentLineAsync(tenantId, lineId, cancellationToken);
            line.QuantityPicked += quantity;
            line.Status = WmsOutboundShipmentStatuses.Picked;
            line.OutboundShipment!.Status = WmsOutboundShipmentStatuses.Picked;
            line.OutboundShipment.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("wms.pick", tenantId, actorUserId, movementGroupId, cancellationToken);
        return (await ReplayAsync(tenantId, idempotencyKey, cancellationToken))!;
    }

    public async Task<WmsMovementResponse> ShipAsync(
        Guid tenantId,
        Guid actorUserId,
        ShipStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var replay = await ReplayAsync(tenantId, idempotencyKey, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var quantity = NormalizeQuantity(request.Quantity);
        var stock = await LoadStockAsync(tenantId, request.PartId, request.BinId, cancellationToken);
        EnsureMovementSafe(stock.InventoryBin);
        if (stock.QuantityOnHand < quantity)
        {
            throw new StlApiException("wms.ship.insufficient", "Insufficient stock on hand to ship.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var movementGroupId = Guid.NewGuid();
        var reservedDelta = -Math.Min(stock.QuantityReserved, quantity);
        stock.QuantityOnHand -= quantity;
        stock.QuantityReserved += reservedDelta;
        stock.UpdatedAt = now;

        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.Ship,
            request.PartId,
            request.BinId,
            null,
            -quantity,
            reservedDelta,
            stock,
            actorUserId,
            "manual",
            request.OutboundShipmentLineId,
            request.Notes,
            now);

        if (request.OutboundShipmentLineId is Guid lineId)
        {
            var line = await LoadShipmentLineAsync(tenantId, lineId, cancellationToken);
            line.QuantityShipped += quantity;
            line.Status = WmsOutboundShipmentStatuses.Shipped;
            line.OutboundShipment!.Status = WmsOutboundShipmentStatuses.Shipped;
            line.OutboundShipment.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("wms.ship", tenantId, actorUserId, movementGroupId, cancellationToken);
        return (await ReplayAsync(tenantId, idempotencyKey, cancellationToken))!;
    }

    public async Task<WmsMovementResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        CancelStockMovementRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var replay = await ReplayAsync(tenantId, idempotencyKey, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var quantity = NormalizeQuantity(request.Quantity);
        var stock = await LoadStockAsync(tenantId, request.PartId, request.BinId, cancellationToken);
        EnsureMovementSafe(stock.InventoryBin);
        if (stock.QuantityReserved < quantity)
        {
            throw new StlApiException("wms.cancel.not_reserved", "Only reserved stock can be cancelled/released.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var movementGroupId = Guid.NewGuid();
        stock.QuantityReserved -= quantity;
        stock.UpdatedAt = now;
        AddLedger(
            tenantId,
            movementGroupId,
            idempotencyKey,
            WmsMovementTypes.Cancel,
            request.PartId,
            request.BinId,
            null,
            0,
            -quantity,
            stock,
            actorUserId,
            "manual",
            null,
            request.Reason,
            now);

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync("wms.cancel", tenantId, actorUserId, movementGroupId, cancellationToken);
        return (await ReplayAsync(tenantId, idempotencyKey, cancellationToken))!;
    }

    public async Task<OutboundShipmentResponse> CreateOutboundShipmentAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateOutboundShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var idempotencyKey = NormalizeIdempotencyKey(request.IdempotencyKey);
        var existing = await LoadShipmentByIdempotencyKeyAsync(tenantId, idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return MapShipment(existing);
        }

        var shipVia = NormalizeShipVia(request.ShipVia);
        if (request.Lines.Count == 0)
        {
            throw new StlApiException("wms.shipments.lines_required", "At least one shipment line is required.", 400);
        }

        var shipmentKey = NormalizeShipmentKey(request.ShipmentKey);
        var duplicate = await db.WmsOutboundShipments.AnyAsync(
            x => x.TenantId == tenantId && x.ShipmentKey == shipmentKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException("wms.shipments.duplicate", "A shipment with this key already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var shipment = new WmsOutboundShipment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ShipmentKey = shipmentKey,
            Status = WmsOutboundShipmentStatuses.Created,
            ShipVia = shipVia,
            DestinationName = NormalizeText(request.DestinationName, 256),
            DestinationAddressSnapshot = NormalizeText(request.DestinationAddressSnapshot, 1024),
            IdempotencyKey = idempotencyKey,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var lineRequest in request.Lines)
        {
            var stock = await LoadStockAsync(tenantId, lineRequest.PartId, lineRequest.FromBinId, cancellationToken);
            EnsureMovementSafe(stock.InventoryBin);
            EnsureAvailable(stock, lineRequest.Quantity);
            shipment.Lines.Add(new WmsOutboundShipmentLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                OutboundShipmentId = shipment.Id,
                PartId = lineRequest.PartId,
                FromInventoryBinId = lineRequest.FromBinId,
                QuantityRequested = NormalizeQuantity(lineRequest.Quantity),
                Status = WmsOutboundShipmentStatuses.Created,
            });
        }

        db.WmsOutboundShipments.Add(shipment);
        await db.SaveChangesAsync(cancellationToken);

        if (shipVia == WmsShipVia.RoutArr)
        {
            var loaded = await LoadShipmentAsync(tenantId, shipment.Id, cancellationToken);
            var routarr = await routArrShipmentClient.CreateShipmentAsync(
                new RoutArrCreateShipmentPayload(
                    tenantId,
                    loaded.Id,
                    loaded.ShipmentKey,
                    loaded.DestinationName,
                    loaded.DestinationAddressSnapshot,
                    loaded.Lines.Select(line => new RoutArrCreateShipmentLinePayload(
                        line.Id,
                        line.PartId,
                        line.Part?.DisplayName ?? string.Empty,
                        line.QuantityRequested)).ToList()),
                cancellationToken);

            loaded.RoutarrShipmentIntentId = routarr.ShipmentIntentId;
            loaded.RoutarrRouteId = routarr.RouteId;
            loaded.RoutarrStatus = routarr.Status;
            loaded.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        await audit.WriteAsync(
            "wms.outbound_shipment.create",
            tenantId,
            actorUserId,
            "wms_outbound_shipment",
            shipment.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapShipment(await LoadShipmentAsync(tenantId, shipment.Id, cancellationToken));
    }

    public async Task<OutboundShipmentResponse> UpdateRoutArrStatusAsync(
        RoutArrShipmentStatusUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var shipment = await LoadShipmentAsync(request.TenantId, request.SupplyarrShipmentId, cancellationToken);
        shipment.RoutarrStatus = NormalizeText(request.Status, 64).ToLowerInvariant();
        shipment.RoutarrRouteId = request.RoutarrRouteId ?? shipment.RoutarrRouteId;
        if (shipment.RoutarrStatus is "delivered" or "completed")
        {
            shipment.Status = WmsOutboundShipmentStatuses.Shipped;
        }
        else if (shipment.RoutarrStatus is "cancelled" or "canceled")
        {
            shipment.Status = WmsOutboundShipmentStatuses.Cancelled;
        }

        shipment.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return MapShipment(shipment);
    }

    private IQueryable<WmsStockLedgerEntry> LedgerQuery(Guid tenantId) =>
        db.WmsStockLedgerEntries
            .AsNoTracking()
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .Where(x => x.TenantId == tenantId);

    private async Task<WmsMovementResponse?> ReplayAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var entries = await LedgerQuery(tenantId)
            .Where(x => x.IdempotencyKey == idempotencyKey)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return null;
        }

        return new WmsMovementResponse(
            entries[0].MovementGroupId,
            idempotencyKey,
            entries.Select(MapLedger).ToList());
    }

    private async Task<PartStockLevel> LoadStockAsync(
        Guid tenantId,
        Guid partId,
        Guid binId,
        CancellationToken cancellationToken)
    {
        return await db.PartStockLevels
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == partId && x.InventoryBinId == binId,
                cancellationToken)
            ?? throw new StlApiException("inventory.stock.no_stock", "No stock level exists for this part in the selected bin.", 409);
    }

    private async Task<PartStockLevel> LoadOrCreateStockAsync(
        Guid tenantId,
        Guid partId,
        Guid binId,
        CancellationToken cancellationToken)
    {
        var existing = await db.PartStockLevels
            .Include(x => x.Part)
            .Include(x => x.InventoryBin)
            .ThenInclude(x => x!.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == partId && x.InventoryBinId == binId,
                cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var part = await db.Parts.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("parts.not_found", "Part was not found.", 404);
        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == binId, cancellationToken)
            ?? throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);

        var now = DateTimeOffset.UtcNow;
        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            InventoryBinId = binId,
            CreatedAt = now,
            UpdatedAt = now,
            Part = part,
            InventoryBin = bin,
        };
        db.PartStockLevels.Add(stock);
        return stock;
    }

    private async Task<WmsOutboundShipment?> LoadShipmentByIdempotencyKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        CancellationToken cancellationToken) =>
        await db.WmsOutboundShipments
            .Include(x => x.Lines)
            .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
            .ThenInclude(x => x.FromInventoryBin)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);

    private async Task<WmsOutboundShipment> LoadShipmentAsync(
        Guid tenantId,
        Guid shipmentId,
        CancellationToken cancellationToken) =>
        await db.WmsOutboundShipments
            .Include(x => x.Lines)
            .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
            .ThenInclude(x => x.FromInventoryBin)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == shipmentId, cancellationToken)
        ?? throw new StlApiException("wms.shipments.not_found", "Outbound shipment was not found.", 404);

    private async Task<WmsOutboundShipmentLine> LoadShipmentLineAsync(
        Guid tenantId,
        Guid lineId,
        CancellationToken cancellationToken) =>
        await db.WmsOutboundShipmentLines
            .Include(x => x.OutboundShipment)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == lineId, cancellationToken)
        ?? throw new StlApiException("wms.shipments.line_not_found", "Outbound shipment line was not found.", 404);

    private static void EnsureMovementSafe(InventoryBin? bin)
    {
        if (bin is null)
        {
            throw new StlApiException("inventory.bins.not_found", "Inventory bin was not found.", 404);
        }

        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("inventory.bins.inactive", "Inventory bin must be active for stock movement.", 409);
        }

        InventoryLocationMovementSafety.EnsureMovementSafe(bin.InventoryLocation);
    }

    private static void EnsureAvailable(PartStockLevel stock, decimal quantity)
    {
        var available = stock.QuantityOnHand - stock.QuantityReserved;
        if (available < quantity)
        {
            throw new StlApiException("inventory.stock.insufficient", "Insufficient available stock.", 409);
        }
    }

    private void AddLedger(
        Guid tenantId,
        Guid movementGroupId,
        string idempotencyKey,
        string movementType,
        Guid partId,
        Guid binId,
        Guid? relatedBinId,
        decimal onHandDelta,
        decimal reservedDelta,
        PartStockLevel stock,
        Guid actorUserId,
        string sourceType,
        Guid? sourceReferenceId,
        string? notes,
        DateTimeOffset now)
    {
        db.WmsStockLedgerEntries.Add(new WmsStockLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MovementGroupId = movementGroupId,
            IdempotencyKey = idempotencyKey,
            MovementType = movementType,
            PartId = partId,
            InventoryBinId = binId,
            RelatedInventoryBinId = relatedBinId,
            QuantityOnHandDelta = onHandDelta,
            QuantityReservedDelta = reservedDelta,
            QuantityOnHandAfter = stock.QuantityOnHand,
            QuantityReservedAfter = stock.QuantityReserved,
            SourceType = sourceType,
            SourceReferenceId = sourceReferenceId,
            Notes = NormalizeText(notes, 1024),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
        });
    }

    private Task WriteAuditAsync(string action, Guid tenantId, Guid actorUserId, Guid movementGroupId, CancellationToken cancellationToken) =>
        audit.WriteAsync(
            action,
            tenantId,
            actorUserId,
            "wms_movement",
            movementGroupId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

    private static WmsStockLedgerEntryResponse MapLedger(WmsStockLedgerEntry entry) =>
        new(
            entry.Id,
            entry.MovementGroupId,
            entry.IdempotencyKey,
            entry.MovementType,
            entry.PartId,
            entry.Part?.PartKey ?? string.Empty,
            entry.Part?.DisplayName ?? string.Empty,
            entry.InventoryBinId,
            entry.InventoryBin?.BinKey ?? string.Empty,
            entry.InventoryBin?.Name ?? string.Empty,
            entry.InventoryBin?.InventoryLocationId ?? Guid.Empty,
            entry.InventoryBin?.InventoryLocation?.LocationKey ?? string.Empty,
            entry.InventoryBin?.InventoryLocation?.Name ?? string.Empty,
            entry.InventoryBin?.InventoryLocation?.StaffarrSiteOrgUnitId,
            entry.InventoryBin?.InventoryLocation?.StaffarrSiteNameSnapshot ?? string.Empty,
            entry.RelatedInventoryBinId,
            entry.QuantityOnHandDelta,
            entry.QuantityReservedDelta,
            entry.QuantityOnHandAfter,
            entry.QuantityReservedAfter,
            entry.SourceType,
            entry.SourceReferenceId,
            entry.Notes,
            entry.CreatedByUserId,
            entry.CreatedAt);

    private static OutboundShipmentResponse MapShipment(WmsOutboundShipment shipment) =>
        new(
            shipment.Id,
            shipment.ShipmentKey,
            shipment.Status,
            shipment.ShipVia,
            shipment.DestinationName,
            shipment.DestinationAddressSnapshot,
            shipment.RoutarrShipmentIntentId,
            shipment.RoutarrRouteId,
            shipment.RoutarrStatus,
            shipment.IdempotencyKey,
            shipment.Lines
                .OrderBy(x => x.Id)
                .Select(line => new OutboundShipmentLineResponse(
                    line.Id,
                    line.PartId,
                    line.Part?.PartKey ?? string.Empty,
                    line.Part?.DisplayName ?? string.Empty,
                    line.FromInventoryBinId,
                    line.FromInventoryBin?.BinKey ?? string.Empty,
                    line.QuantityRequested,
                    line.QuantityReserved,
                    line.QuantityPicked,
                    line.QuantityShipped,
                    line.Status))
                .ToList(),
            shipment.CreatedAt,
            shipment.UpdatedAt);

    private static string NormalizeIdempotencyKey(string value)
    {
        var normalized = NormalizeText(value, 128);
        if (normalized.Length < 8)
        {
            throw new StlApiException("wms.idempotency_key_invalid", "Idempotency key must be at least 8 characters.", 400);
        }

        return normalized;
    }

    private static string NormalizeShipmentKey(string value)
    {
        var normalized = NormalizeText(value, 128).ToLowerInvariant();
        if (normalized.Length < 2)
        {
            throw new StlApiException("wms.shipments.invalid_key", "Shipment key is required.", 400);
        }

        return normalized;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException("wms.quantity_invalid", "Quantity must be greater than zero.", 400);
        }

        return quantity;
    }

    private static string NormalizeSourceType(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "manual" : NormalizeText(value, 64).ToLowerInvariant();

    private static string NormalizeShipVia(string value)
    {
        var normalized = NormalizeText(value, 32).ToLowerInvariant();
        if (normalized is not (WmsShipVia.Manual or WmsShipVia.RoutArr))
        {
            throw new StlApiException("wms.shipvia_invalid", "shipVia must be manual or routarr.", 400);
        }

        return normalized;
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
