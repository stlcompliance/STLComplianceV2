using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;
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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> AllowedLineConditions = new(StringComparer.OrdinalIgnoreCase)
    {
        "good",
        "damaged",
        "wrong_item",
        "pending_inspection",
        "quarantined",
        "returned"
    };

    public async Task<IReadOnlyList<ReceivingReceiptResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? purchaseOrderId = null,
        string? purchaseOrderKey = null,
        string? packingSlipReference = null,
        string? invoiceReference = null,
        string? queryText = null,
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
            query = string.Equals(normalizedStatus, ReceivingReceiptStatuses.Posted, StringComparison.OrdinalIgnoreCase)
                ? query.Where(x => ReceivingReceiptStatuses.PostedLike.Contains(x.Status))
                : query.Where(x => x.Status == normalizedStatus);
        }

        if (purchaseOrderId is not null)
        {
            query = query.Where(x => x.PurchaseOrderId == purchaseOrderId);
        }

        if (!string.IsNullOrWhiteSpace(purchaseOrderKey))
        {
            var normalizedOrderKey = NormalizePurchaseOrderKey(purchaseOrderKey);
            query = query.Where(x => x.PurchaseOrder.OrderKey == normalizedOrderKey);
        }

        if (!string.IsNullOrWhiteSpace(packingSlipReference))
        {
            var normalizedPackingSlipReference = NormalizePackingSlipReference(packingSlipReference);
            query = query.Where(x => x.PackingSlipReference == normalizedPackingSlipReference);
        }

        if (!string.IsNullOrWhiteSpace(invoiceReference))
        {
            var normalizedInvoiceReference = NormalizeInvoiceReference(invoiceReference);
            query = query.Where(x => x.InvoiceReference == normalizedInvoiceReference);
        }

        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var normalizedQuery = queryText.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.ReceiptKey.Contains(normalizedQuery)
                || x.PurchaseOrder.OrderKey.Contains(normalizedQuery)
                || x.PackingSlipReference.ToLower().Contains(normalizedQuery)
                || x.InvoiceReference.ToLower().Contains(normalizedQuery));
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

    public async Task<ReceivingReceiptResponse> GetByReceiptKeyAsync(
        Guid tenantId,
        string receiptKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedReceiptKey = NormalizeReceiptKey(receiptKey);
        var entity = await db.ReceivingReceipts
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
                x => x.TenantId == tenantId && x.ReceiptKey == normalizedReceiptKey,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);

        return Map(entity);
    }

    public async Task<IReadOnlyList<ReceivingReceiptResponse>> ListByPackingSlipReferenceAsync(
        Guid tenantId,
        string packingSlipReference,
        CancellationToken cancellationToken = default)
    {
        var normalizedReference = NormalizePackingSlipReference(packingSlipReference);
        if (string.IsNullOrWhiteSpace(normalizedReference))
        {
            throw new StlApiException(
                "receiving.packing_slip_reference.required",
                "Packing slip reference is required.",
                400);
        }

        var entities = await db.ReceivingReceipts
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
            .Include(x => x.InventoryBin)
                .ThenInclude(x => x.InventoryLocation)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
                .ThenInclude(x => x.PurchaseOrderLine)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Exceptions)
            .Where(x =>
                x.TenantId == tenantId
                && x.PackingSlipReference == normalizedReference)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToList();
    }

    public async Task<ReceivingReceiptResponse> CreateFromPurchaseOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        CreateReceivingReceiptFromPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await LoadIssuedPurchaseOrderForReceivingByIdAsync(
            tenantId,
            purchaseOrderId,
            cancellationToken);
        return await CreateFromPurchaseOrderAsync(
            tenantId,
            actorUserId,
            purchaseOrder,
            request,
            cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> CreateFromPurchaseOrderKeyAsync(
        Guid tenantId,
        Guid actorUserId,
        string purchaseOrderKey,
        CreateReceivingReceiptFromPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedOrderKey = NormalizePurchaseOrderKey(purchaseOrderKey);
        var purchaseOrder = await LoadIssuedPurchaseOrderForReceivingByKeyAsync(
            tenantId,
            normalizedOrderKey,
            cancellationToken);
        return await CreateFromPurchaseOrderAsync(
            tenantId,
            actorUserId,
            purchaseOrder,
            request,
            cancellationToken);
    }

    private async Task<ReceivingReceiptResponse> CreateFromPurchaseOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        PurchaseOrder purchaseOrder,
        CreateReceivingReceiptFromPurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {

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
                && x.PurchaseOrderId == purchaseOrder.Id
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

        var selectedLineIds = (request.PurchaseOrderLineIds ?? [])
            .Distinct()
            .ToList();
        if (selectedLineIds.Count > 0)
        {
            var openLineIds = linesWithRemaining
                .Select(x => x.Line.Id)
                .ToHashSet();
            var invalidSelection = selectedLineIds
                .Where(id => !openLineIds.Contains(id))
                .ToList();
            if (invalidSelection.Count > 0)
            {
                throw new StlApiException(
                    "receiving.purchase_order.line_selection.invalid",
                    "Selected purchase order lines must belong to the purchase order and have remaining quantity.",
                    400);
            }

            linesWithRemaining = linesWithRemaining
                .Where(x => selectedLineIds.Contains(x.Line.Id))
                .ToList();
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
            PackingSlipReference = NormalizePackingSlipReference(request.PackingSlipReference ?? string.Empty),
            PackingSlipFileName = NormalizePackingSlipFileName(request.PackingSlipFileName ?? string.Empty),
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
                Condition = "good",
                SerialLotNumbersJson = "[]",
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

        var hasPackingSlipReference = !string.IsNullOrWhiteSpace(entity.PackingSlipReference);
        var hasMissingPackingSlipException = entity.Lines
            .SelectMany(x => x.Exceptions)
            .Any(x => string.Equals(
                x.ExceptionType,
                ReceivingExceptionTypes.MissingPackingSlip,
                StringComparison.OrdinalIgnoreCase));
        if (!hasPackingSlipReference && !hasMissingPackingSlipException)
        {
            throw new StlApiException(
                "receiving.packing_slip.required",
                "Packing slip reference is required before posting unless a missing_packing_slip exception is recorded.",
                400);
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
                ValidateSerialLotTracking(line);
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

        entity.Status = DeterminePostedStatus(entity);
        entity.PostedAt = now;
        entity.PostedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await ReleaseLinkedReservationsAfterPostAsync(
            tenantId,
            actorUserId,
            entity,
            now,
            cancellationToken);

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

    private async Task ReleaseLinkedReservationsAfterPostAsync(
        Guid tenantId,
        Guid actorUserId,
        ReceivingReceipt receipt,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        foreach (var line in receipt.Lines.Where(x => x.QuantityReceived > 0))
        {
            var remainingToRelease = line.QuantityReceived;
            if (remainingToRelease <= 0)
            {
                continue;
            }

            var reservations = await db.PartStockReservations
                .Where(x =>
                    x.TenantId == tenantId
                    && x.Status == StockReservationStatuses.Active
                    && x.SourceReferenceId == line.PurchaseOrderLineId
                    && x.SourceType == "purchase_order_line")
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var reservation in reservations)
            {
                if (remainingToRelease <= 0)
                {
                    break;
                }

                var stockLevel = await db.PartStockLevels.FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == reservation.PartStockLevelId,
                    cancellationToken);
                if (stockLevel is null)
                {
                    continue;
                }

                var quantityToRelease = decimal.Min(reservation.QuantityReserved, remainingToRelease);
                if (quantityToRelease <= 0)
                {
                    continue;
                }

                stockLevel.QuantityReserved -= quantityToRelease;
                if (stockLevel.QuantityReserved < 0)
                {
                    stockLevel.QuantityReserved = 0;
                }

                stockLevel.UpdatedAt = now;
                reservation.QuantityReserved -= quantityToRelease;
                if (reservation.QuantityReserved <= 0)
                {
                    reservation.QuantityReserved = 0;
                    reservation.Status = StockReservationStatuses.Released;
                    reservation.ReleasedByUserId = actorUserId;
                    reservation.ReleasedAt = now;
                    reservation.ReleaseReason = "released after linked receipt posting";
                }

                reservation.UpdatedAt = now;

                remainingToRelease -= quantityToRelease;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<PurchaseOrder> LoadIssuedPurchaseOrderForReceivingByIdAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseOrderId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.purchase_order.not_found",
                "Purchase order was not found.",
                404);
    }

    private async Task<PurchaseOrder> LoadIssuedPurchaseOrderForReceivingByKeyAsync(
        Guid tenantId,
        string orderKey,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.OrderKey == orderKey,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.purchase_order.not_found",
                "Purchase order was not found.",
                404);
    }

    public async Task<ReceivingReceiptResponse> CloseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        if (string.Equals(entity.Status, ReceivingReceiptStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "receiving.not_posted",
                "Draft receiving receipts cannot be closed.",
                409);
        }

        if (string.Equals(entity.Status, ReceivingReceiptStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            return await GetAsync(tenantId, entity.Id, cancellationToken);
        }

        entity.Status = ReceivingReceiptStatuses.Closed;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "receiving_receipt.close",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> ReopenAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        if (!string.Equals(entity.Status, ReceivingReceiptStatuses.Closed, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "receiving.not_closed",
                "Only closed receiving receipts can be reopened.",
                409);
        }

        entity.Status = ReceivingReceiptStatuses.Posted;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "receiving_receipt.reopen",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdateLineTrackingAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        Guid lineId,
        UpdateReceivingReceiptLineTrackingRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "receiving.line.not_found",
                "Receiving receipt line was not found.",
                404);

        line.SerialLotNumbersJson = NormalizeSerialLotNumbersJson(request.SerialLotNumbers);
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.line.tracking.update",
            tenantId,
            actorUserId,
            "receiving_receipt_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdateLineConditionAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        Guid lineId,
        UpdateReceivingReceiptLineConditionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "receiving.line.not_found",
                "Receiving receipt line was not found.",
                404);

        line.Condition = NormalizeLineCondition(request.Condition);
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.line.condition.update",
            tenantId,
            actorUserId,
            "receiving_receipt_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdatePackingSlipAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        UpdateReceivingPackingSlipRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);

        entity.PackingSlipReference = NormalizePackingSlipReference(request.PackingSlipReference ?? string.Empty);
        entity.PackingSlipFileName = NormalizePackingSlipFileName(request.PackingSlipFileName ?? string.Empty);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.packing_slip.update",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdateInvoiceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        UpdateReceivingInvoiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);

        entity.InvoiceReference = NormalizeInvoiceReference(request.InvoiceReference ?? string.Empty);
        entity.InvoiceFileName = NormalizeInvoiceFileName(request.InvoiceFileName ?? string.Empty);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.invoice.update",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<ReceivingReceiptResponse> UpdateInventoryBinAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid receivingReceiptId,
        UpdateReceivingInventoryBinRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, receivingReceiptId, cancellationToken);
        EnsureEditable(entity);

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

        entity.InventoryBinId = bin.Id;
        entity.InventoryBin = bin;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "receiving_receipt.inventory_bin.update",
            tenantId,
            actorUserId,
            "receiving_receipt",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<byte[]> BuildAccountingExportCsvAsync(
        Guid tenantId,
        Guid receivingReceiptId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ReceivingReceipts
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.PurchaseRequest)
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.VendorParty)
            .Include(x => x.InventoryBin)
                .ThenInclude(x => x.InventoryLocation)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.Lines)
                .ThenInclude(x => x.PurchaseOrderLine)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == receivingReceiptId,
                cancellationToken)
            ?? throw new StlApiException(
                "receiving.not_found",
                "Receiving receipt was not found.",
                404);

        var builder = new StringBuilder();
        builder.AppendLine("receiptKey,receiptStatus,postedAt,orderKey,orderStatus,requestKey,vendorPartyKey,vendorDisplayName,locationKey,binKey,lineNumber,partKey,partDisplayName,quantityReceived,unitOfMeasure,unitPrice,receivedAmount,receiptNotes,orderIssuedAt,invoiceReference,invoiceFileName");

        foreach (var line in entity.Lines.OrderBy(x => x.LineNumber))
        {
            var row = new[]
            {
                entity.ReceiptKey,
                entity.Status,
                entity.PostedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                entity.PurchaseOrder.OrderKey,
                entity.PurchaseOrder.Status,
                entity.PurchaseOrder.PurchaseRequest?.RequestKey ?? string.Empty,
                entity.PurchaseOrder.VendorParty?.PartyKey ?? string.Empty,
                entity.PurchaseOrder.VendorParty?.DisplayName ?? string.Empty,
                entity.InventoryBin.InventoryLocation?.LocationKey ?? string.Empty,
                entity.InventoryBin.BinKey,
                line.LineNumber.ToString(CultureInfo.InvariantCulture),
                line.Part.PartKey,
                line.Part.DisplayName,
                line.QuantityReceived.ToString(CultureInfo.InvariantCulture),
                line.PurchaseOrderLine.UnitOfMeasure,
                string.Empty,
                string.Empty,
                entity.Notes,
                entity.PurchaseOrder.IssuedAt?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
                entity.InvoiceReference,
                entity.InvoiceFileName
            };
            builder.AppendLine(string.Join(",", row.Select(x => EscapeCsv(x ?? string.Empty))));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private void EnsureAutoMismatchExceptions(
        ReceivingReceipt receipt,
        ReceivingReceiptLine line,
        Guid tenantId,
        Guid actorUserId,
        DateTimeOffset now)
    {
        var existingOpenOrResolved = line.Exceptions
            .Where(x => !string.Equals(x.Status, ReceivingExceptionStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
            .ToList();
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
            entity.PackingSlipReference,
            entity.PackingSlipFileName,
            entity.InvoiceReference,
            entity.InvoiceFileName,
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
            line.Condition,
            ordered,
            previouslyReceived,
            remaining,
            DeserializeSerialLotNumbers(line.SerialLotNumbersJson),
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

    private static string NormalizePurchaseOrderKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException(
                "receiving.purchase_order.order_key.required",
                "Purchase order key is required.",
                400);
        }

        if (key.Length > 128)
        {
            throw new StlApiException(
                "receiving.purchase_order.order_key.too_long",
                "Purchase order key must be 128 characters or fewer.",
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

    private static string NormalizeLineCondition(string value)
    {
        var condition = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (condition.Length == 0)
        {
            throw new StlApiException(
                "receiving.line.condition.required",
                "Line condition is required.",
                400);
        }

        if (!AllowedLineConditions.Contains(condition))
        {
            throw new StlApiException(
                "receiving.line.condition.invalid",
                "Line condition is invalid.",
                400);
        }

        return condition;
    }

    private static void ValidateSerialLotTracking(ReceivingReceiptLine line)
    {
        if (!line.Part.RequiresSerialLotTracking || line.QuantityReceived <= 0)
        {
            return;
        }

        var numbers = DeserializeSerialLotNumbers(line.SerialLotNumbersJson);
        if (numbers.Count == 0)
        {
            throw new StlApiException(
                "receiving.line.serial_lot.required",
                "Serial/lot numbers are required for this part before posting.",
                400);
        }

        var wholeUnits = decimal.Truncate(line.QuantityReceived);
        if (line.QuantityReceived == wholeUnits && wholeUnits > 0 && numbers.Count != (int)wholeUnits)
        {
            throw new StlApiException(
                "receiving.line.serial_lot.count_mismatch",
                "Serial/lot number count must match received quantity for whole-unit receipts.",
                400);
        }
    }

    private static string NormalizeSerialLotNumbersJson(IReadOnlyList<string>? serialLotNumbers)
    {
        var normalized = (serialLotNumbers ?? [])
            .Select(x => (x ?? string.Empty).Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Length > 128 ? x[..128] : x)
            .Take(200)
            .ToList();
        return JsonSerializer.Serialize(normalized, JsonOptions);
    }

    private static IReadOnlyList<string> DeserializeSerialLotNumbers(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
            return parsed
                .Select(x => (x ?? string.Empty).Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizePackingSlipReference(string value)
    {
        var reference = (value ?? string.Empty).Trim();
        return reference.Length > 256 ? reference[..256] : reference;
    }

    private static string NormalizePackingSlipFileName(string value)
    {
        var fileName = (value ?? string.Empty).Trim();
        return fileName.Length > 256 ? fileName[..256] : fileName;
    }

    private static string NormalizeInvoiceReference(string value)
    {
        var reference = (value ?? string.Empty).Trim();
        return reference.Length > 256 ? reference[..256] : reference;
    }

    private static string NormalizeInvoiceFileName(string value)
    {
        var fileName = (value ?? string.Empty).Trim();
        return fileName.Length > 256 ? fileName[..256] : fileName;
    }

    private static string EscapeCsv(string value)
    {
        var safe = value ?? string.Empty;
        if (safe.IndexOfAny([',', '"', '\r', '\n']) >= 0)
        {
            return $"\"{safe.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return safe;
    }

    private static string DeterminePostedStatus(ReceivingReceipt receipt)
    {
        var exceptions = receipt.Lines.SelectMany(x => x.Exceptions).ToList();
        var conditions = receipt.Lines
            .Select(x => (x.Condition ?? string.Empty).Trim().ToLowerInvariant())
            .Where(x => x.Length > 0)
            .ToList();

        if (exceptions.Any(x => string.Equals(x.ExceptionType, ReceivingExceptionTypes.WrongItem, StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.WrongItem;
        }

        if (conditions.Any(x => string.Equals(x, "wrong_item", StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.WrongItem;
        }

        if (exceptions.Any(x =>
                string.Equals(x.ExceptionType, ReceivingExceptionTypes.QualityIssue, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.ExceptionType, ReceivingExceptionTypes.ExpiredItem, StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.Quarantined;
        }

        if (conditions.Any(x => string.Equals(x, "quarantined", StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.Quarantined;
        }

        if (exceptions.Any(x => string.Equals(x.ExceptionType, ReceivingExceptionTypes.RequiresInspection, StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.PendingInspection;
        }

        if (conditions.Any(x => string.Equals(x, "pending_inspection", StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.PendingInspection;
        }

        if (exceptions.Any(x =>
                string.Equals(x.ExceptionType, ReceivingExceptionTypes.Damage, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.ExceptionType, ReceivingExceptionTypes.DamagedGoods, StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.Damaged;
        }

        if (conditions.Any(x => string.Equals(x, "damaged", StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.Damaged;
        }

        if (conditions.Count > 0
            && conditions.All(x => string.Equals(x, "returned", StringComparison.OrdinalIgnoreCase)))
        {
            return ReceivingReceiptStatuses.Returned;
        }

        var totalExpected = receipt.Lines.Sum(x => x.QuantityExpected);
        var totalReceived = receipt.Lines.Sum(x => x.QuantityReceived);

        if (totalReceived > totalExpected + 0.0001m)
        {
            return ReceivingReceiptStatuses.Overreceived;
        }

        if (totalReceived + 0.0001m < totalExpected)
        {
            return ReceivingReceiptStatuses.Underreceived;
        }

        if (receipt.Lines.Any(x => x.QuantityReceived <= 0))
        {
            return ReceivingReceiptStatuses.PartiallyReceived;
        }

        return ReceivingReceiptStatuses.Posted;
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
