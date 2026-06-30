using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public sealed class PurchaseOrderService(
    SupplyArrDbContext db,
    VendorProcurementGuardService vendorProcurementGuard,
    ComplianceCoreVendorUseGateClient complianceCoreVendorUseGate,
    StaffarrProcurementApprovalAuthorityService approvalAuthority,
    SupplyArrDemandStatusCallbackCoordinator demandStatusCallbacks,
    ProcurementNotificationEnqueueService notificationEnqueue,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PurchaseOrderResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.VendorParty)
                .ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.PurchaseRequest)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        var orders = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(Map).ToList();
    }

    public async Task<PurchaseOrderResponse> GetAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, purchaseOrderId, cancellationToken);
        return Map(entity);
    }

    public async Task<PurchaseOrderResponse> CreateFromPurchaseRequestAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        CreatePurchaseOrderFromPurchaseRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseRequest = await db.PurchaseRequests
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Include(x => x.VendorParty)
                .ThenInclude(x => x!.ParentExternalParty)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseRequestId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_orders.purchase_request.not_found",
                "Purchase request was not found.",
                404);

        if (!string.Equals(
                purchaseRequest.Status,
                PurchaseRequestStatuses.Approved,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "purchase_orders.purchase_request.not_approved",
                "Purchase orders can only be created from approved purchase requests.",
                409);
        }

        if (!purchaseRequest.VendorPartyId.HasValue)
        {
            throw new StlApiException(
                "purchase_orders.purchase_request.supplier_required",
                "Approved purchase request must have a supplier before creating a purchase order.",
                400);
        }

        await vendorProcurementGuard.EnsureVendorAllowedForScopeAsync(
            tenantId,
            purchaseRequest.VendorPartyId.Value,
            VendorRestrictionScopes.PurchaseOrders,
            cancellationToken);

        if (purchaseRequest.Lines.Count == 0)
        {
            throw new StlApiException(
                "purchase_orders.purchase_request.lines_required",
                "Approved purchase request must have line items.",
                400);
        }

        var existingOpen = await db.PurchaseOrders.AnyAsync(
            x => x.TenantId == tenantId
                && x.PurchaseRequestId == purchaseRequestId
                && PurchaseOrderStatuses.Open.Contains(x.Status),
            cancellationToken);
        if (existingOpen)
        {
            throw new StlApiException(
                "purchase_orders.purchase_request.already_linked",
                "An open purchase order already exists for this purchase request.",
                409);
        }

        var orderKey = NormalizeOrderKey(request.OrderKey);
        var duplicateKey = await db.PurchaseOrders.AnyAsync(
            x => x.TenantId == tenantId && x.OrderKey == orderKey,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "purchase_orders.duplicate",
                "A purchase order with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderKey = orderKey,
            Title = NormalizeTitle(request.Title ?? purchaseRequest.Title),
            Notes = NormalizeNotes(request.Notes ?? purchaseRequest.Notes),
            Status = PurchaseOrderStatuses.Draft,
            PurchaseRequestId = purchaseRequest.Id,
            VendorPartyId = purchaseRequest.VendorPartyId.Value,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        var lineNumber = 1;
        foreach (var prLine in purchaseRequest.Lines.OrderBy(x => x.LineNumber))
        {
            entity.Lines.Add(new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PurchaseOrderId = entity.Id,
                PurchaseRequestLineId = prLine.Id,
                LineNumber = lineNumber++,
                PartId = prLine.PartId,
                QuantityOrdered = prLine.QuantityRequested,
                UnitOfMeasure = prLine.UnitOfMeasure,
                Notes = prLine.Notes,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        db.PurchaseOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.create_from_pr",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await demandStatusCallbacks.NotifyPurchaseOrderCreatedAsync(
            tenantId,
            purchaseRequest.Id,
            entity.Id,
            now,
            cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        UpdatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        EnsureEditable(entity);

        entity.Title = NormalizeTitle(request.Title);
        entity.Notes = NormalizeNotes(request.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.update",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> AddLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        AddPurchaseOrderLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        EnsureEditable(entity);

        var now = DateTimeOffset.UtcNow;
        await AddLineInternalAsync(entity, request.PartId, request.QuantityOrdered, request.Notes, now, cancellationToken);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.line.add",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        Guid lineId,
        UpdatePurchaseOrderLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "purchase_orders.line.not_found",
                "Purchase order line was not found.",
                404);

        line.QuantityOrdered = NormalizeQuantity(request.QuantityOrdered);
        line.Notes = NormalizeNotes(request.Notes);
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.line.update",
            tenantId,
            actorUserId,
            "purchase_order_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> RemoveLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "purchase_orders.line.not_found",
                "Purchase order line was not found.",
                404);

        db.PurchaseOrderLines.Remove(line);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        RenumberLines(entity);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.line.remove",
            tenantId,
            actorUserId,
            "purchase_order_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        if (!PurchaseOrderStatusRules.CanTransition(entity.Status, PurchaseOrderStatuses.Approved))
        {
            throw new StlApiException(
                "purchase_orders.invalid_transition",
                "Only draft purchase orders can be approved.",
                409);
        }

        if (entity.Lines.Count == 0)
        {
            throw new StlApiException(
                "purchase_orders.lines.required",
                "At least one line item is required before approval.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseOrderStatuses.Approved;
        entity.ApprovedAt = now;
        entity.ApprovedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.approve",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseOrderResponse> IssueAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        if (!PurchaseOrderStatusRules.CanTransition(entity.Status, PurchaseOrderStatuses.Issued))
        {
            throw new StlApiException(
                "purchase_orders.invalid_transition",
                "Only approved purchase orders can be issued.",
                409);
        }

        await vendorProcurementGuard.EnsureVendorAllowedForScopeAsync(
            tenantId,
            entity.VendorPartyId,
            VendorRestrictionScopes.PurchaseOrders,
            cancellationToken);

        await approvalAuthority.EnsureCanIssuePurchaseOrderAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            entity,
            cancellationToken);

        await EnsureComplianceCoreVendorUseAllowedAsync(entity, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseOrderStatuses.Issued;
        entity.IssuedAt = now;
        entity.IssuedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.issue",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await demandStatusCallbacks.NotifyPurchaseOrderIssuedAsync(
            tenantId,
            entity.Id,
            now,
            cancellationToken);

        await notificationEnqueue.TryEnqueueAsync(
            tenantId,
            ProcurementNotificationEventKinds.PurchaseOrderIssued,
            entity.VendorPartyId,
            "purchase_order",
            entity.Id,
            cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.PurchaseOrderIssued,
            "purchase_order",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Purchase order issued: {entity.OrderKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private async Task EnsureComplianceCoreVendorUseAllowedAsync(
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken)
    {
        if (!complianceCoreVendorUseGate.IsConfigured)
        {
            return;
        }

        var context = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["product"] = "supplyarr",
            ["action"] = "purchase_order_issue",
            ["purchaseOrderId"] = purchaseOrder.Id.ToString("D"),
            ["purchase_order_id"] = purchaseOrder.Id.ToString("D"),
            ["purchaseOrderKey"] = purchaseOrder.OrderKey,
            ["purchase_order_key"] = purchaseOrder.OrderKey,
            ["vendorPartyId"] = purchaseOrder.VendorPartyId.ToString("D"),
            ["vendor_party_id"] = purchaseOrder.VendorPartyId.ToString("D"),
            ["vendorPartyKey"] = purchaseOrder.VendorParty.PartyKey,
            ["vendor_party_key"] = purchaseOrder.VendorParty.PartyKey,
        };

        context["purchaseRequestId"] = purchaseOrder.PurchaseRequestId.ToString("D");
        context["purchase_request_id"] = purchaseOrder.PurchaseRequestId.ToString("D");

        var partIds = purchaseOrder.Lines
            .Select(line => line.PartId.ToString("D"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (partIds.Count > 0)
        {
            context["partIds"] = string.Join(",", partIds);
            context["part_ids"] = string.Join(",", partIds);
        }

        var result = await complianceCoreVendorUseGate.CheckVendorUseAsync(
            purchaseOrder.TenantId,
            purchaseOrder.VendorPartyId,
            purchaseOrder.VendorParty.DisplayName,
            context,
            cancellationToken);

        if (result is null || IsPermissiveComplianceCoreGateOutcome(result.Outcome))
        {
            return;
        }

        throw new StlApiException(
            "purchase_orders.compliancecore_vendor_gate_blocked",
            result.Message,
            409,
            new Dictionary<string, object?>
            {
                ["outcome"] = result.Outcome,
                ["reasonCode"] = result.ReasonCode,
                ["checkResultId"] = result.CheckResultId,
                ["traceId"] = result.TraceId,
                ["appliedWaiverId"] = result.AppliedWaiverId,
                ["appliedWaiverKey"] = result.AppliedWaiverKey,
            });
    }

    private static bool IsPermissiveComplianceCoreGateOutcome(string outcome) =>
        string.Equals(outcome, "allow", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "warn", StringComparison.OrdinalIgnoreCase)
        || string.Equals(outcome, "waived", StringComparison.OrdinalIgnoreCase);

    public async Task<PurchaseOrderResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseOrderId,
        CancelPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseOrderId, cancellationToken);
        if (!PurchaseOrderStatusRules.CanTransition(entity.Status, PurchaseOrderStatuses.Cancelled))
        {
            throw new StlApiException(
                "purchase_orders.invalid_transition",
                "Only draft or approved purchase orders can be cancelled.",
                409);
        }

        var reason = NormalizeCancellationReason(request.Reason);
        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseOrderStatuses.Cancelled;
        entity.CancelledAt = now;
        entity.CancelledByUserId = actorUserId;
        entity.CancellationReason = reason;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_order.cancel",
            tenantId,
            actorUserId,
            "purchase_order",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: "cancelled",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private async Task AddLineInternalAsync(
        PurchaseOrder entity,
        Guid partId,
        decimal quantityOrdered,
        string notes,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var part = await db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == entity.TenantId && x.Id == partId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_orders.part.not_found",
                "Part was not found.",
                404);

        var lineNumber = entity.Lines.Count == 0
            ? 1
            : entity.Lines.Max(x => x.LineNumber) + 1;

        entity.Lines.Add(new PurchaseOrderLine
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            PurchaseOrderId = entity.Id,
            LineNumber = lineNumber,
            PartId = part.Id,
            QuantityOrdered = NormalizeQuantity(quantityOrdered),
            UnitOfMeasure = part.UnitOfMeasure,
            Notes = NormalizeNotes(notes),
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private static void RenumberLines(PurchaseOrder entity)
    {
        var lineNumber = 1;
        foreach (var line in entity.Lines.OrderBy(x => x.LineNumber))
        {
            line.LineNumber = lineNumber++;
        }
    }

    private static void EnsureEditable(PurchaseOrder entity)
    {
        if (!PurchaseOrderStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException(
                "purchase_orders.not_editable",
                "Purchase order can only be edited while in draft status.",
                409);
        }
    }

    private async Task<PurchaseOrder> LoadAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.VendorParty)
                .ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.PurchaseRequest)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseOrderId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_orders.not_found",
                "Purchase order was not found.",
                404);
    }

    private async Task<PurchaseOrder> LoadTrackedAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseOrders
            .Include(x => x.VendorParty)
                .ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.PurchaseRequest)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseOrderId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_orders.not_found",
                "Purchase order was not found.",
                404);
    }

    private static PurchaseOrderResponse Map(PurchaseOrder entity) =>
        new(
            entity.Id,
            entity.OrderKey,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.PurchaseRequestId,
            entity.PurchaseRequest.RequestKey,
            entity.VendorPartyId,
            entity.VendorParty.PartyKey,
            entity.VendorParty.DisplayName,
            entity.VendorParty.ParentExternalPartyId,
            entity.VendorParty.ParentExternalParty?.DisplayName,
            entity.VendorParty.UnitKind,
            ParseServiceTypes(entity.VendorParty.ServiceTypesJson),
            entity.VendorPartyId,
            entity.VendorParty.PartyKey,
            entity.VendorParty.DisplayName,
            entity.CreatedByUserId,
            entity.ApprovedAt,
            entity.ApprovedByUserId,
            entity.IssuedAt,
            entity.IssuedByUserId,
            entity.CancelledAt,
            entity.CancelledByUserId,
            entity.CancellationReason,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(MapLine)
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static PurchaseOrderLineResponse MapLine(PurchaseOrderLine line)
    {
        var remaining = line.QuantityOrdered - line.QuantityReceived;
        if (remaining < 0)
        {
            remaining = 0;
        }

        return new(
            line.Id,
            line.LineNumber,
            line.PurchaseRequestLineId,
            line.PartId,
            line.Part.PartKey,
            line.Part.DisplayName,
            line.QuantityOrdered,
            line.QuantityReceived,
            remaining,
            line.UnitOfMeasure,
            line.Notes,
            line.CreatedAt,
            line.UpdatedAt);
    }

    private static string NormalizeOrderKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException(
                "purchase_orders.order_key.required",
                "Order key is required.",
                400);
        }

        if (key.Length > 128)
        {
            throw new StlApiException(
                "purchase_orders.order_key.too_long",
                "Order key must be 128 characters or fewer.",
                400);
        }

        return key;
    }

    private static string NormalizeTitle(string value)
    {
        var title = (value ?? string.Empty).Trim();
        if (title.Length == 0)
        {
            throw new StlApiException(
                "purchase_orders.title.required",
                "Title is required.",
                400);
        }

        return title.Length > 256 ? title[..256] : title;
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
                "purchase_orders.cancellation_reason.required",
                "Cancellation reason is required.",
                400);
        }

        return reason.Length > 512 ? reason[..512] : reason;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "purchase_orders.quantity.invalid",
                "Quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }
}
