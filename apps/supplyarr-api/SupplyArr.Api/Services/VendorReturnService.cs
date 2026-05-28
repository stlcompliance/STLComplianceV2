using Microsoft.EntityFrameworkCore;

using SupplyArr.Api.Contracts;

using SupplyArr.Api.Data;

using SupplyArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace SupplyArr.Api.Services;



public sealed class VendorReturnService(

    SupplyArrDbContext db,

    PartStockService stock,

    ISupplyArrAuditService audit)

{

    public async Task<IReadOnlyList<VendorReturnResponse>> ListAsync(

        Guid tenantId,

        string? status = null,

        Guid? vendorPartyId = null,

        Guid? purchaseOrderId = null,

        Guid? partId = null,

        CancellationToken cancellationToken = default)

    {

        var query = db.VendorReturns

            .AsNoTracking()

            .Include(x => x.VendorParty)

            .Include(x => x.PurchaseOrder)

                .ThenInclude(x => x!.PurchaseRequest)

            .Include(x => x.InventoryBin)

                .ThenInclude(x => x!.InventoryLocation)

            .Include(x => x.Lines)

                .ThenInclude(x => x.Part)

            .Include(x => x.Lines)

                .ThenInclude(x => x.PurchaseOrderLine)

            .Where(x => x.TenantId == tenantId);



        if (!string.IsNullOrWhiteSpace(status))

        {

            var normalizedStatus = status.Trim().ToLowerInvariant();

            query = query.Where(x => x.Status == normalizedStatus);

        }



        if (vendorPartyId is not null)

        {

            query = query.Where(x => x.VendorPartyId == vendorPartyId);

        }



        if (purchaseOrderId is not null)

        {

            query = query.Where(x => x.PurchaseOrderId == purchaseOrderId);

        }



        if (partId is not null)

        {

            query = query.Where(x => x.Lines.Any(line => line.PartId == partId));

        }



        var items = await query

            .OrderByDescending(x => x.UpdatedAt)

            .ToListAsync(cancellationToken);



        return items.Select(Map).ToList();

    }



    public async Task<VendorReturnResponse> GetAsync(

        Guid tenantId,

        Guid returnId,

        CancellationToken cancellationToken = default)

    {

        var entity = await LoadAsync(tenantId, returnId, cancellationToken);

        return Map(entity);

    }



    public async Task<VendorReturnResponse> CreateFromStockAsync(

        Guid tenantId,

        Guid actorUserId,

        CreateVendorReturnFromStockRequest request,

        CancellationToken cancellationToken = default)

    {

        if (request.Lines is null || request.Lines.Count == 0)

        {

            throw new StlApiException(

                "return.lines.required",

                "At least one return line is required.",

                400);

        }



        var vendor = await LoadVendorAsync(tenantId, request.VendorPartyId, cancellationToken);

        var bin = await LoadActiveBinAsync(tenantId, request.InventoryBinId, cancellationToken);

        var returnKey = NormalizeReturnKey(request.ReturnKey);

        await EnsureUniqueKeyAsync(tenantId, returnKey, cancellationToken);



        var now = DateTimeOffset.UtcNow;

        var entity = new VendorReturn

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            ReturnKey = returnKey,

            Status = VendorReturnStatuses.Draft,

            SourceType = VendorReturnSourceTypes.Stock,

            VendorPartyId = vendor.Id,

            PurchaseOrderId = null,

            InventoryBinId = bin.Id,

            RmaNumber = NormalizeRmaNumber(request.RmaNumber ?? string.Empty),

            Notes = NormalizeNotes(request.Notes ?? string.Empty),

            CreatedByUserId = actorUserId,

            CreatedAt = now,

            UpdatedAt = now,

            VendorParty = vendor,

            InventoryBin = bin

        };



        var lineNumber = 1;

        foreach (var lineRequest in request.Lines)

        {

            var part = await LoadPartAsync(tenantId, lineRequest.PartId, cancellationToken);

            var quantity = NormalizeQuantity(lineRequest.Quantity);



            entity.Lines.Add(new VendorReturnLine

            {

                Id = Guid.NewGuid(),

                TenantId = tenantId,

                VendorReturnId = entity.Id,

                LineNumber = lineNumber++,

                PartId = part.Id,

                PurchaseOrderLineId = null,

                Quantity = quantity,

                Notes = NormalizeLineNotes(lineRequest.Notes ?? string.Empty),

                CreatedAt = now,

                UpdatedAt = now,

                Part = part

            });

        }



        db.VendorReturns.Add(entity);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "vendor_return.create_from_stock",

            tenantId,

            actorUserId,

            "vendor_return",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<VendorReturnResponse> CreateFromPurchaseOrderLineAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid purchaseOrderLineId,

        CreateVendorReturnFromPurchaseOrderLineRequest request,

        CancellationToken cancellationToken = default)

    {

        var line = await db.PurchaseOrderLines

            .Include(x => x.PurchaseOrder)

                .ThenInclude(x => x.PurchaseRequest)

            .Include(x => x.Part)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == purchaseOrderLineId,

                cancellationToken)

            ?? throw new StlApiException(

                "return.purchase_order_line.not_found",

                "Purchase order line was not found.",

                404);



        if (!string.Equals(

                line.PurchaseOrder.Status,

                PurchaseOrderStatuses.Issued,

                StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "return.purchase_order.not_issued",

                "Returns can only be recorded against issued purchase orders.",

                409);

        }



        if (line.QuantityReceived <= 0)

        {

            throw new StlApiException(

                "return.purchase_order_line.not_received",

                "Purchase order line has no received quantity to return.",

                409);

        }



        var bin = await LoadActiveBinAsync(tenantId, request.InventoryBinId, cancellationToken);

        var returnKey = NormalizeReturnKey(request.ReturnKey);

        await EnsureUniqueKeyAsync(tenantId, returnKey, cancellationToken);



        var quantity = request.Quantity is null

            ? line.QuantityReceived

            : NormalizeQuantity(request.Quantity.Value);



        if (quantity > line.QuantityReceived)

        {

            throw new StlApiException(

                "return.quantity.exceeds_received",

                "Return quantity cannot exceed the received purchase order quantity.",

                400);

        }



        var available = await GetAvailableOnHandAsync(

            tenantId,

            line.PartId,

            bin.Id,

            cancellationToken);

        if (quantity > available)

        {

            throw new StlApiException(

                "return.quantity.exceeds_stock",

                "Return quantity exceeds available stock in the selected bin.",

                400);

        }



        var now = DateTimeOffset.UtcNow;

        var entity = new VendorReturn

        {

            Id = Guid.NewGuid(),

            TenantId = tenantId,

            ReturnKey = returnKey,

            Status = VendorReturnStatuses.Draft,

            SourceType = VendorReturnSourceTypes.PurchaseOrderLine,

            VendorPartyId = line.PurchaseOrder.VendorPartyId,

            PurchaseOrderId = line.PurchaseOrderId,

            InventoryBinId = bin.Id,

            RmaNumber = NormalizeRmaNumber(request.RmaNumber ?? string.Empty),

            Notes = NormalizeNotes(request.Notes ?? string.Empty),

            CreatedByUserId = actorUserId,

            CreatedAt = now,

            UpdatedAt = now,

            VendorParty = await LoadVendorAsync(tenantId, line.PurchaseOrder.VendorPartyId, cancellationToken),

            PurchaseOrder = line.PurchaseOrder,

            InventoryBin = bin,

            Lines =

            [

                new VendorReturnLine

                {

                    Id = Guid.NewGuid(),

                    TenantId = tenantId,

                    VendorReturnId = default,

                    LineNumber = 1,

                    PartId = line.PartId,

                    PurchaseOrderLineId = line.Id,

                    Quantity = quantity,

                    Notes = string.Empty,

                    CreatedAt = now,

                    UpdatedAt = now,

                    Part = line.Part,

                    PurchaseOrderLine = line

                }

            ]

        };

        entity.Lines[0].VendorReturnId = entity.Id;



        db.VendorReturns.Add(entity);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "vendor_return.create_from_po_line",

            tenantId,

            actorUserId,

            "vendor_return",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<VendorReturnResponse> PostAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid returnId,

        CancellationToken cancellationToken = default)

    {

        var entity = await LoadTrackedAsync(tenantId, returnId, cancellationToken);

        EnsureDraft(entity);



        if (entity.Lines.Count == 0)

        {

            throw new StlApiException(

                "return.lines.required",

                "At least one return line is required before posting.",

                400);

        }



        foreach (var line in entity.Lines)

        {

            if (line.Quantity <= 0)

            {

                throw new StlApiException(

                    "return.line.quantity_invalid",

                    "Each return line must have a quantity greater than zero.",

                    400);

            }



            if (line.PurchaseOrderLineId is not null)

            {

                var received = line.PurchaseOrderLine!.QuantityReceived;

                if (line.Quantity > received)

                {

                    throw new StlApiException(

                        "return.quantity.exceeds_received",

                        "Return quantity cannot exceed the received purchase order quantity.",

                        400);

                }

            }



            await stock.DecrementOnHandAsync(

                tenantId,

                actorUserId,

                line.PartId,

                entity.InventoryBinId,

                line.Quantity,

                cancellationToken);

        }



        var now = DateTimeOffset.UtcNow;

        entity.Status = VendorReturnStatuses.Posted;

        entity.PostedAt = now;

        entity.PostedByUserId = actorUserId;

        entity.UpdatedAt = now;



        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "vendor_return.post",

            tenantId,

            actorUserId,

            "vendor_return",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    public async Task<VendorReturnResponse> CancelAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid returnId,

        CancelVendorReturnRequest request,

        CancellationToken cancellationToken = default)

    {

        var entity = await LoadTrackedAsync(tenantId, returnId, cancellationToken);

        EnsureDraft(entity);



        var now = DateTimeOffset.UtcNow;

        entity.Status = VendorReturnStatuses.Cancelled;

        entity.CancelledByUserId = actorUserId;

        entity.CancelledAt = now;

        entity.CancellationReason = NormalizeCancellationReason(request.Reason);

        entity.UpdatedAt = now;



        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "vendor_return.cancel",

            tenantId,

            actorUserId,

            "vendor_return",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return await GetAsync(tenantId, entity.Id, cancellationToken);

    }



    private async Task<decimal> GetAvailableOnHandAsync(

        Guid tenantId,

        Guid partId,

        Guid binId,

        CancellationToken cancellationToken)

    {

        var level = await db.PartStockLevels.AsNoTracking().FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.PartId == partId && x.InventoryBinId == binId,

            cancellationToken);



        if (level is null)

        {

            return 0;

        }



        var available = level.QuantityOnHand - level.QuantityReserved;

        return available < 0 ? 0 : decimal.Round(available, 4, MidpointRounding.AwayFromZero);

    }



    private async Task<ExternalParty> LoadVendorAsync(

        Guid tenantId,

        Guid vendorPartyId,

        CancellationToken cancellationToken) =>

        await db.ExternalParties.FirstOrDefaultAsync(

            x => x.TenantId == tenantId

                && x.Id == vendorPartyId

                && x.PartyType == "vendor",

            cancellationToken)

        ?? throw new StlApiException(

            "return.vendor.not_found",

            "Vendor was not found.",

            404);



    private async Task<Part> LoadPartAsync(

        Guid tenantId,

        Guid partId,

        CancellationToken cancellationToken) =>

        await db.Parts.FirstOrDefaultAsync(

            x => x.TenantId == tenantId && x.Id == partId,

            cancellationToken)

        ?? throw new StlApiException("parts.not_found", "Part was not found.", 404);



    private async Task<InventoryBin> LoadActiveBinAsync(

        Guid tenantId,

        Guid binId,

        CancellationToken cancellationToken)

    {

        var bin = await db.InventoryBins

            .Include(x => x.InventoryLocation)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == binId,

                cancellationToken)

            ?? throw new StlApiException(

                "return.bin.not_found",

                "Inventory bin was not found.",

                404);



        if (!string.Equals(bin.Status, "active", StringComparison.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "return.bin.inactive",

                "Returns cannot be sourced from an inactive bin.",

                400);

        }



        return bin;

    }



    private static void EnsureDraft(VendorReturn entity)

    {

        if (!VendorReturnStatuses.Editable.Contains(entity.Status))

        {

            throw new StlApiException(

                "return.not_editable",

                "Vendor return can only be edited while in draft status.",

                409);

        }

    }



    private async Task EnsureUniqueKeyAsync(

        Guid tenantId,

        string returnKey,

        CancellationToken cancellationToken)

    {

        var duplicate = await db.VendorReturns.AnyAsync(

            x => x.TenantId == tenantId && x.ReturnKey == returnKey,

            cancellationToken);

        if (duplicate)

        {

            throw new StlApiException(

                "return.duplicate",

                "A vendor return with this key already exists.",

                409);

        }

    }



    private async Task<VendorReturn> LoadAsync(

        Guid tenantId,

        Guid returnId,

        CancellationToken cancellationToken) =>

        await db.VendorReturns

            .AsNoTracking()

            .Include(x => x.VendorParty)

            .Include(x => x.PurchaseOrder)

                .ThenInclude(x => x!.PurchaseRequest)

            .Include(x => x.InventoryBin)

                .ThenInclude(x => x!.InventoryLocation)

            .Include(x => x.Lines)

                .ThenInclude(x => x.Part)

            .Include(x => x.Lines)

                .ThenInclude(x => x.PurchaseOrderLine)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == returnId,

                cancellationToken)

        ?? throw new StlApiException(

            "return.not_found",

            "Vendor return was not found.",

            404);



    private async Task<VendorReturn> LoadTrackedAsync(

        Guid tenantId,

        Guid returnId,

        CancellationToken cancellationToken) =>

        await db.VendorReturns

            .Include(x => x.VendorParty)

            .Include(x => x.PurchaseOrder)

                .ThenInclude(x => x!.PurchaseRequest)

            .Include(x => x.InventoryBin)

                .ThenInclude(x => x!.InventoryLocation)

            .Include(x => x.Lines)

                .ThenInclude(x => x.Part)

            .Include(x => x.Lines)

                .ThenInclude(x => x.PurchaseOrderLine)

            .FirstOrDefaultAsync(

                x => x.TenantId == tenantId && x.Id == returnId,

                cancellationToken)

        ?? throw new StlApiException(

            "return.not_found",

            "Vendor return was not found.",

            404);



    private static VendorReturnResponse Map(VendorReturn entity)

    {

        var purchaseRequestKey = entity.PurchaseOrder?.PurchaseRequest?.RequestKey;

        var purchaseRequestId = entity.PurchaseOrder?.PurchaseRequestId;



        return new(

            entity.Id,

            entity.ReturnKey,

            entity.Status,

            entity.SourceType,

            entity.VendorPartyId,

            entity.VendorParty.PartyKey,

            entity.VendorParty.DisplayName,

            entity.PurchaseOrderId,

            entity.PurchaseOrder?.OrderKey,

            purchaseRequestId,

            purchaseRequestKey,

            entity.InventoryBinId,

            entity.InventoryBin.BinKey,

            entity.InventoryBin.Name,

            entity.InventoryBin.InventoryLocationId,

            entity.InventoryBin.InventoryLocation?.LocationKey ?? string.Empty,

            entity.InventoryBin.InventoryLocation?.Name ?? string.Empty,

            entity.RmaNumber,

            entity.Notes,

            entity.CreatedByUserId,

            entity.PostedByUserId,

            entity.PostedAt,

            entity.CancelledByUserId,

            entity.CancelledAt,

            entity.CancellationReason,

            entity.Lines

                .OrderBy(x => x.LineNumber)

                .Select(MapLine)

                .ToList(),

            entity.CreatedAt,

            entity.UpdatedAt);

    }



    private static VendorReturnLineResponse MapLine(VendorReturnLine line) =>

        new(

            line.Id,

            line.LineNumber,

            line.PartId,

            line.Part.PartKey,

            line.Part.DisplayName,

            line.PurchaseOrderLineId,

            line.PurchaseOrderLine?.LineNumber,

            line.Quantity,

            line.Notes,

            line.CreatedAt,

            line.UpdatedAt);



    private static string NormalizeReturnKey(string value)

    {

        var key = (value ?? string.Empty).Trim().ToLowerInvariant();

        if (key.Length == 0)

        {

            throw new StlApiException(

                "return.key.required",

                "Return key is required.",

                400);

        }



        if (key.Length > 128)

        {

            throw new StlApiException(

                "return.key.too_long",

                "Return key must be 128 characters or fewer.",

                400);

        }



        return key;

    }



    private static string NormalizeRmaNumber(string value)

    {

        var rma = (value ?? string.Empty).Trim();

        return rma.Length > 128 ? rma[..128] : rma;

    }



    private static string NormalizeNotes(string value)

    {

        var notes = (value ?? string.Empty).Trim();

        return notes.Length > 1024 ? notes[..1024] : notes;

    }



    private static string NormalizeLineNotes(string value)

    {

        var notes = (value ?? string.Empty).Trim();

        return notes.Length > 512 ? notes[..512] : notes;

    }



    private static decimal NormalizeQuantity(decimal quantity)

    {

        if (quantity <= 0)

        {

            throw new StlApiException(

                "return.quantity.invalid",

                "Return quantity must be greater than zero.",

                400);

        }



        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);

    }



    private static string NormalizeCancellationReason(string value)

    {

        var reason = (value ?? string.Empty).Trim();

        if (reason.Length == 0)

        {

            throw new StlApiException(

                "return.cancel.reason_required",

                "Cancellation reason is required.",

                400);

        }



        return reason.Length > 512 ? reason[..512] : reason;

    }

}

