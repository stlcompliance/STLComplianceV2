using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class BackorderService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<BackorderResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? purchaseOrderId = null,
        Guid? partId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Backorders
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.PurchaseRequest)
            .Include(x => x.PurchaseOrderLine)
            .Include(x => x.Part)
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

        if (partId is not null)
        {
            query = query.Where(x => x.PartId == partId);
        }

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<BackorderResponse> GetAsync(
        Guid tenantId,
        Guid backorderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, backorderId, cancellationToken);
        return Map(entity);
    }

    public async Task<BackorderResponse> CreateFromPurchaseOrderLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderLineId,
        CreateBackorderFromPurchaseOrderLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var line = await db.PurchaseOrderLines
            .Include(x => x.PurchaseOrder)
            .Include(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseOrderLineId,
                cancellationToken)
            ?? throw new StlApiException(
                "backorder.purchase_order_line.not_found",
                "Purchase order line was not found.",
                404);

        if (!string.Equals(
                line.PurchaseOrder.Status,
                PurchaseOrderStatuses.Issued,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "backorder.purchase_order.not_issued",
                "Backorders can only be recorded against issued purchase orders.",
                409);
        }

        var remaining = ComputeRemaining(line);
        if (remaining <= 0)
        {
            throw new StlApiException(
                "backorder.purchase_order_line.fully_received",
                "Purchase order line has no remaining quantity to backorder.",
                409);
        }

        var quantity = request.QuantityBackordered is null
            ? remaining
            : NormalizeQuantity(request.QuantityBackordered.Value);

        if (quantity > remaining)
        {
            throw new StlApiException(
                "backorder.quantity.exceeds_remaining",
                "Backorder quantity cannot exceed the remaining purchase order quantity.",
                400);
        }

        var backorderKey = NormalizeBackorderKey(request.BackorderKey);
        await EnsureUniqueKeyAsync(tenantId, backorderKey, cancellationToken);

        var existingOpen = await FindOpenForLineAsync(tenantId, line.Id, cancellationToken);
        if (existingOpen is not null)
        {
            throw new StlApiException(
                "backorder.open_exists",
                "An open backorder already exists for this purchase order line.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = BuildBackorder(
            tenantId,
            actorUserId,
            backorderKey,
            BackorderSourceTypes.PurchaseOrderLine,
            line,
            quantity,
            request.ExpectedBy,
            NormalizeNotes(request.Notes ?? string.Empty),
            receivingReceiptId: null,
            receivingReceiptLineId: null,
            now);

        db.Backorders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "backorder.create_from_po_line",
            tenantId,
            actorUserId,
            "backorder",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task SyncAfterReceivingPostAsync(
        Guid tenantId,
        Guid actorUserId,
        ReceivingReceipt receipt,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var receiptKey = receipt.ReceiptKey;

        foreach (var receiptLine in receipt.Lines)
        {
            var poLine = receiptLine.PurchaseOrderLine;
            var remaining = ComputeRemaining(poLine);
            var existingOpen = await FindOpenForLineAsync(tenantId, poLine.Id, cancellationToken);

            if (remaining <= 0)
            {
                if (existingOpen is not null)
                {
                    existingOpen.Status = BackorderStatuses.Fulfilled;
                    existingOpen.QuantityFulfilled = existingOpen.QuantityBackordered;
                    existingOpen.FulfilledByUserId = actorUserId;
                    existingOpen.FulfilledAt = now;
                    existingOpen.UpdatedAt = now;
                }

                continue;
            }

            if (existingOpen is not null)
            {
                existingOpen.QuantityBackordered = remaining;
                existingOpen.QuantityFulfilled = 0;
                existingOpen.ReceivingReceiptId = receipt.Id;
                existingOpen.ReceivingReceiptLineId = receiptLine.Id;
                existingOpen.SourceType = BackorderSourceTypes.ReceiptPost;
                existingOpen.UpdatedAt = now;
                continue;
            }

            var backorderKey = $"bo-{receiptKey}-ln{receiptLine.LineNumber}";
            var duplicateKey = await db.Backorders.AnyAsync(
                x => x.TenantId == tenantId && x.BackorderKey == backorderKey,
                cancellationToken);
            if (duplicateKey)
            {
                backorderKey = $"bo-{receiptKey}-ln{receiptLine.LineNumber}-{Guid.NewGuid():N}"[..128];
            }

            var entity = BuildBackorder(
                tenantId,
                actorUserId,
                backorderKey,
                BackorderSourceTypes.ReceiptPost,
                poLine,
                remaining,
                expectedBy: null,
                notes: $"Auto-created from posted receipt {receiptKey}.",
                receipt.Id,
                receiptLine.Id,
                now);

            db.Backorders.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<BackorderResponse> FulfillAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid backorderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, backorderId, cancellationToken);
        EnsureOpen(entity);

        var now = DateTimeOffset.UtcNow;
        entity.Status = BackorderStatuses.Fulfilled;
        entity.QuantityFulfilled = entity.QuantityBackordered;
        entity.FulfilledByUserId = actorUserId;
        entity.FulfilledAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "backorder.fulfill",
            tenantId,
            actorUserId,
            "backorder",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<BackorderResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid backorderId,
        CancelBackorderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, backorderId, cancellationToken);
        EnsureOpen(entity);

        var now = DateTimeOffset.UtcNow;
        entity.Status = BackorderStatuses.Cancelled;
        entity.CancelledByUserId = actorUserId;
        entity.CancelledAt = now;
        entity.CancellationReason = NormalizeCancellationReason(request.Reason);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "backorder.cancel",
            tenantId,
            actorUserId,
            "backorder",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private static Backorder BuildBackorder(
        Guid tenantId,
        Guid actorUserId,
        string backorderKey,
        string sourceType,
        PurchaseOrderLine line,
        decimal quantity,
        DateTimeOffset? expectedBy,
        string notes,
        Guid? receivingReceiptId,
        Guid? receivingReceiptLineId,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BackorderKey = backorderKey,
            Status = BackorderStatuses.Open,
            SourceType = sourceType,
            PurchaseOrderId = line.PurchaseOrderId,
            PurchaseOrderLineId = line.Id,
            PurchaseRequestId = line.PurchaseOrder.PurchaseRequestId,
            PurchaseRequestLineId = line.PurchaseRequestLineId,
            ReceivingReceiptId = receivingReceiptId,
            ReceivingReceiptLineId = receivingReceiptLineId,
            PartId = line.PartId,
            QuantityBackordered = quantity,
            QuantityFulfilled = 0,
            ExpectedBy = expectedBy,
            Notes = notes,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
            PurchaseOrder = line.PurchaseOrder,
            PurchaseOrderLine = line,
            Part = line.Part
        };

    private async Task<Backorder?> FindOpenForLineAsync(
        Guid tenantId,
        Guid purchaseOrderLineId,
        CancellationToken cancellationToken) =>
        await db.Backorders.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.PurchaseOrderLineId == purchaseOrderLineId
                && x.Status == BackorderStatuses.Open,
            cancellationToken);

    private static decimal ComputeRemaining(PurchaseOrderLine line)
    {
        var remaining = line.QuantityOrdered - line.QuantityReceived;
        return remaining < 0 ? 0 : decimal.Round(remaining, 4, MidpointRounding.AwayFromZero);
    }

    private static void EnsureOpen(Backorder entity)
    {
        if (!string.Equals(entity.Status, BackorderStatuses.Open, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "backorder.not_open",
                "Only open backorders can be updated.",
                409);
        }
    }

    private async Task EnsureUniqueKeyAsync(
        Guid tenantId,
        string backorderKey,
        CancellationToken cancellationToken)
    {
        var duplicate = await db.Backorders.AnyAsync(
            x => x.TenantId == tenantId && x.BackorderKey == backorderKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "backorder.duplicate",
                "A backorder with this key already exists.",
                409);
        }
    }

    private async Task<Backorder> LoadAsync(
        Guid tenantId,
        Guid backorderId,
        CancellationToken cancellationToken)
    {
        return await db.Backorders
            .AsNoTracking()
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.PurchaseRequest)
            .Include(x => x.PurchaseOrderLine)
            .Include(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == backorderId,
                cancellationToken)
            ?? throw new StlApiException(
                "backorder.not_found",
                "Backorder was not found.",
                404);
    }

    private async Task<Backorder> LoadTrackedAsync(
        Guid tenantId,
        Guid backorderId,
        CancellationToken cancellationToken)
    {
        return await db.Backorders
            .Include(x => x.PurchaseOrder)
                .ThenInclude(x => x.PurchaseRequest)
            .Include(x => x.PurchaseOrderLine)
            .Include(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == backorderId,
                cancellationToken)
            ?? throw new StlApiException(
                "backorder.not_found",
                "Backorder was not found.",
                404);
    }

    private static BackorderResponse Map(Backorder entity)
    {
        var open = entity.QuantityBackordered - entity.QuantityFulfilled;
        if (open < 0)
        {
            open = 0;
        }

        var purchaseRequestKey = entity.PurchaseRequestId is null
            ? null
            : entity.PurchaseOrder.PurchaseRequest?.RequestKey;

        return new(
            entity.Id,
            entity.BackorderKey,
            entity.Status,
            entity.SourceType,
            entity.PurchaseOrderId,
            entity.PurchaseOrder.OrderKey,
            entity.PurchaseOrderLineId,
            entity.PurchaseOrderLine.LineNumber,
            entity.PurchaseRequestId,
            purchaseRequestKey,
            entity.PurchaseRequestLineId,
            entity.ReceivingReceiptId,
            null,
            entity.ReceivingReceiptLineId,
            entity.PartId,
            entity.Part.PartKey,
            entity.Part.DisplayName,
            entity.QuantityBackordered,
            entity.QuantityFulfilled,
            open,
            entity.ExpectedBy,
            entity.Notes,
            entity.CreatedByUserId,
            entity.FulfilledByUserId,
            entity.FulfilledAt,
            entity.CancelledByUserId,
            entity.CancelledAt,
            entity.CancellationReason,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static string NormalizeBackorderKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException(
                "backorder.key.required",
                "Backorder key is required.",
                400);
        }

        if (key.Length > 128)
        {
            throw new StlApiException(
                "backorder.key.too_long",
                "Backorder key must be 128 characters or fewer.",
                400);
        }

        return key;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "backorder.quantity.invalid",
                "Backorder quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeNotes(string value)
    {
        var notes = (value ?? string.Empty).Trim();
        return notes.Length > 1024 ? notes[..1024] : notes;
    }

    private static string NormalizeCancellationReason(string value)
    {
        var reason = (value ?? string.Empty).Trim();
        if (reason.Length == 0)
        {
            throw new StlApiException(
                "backorder.cancel.reason_required",
                "Cancellation reason is required.",
                400);
        }

        return reason.Length > 512 ? reason[..512] : reason;
    }
}
