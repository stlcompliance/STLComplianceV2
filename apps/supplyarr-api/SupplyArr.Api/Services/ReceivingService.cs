using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ReceivingService(
    SupplyArrDbContext db,
    VendorProcurementGuardService vendorProcurementGuard,
    PartStockService stock,
    BackorderService backorders,
    SupplyArrDemandStatusCallbackCoordinator demandStatusCallbacks,
    ProcurementNotificationEnqueueService notificationEnqueue,
    IntegrationOutboxEnqueueService integrationOutbox,
    TrainArrQualificationCheckClient trainArrQualificationCheckClient,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<ReceivingReceiptResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? purchaseOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ReceivingReceipts
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
            .Include(x => x.InventoryBin)
                .ThenInclude(x => x!.InventoryLocation)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
                .ThenInclude(x => x.PurchaseOrderLine)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Exceptions)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (purchaseOrderId is not null)
        {
            query = query.Where(x => x.PurchaseOrderId == purchaseOrderId);
        }

        var receipts = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return receipts.Select(Map).ToList();
    }

    public async Task<ReceivingReceiptResponse> GetAsync(
        Guid tenantId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, receivingReceiptId, cancellationToken);
        return Map(entity);
    }

    public async Task<ReceivingReceiptResponse> CreateFromPurchaseOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        CreateReceivingReceiptFromPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await db.PurchaseOrders
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseOrderId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.purchase_order.not_found",
                "Purchase order was not found.",
                404);

        if (!string.Equals(
                purchaseOrder.Status,
                PurchaseOrderStatuses.Issued,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "receiving.purchase_order.not_issued",
                "Receiving receipts can only be created against issued purchase orders.",
                409);
        }

        await vendorProcurementGuard.EnsureVendorAllowedForScopeAsync(
            tenantId,
            purchaseOrder.VendorPartyId,
            VendorRestrictionScopes.Receiving,
            cancellationToken);

        var bin = await db.InventoryBins
            .Include(x => x.InventoryLocation)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == request.InventoryBinId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.bin.not_found",
                "Inventory bin was not found.",
                404);

        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "receiving.bin.inactive",
                "Stock cannot be received into an inactive bin.",
                400);
        }

        var receiptKey = NormalizeReceiptKey(request.ReceiptKey);
        var duplicateKey = await db.ReceivingReceipts.AnyAsync(
            x => x.TenantId == tenantId && x.ReceiptKey == receiptKey,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "receiving.duplicate",
                "A receiving receipt with this key already exists.",
                409);
        }

        var openDraft = await db.ReceivingReceipts.AnyAsync(
            x => x.TenantId == tenantId
                && x.PurchaseOrderId == purchaseOrderId
                && x.Status == ReceivingReceiptStatuses.Draft,
            cancellationToken);
        if (openDraft)
        {
            throw new StlApiException(
                "receiving.purchase_order.draft_exists",
                "A draft receiving receipt already exists for this purchase order.",
                409);
        }

        var linesWithRemaining = purchaseOrder.Lines
            .Select(line => new
            {
                Line = line,
                Remaining = line.QuantityOrdered - line.QuantityReceived
            })
            .Where(x => x.Remaining > 0)
            .ToList();

        if (linesWithRemaining.Count == 0)
        {
            throw new StlApiException(
                "receiving.purchase_order.fully_received",
                "All purchase order lines are already fully received.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ReceivingReceipt
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceiptKey = receiptKey,
            PurchaseOrderId = purchaseOrder.Id,
            InventoryBinId = bin.Id,
            Status = ReceivingReceiptStatuses.Draft,
            Notes = NormalizeNotes(request.Notes ?? string.Empty),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
            InventoryBin = bin,
            PurchaseOrder = purchaseOrder
        };

        var lineNumber = 1;
        foreach (var item in linesWithRemaining.OrderBy(x => x.Line.LineNumber))
        {
            entity.Lines.Add(new ReceivingReceiptLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReceivingReceiptId = entity.Id,
                PurchaseOrderLineId = item.Line.Id,
                PartId = item.Line.PartId,
                LineNumber = lineNumber++,
                QuantityExpected = decimal.Round(item.Remaining, 4, MidpointRounding.AwayFromZero),
                QuantityReceived = decimal.Round(item.Remaining, 4, MidpointRounding.AwayFromZero),
                CreatedAt = now,
                UpdatedAt = now,
                Part = item.Line.Part,
                PurchaseOrderLine = item.Line
            });
        }

        db.ReceivingReceipts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.create_from_po",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        Guid lineId,
        UpdateReceivingReceiptLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "receiving.line.not_found",
                "Receiving receipt line was not found.",
                404);

        var quantity = NormalizeQuantity(request.QuantityReceived);
        line.QuantityReceived = quantity;
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.line.update",
            tenantId,
            actorUserId,
            "receiving_receipt_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> PostAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);
        var now = DateTimeOffset.UtcNow;

        // Auto-create mismatch exceptions so posting can proceed without manual discrepancy entry.
        // This aligns receipt posting with end-goal behavior for quantity mismatch handling.
        foreach (var line in entity.Lines)
        {
            EnsureAutoMismatchExceptions(entity, line, tenantId, actorUserId, now);
        }

        foreach (var line in entity.Lines)
        {
            if (line.QuantityReceived < 0)
            {
                throw new StlApiException(
                    "receiving.line.quantity_invalid",
                    "Line quantity cannot be negative.",
                    400);
            }

            var lineExceptions = line.Exceptions.ToList();

            if (line.QuantityReceived > 0 || lineExceptions.Count > 0)
            {
                ReceivingExceptionService.ValidateLineCoverageForPost(line, lineExceptions);
            }
            else if (line.QuantityExpected > 0)
            {
                throw new StlApiException(
                    "receiving.line.quantity_required",
                    "Each line must have a received quantity or documented exceptions before posting.",
                    400);
            }
        }

        if (entity.Lines.All(x => x.QuantityReceived <= 0)
            && !await db.ReceivingExceptions.AnyAsync(
                x => x.TenantId == tenantId && x.ReceivingReceiptId == receivingReceiptId,
                cancellationToken))
        {
            throw new StlApiException(
                "receiving.lines.required",
                "At least one line item with quantity or exceptions is required before posting.",
                400);
        }

        await EnsureReceivingPersonQualifiedAsync(
            tenantId,
            actorPersonId,
            entity,
            cancellationToken);

        foreach (var line in entity.Lines)
        {
            if (line.QuantityReceived > 0)
            {
                line.PurchaseOrderLine.QuantityReceived += line.QuantityReceived;
                line.PurchaseOrderLine.UpdatedAt = now;

                await stock.IncrementOnHandAsync(
                    tenantId,
                    actorUserId,
                    line.PartId,
                    entity.InventoryBinId,
                    line.QuantityReceived,
                    cancellationToken);
            }
        }

        entity.Status = ReceivingReceiptStatuses.Posted;
        entity.PostedAt = now;
        entity.PostedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await backorders.SyncAfterReceivingPostAsync(
            tenantId,
            actorUserId,
            entity,
            cancellationToken);

        var quantityReceivedDelta = entity.Lines.Sum(x => x.QuantityReceived);
        await demandStatusCallbacks.NotifyReceivingPostedAsync(
            tenantId,
            entity.PurchaseOrderId,
            entity.Id,
            quantityReceivedDelta,
            now,
            cancellationToken);

        await audit.WriteAsync(
            "receiving_receipt.post",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await notificationEnqueue.TryEnqueueAsync(
            tenantId,
            ProcurementNotificationEventKinds.ReceivingReceiptPosted,
            entity.PurchaseOrder?.VendorPartyId,
            "receiving_receipt",
            entity.Id,
            cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.ReceivingReceiptPosted,
            "receiving_receipt",
            entity.Id,
            new IntegrationOutboxPayload(
                tenantId,
                $"Receiving receipt posted: {entity.ReceiptKey}",
                entity.PurchaseOrder?.VendorPartyId),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private void EnsureAutoMismatchExceptions(
        ReceivingReceipt receipt,
        ReceivingReceiptLine line,
        Guid tenantId,
        Guid actorUserId,
        DateTimeOffset now)
    {
        var existingOpenOrResolved = line.Exceptions.ToList();
        var existingShort = existingOpenOrResolved
            .Where(x => string.Equals(x.ExceptionType, ReceivingExceptionTypes.Short, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Quantity);
        var existingOver = existingOpenOrResolved
            .Where(x => string.Equals(x.ExceptionType, ReceivingExceptionTypes.Over, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Quantity);
        var existingDamage = existingOpenOrResolved
            .Where(x => string.Equals(x.ExceptionType, ReceivingExceptionTypes.Damage, StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Quantity);

        var remainingOnOrder = line.PurchaseOrderLine.QuantityOrdered - line.PurchaseOrderLine.QuantityReceived;
        var overVariance = line.QuantityReceived > remainingOnOrder ? line.QuantityReceived - remainingOnOrder : 0m;
        var missingOver = overVariance - existingOver;
        if (missingOver > 0.0001m)
        {
            AddAutoException(
                receipt,
                line,
                tenantId,
                actorUserId,
                ReceivingExceptionTypes.Over,
                missingOver,
                now);
            existingOver += missingOver;
        }

        var shortVariance = line.QuantityExpected - (line.QuantityReceived + existingDamage + existingShort);
        if (shortVariance > 0.0001m)
        {
            AddAutoException(
                receipt,
                line,
                tenantId,
                actorUserId,
                ReceivingExceptionTypes.Short,
                shortVariance,
                now);
        }
    }

    private void AddAutoException(
        ReceivingReceipt receipt,
        ReceivingReceiptLine line,
        Guid tenantId,
        Guid actorUserId,
        string exceptionType,
        decimal quantity,
        DateTimeOffset now)
    {
        var entity = new ReceivingException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReceivingReceiptId = receipt.Id,
            ReceivingReceiptLineId = line.Id,
            ExceptionType = exceptionType,
            Quantity = decimal.Round(quantity, 4, MidpointRounding.AwayFromZero),
            Notes = "Auto-created from receiving quantity mismatch during receipt posting.",
            Status = ReceivingExceptionStatuses.Open,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
            ReceivingReceiptLine = line
        };

        line.Exceptions.Add(entity);
        db.ReceivingExceptions.Add(entity);
        line.UpdatedAt = now;
        receipt.UpdatedAt = now;
    }

    private static void EnsureEditable(ReceivingReceipt entity)
    {
        if (!ReceivingReceiptStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException(
                "receiving.not_editable",
                "Receiving receipt can only be edited while in draft status.",
                409);
        }
    }

    private async Task<ReceivingReceipt> LoadAsync(
        Guid tenantId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken)
    {
        return await db.ReceivingReceipts
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
            .Include(x => x.InventoryBin)
                .ThenInclude(x => x!.InventoryLocation)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
                .ThenInclude(x => x.PurchaseOrderLine)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Exceptions)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == receivingReceiptId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);
    }

    private async Task<ReceivingReceipt> LoadTrackedAsync(
        Guid tenantId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken)
    {
        return await db.ReceivingReceipts
            .Include(x => x.PurchaseOrder)
            .Include(x => x.InventoryBin)
                .ThenInclude(x => x!.InventoryLocation)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
                .ThenInclude(x => x.PurchaseOrderLine)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Exceptions)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == receivingReceiptId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);
    }

    private static ReceivingReceiptResponse Map(ReceivingReceipt entity) =>
        new(
            entity.Id,
            entity.ReceiptKey,
            entity.Status,
            entity.PurchaseOrderId,
            entity.PurchaseOrder.OrderKey,
            entity.InventoryBinId,
            entity.InventoryBin.BinKey,
            entity.InventoryBin.Name,
            entity.InventoryBin.InventoryLocationId,
            entity.InventoryBin.InventoryLocation?.LocationKey ?? string.Empty,
            entity.InventoryBin.InventoryLocation?.Name ?? string.Empty,
            entity.Notes,
            entity.CreatedByUserId,
            entity.PostedAt,
            entity.PostedByUserId,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(MapLine)
                .ToList(),
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .SelectMany(line => line.Exceptions
                    .OrderBy(x => x.ExceptionType)
                    .Select(ex => ReceivingExceptionService.Map(ex, line)))
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static ReceivingReceiptLineResponse MapLine(ReceivingReceiptLine line)
    {
        var ordered = line.PurchaseOrderLine.QuantityOrdered;
        var previouslyReceived = line.PurchaseOrderLine.QuantityReceived;
        var remaining = ordered - previouslyReceived;
        if (remaining < 0)
        {
            remaining = 0;
        }

        return new(
            line.Id,
            line.LineNumber,
            line.PurchaseOrderLineId,
            line.PartId,
            line.Part.PartKey,
            line.Part.DisplayName,
            line.QuantityExpected,
            line.QuantityReceived,
            ordered,
            previouslyReceived,
            remaining,
            line.Exceptions
                .OrderBy(x => x.ExceptionType)
                .Select(ex => ReceivingExceptionService.Map(ex, line))
                .ToList(),
            line.CreatedAt,
            line.UpdatedAt);
    }

    private static string NormalizeReceiptKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException(
                "receiving.receipt_key.required",
                "Receipt key is required.",
                400);
        }

        if (key.Length > 128)
        {
            throw new StlApiException(
                "receiving.receipt_key.too_long",
                "Receipt key must be 128 characters or fewer.",
                400);
        }

        return key;
    }

    private static string NormalizeNotes(string value)
    {
        var notes = (value ?? string.Empty).Trim();
        return notes.Length > 1024 ? notes[..1024] : notes;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new StlApiException(
                "receiving.quantity.invalid",
                "Quantity cannot be negative.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private async Task EnsureReceivingPersonQualifiedAsync(
        Guid tenantId,
        Guid actorPersonId,
        ReceivingReceipt entity,
        CancellationToken cancellationToken)
    {
        if (!trainArrQualificationCheckClient.IsReceivingCheckConfigured)
        {
            return;
        }

        var context = new Dictionary<string, string>
        {
            ["product"] = "supplyarr",
            ["action"] = "receiving_post",
            ["receivingReceiptId"] = entity.Id.ToString("D"),
            ["purchaseOrderId"] = entity.PurchaseOrderId.ToString("D"),
            ["inventoryBinId"] = entity.InventoryBinId.ToString("D"),
            ["receiptKey"] = entity.ReceiptKey
        };

        if (entity.InventoryBin?.InventoryLocationId is Guid locationId)
        {
            context["inventoryLocationId"] = locationId.ToString("D");
        }

        var check = await trainArrQualificationCheckClient.CheckReceivingAsync(
            tenantId,
            actorPersonId,
            context,
            cancellationToken);

        if (check is null || string.Equals(check.Outcome, "allow", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new StlApiException(
            "receiving.person_qualification_blocked",
            $"TrainArr receiving qualification check returned {check.Outcome}: {check.Message}",
            409);
    }
}
