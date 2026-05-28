using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class EmergencyPurchaseService(
    SupplyArrDbContext db,
    VendorProcurementGuardService vendorProcurementGuard,
    PurchaseOrderService purchaseOrderService,
    SupplyArrDemandStatusCallbackCoordinator demandStatusCallbacks,
    ProcurementNotificationEnqueueService notificationEnqueue,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<EmergencyPurchaseResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId && x.IsEmergency);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalized);
        }

        var rows = await query.OrderByDescending(x => x.UpdatedAt).ToListAsync(cancellationToken);
        return await MapListAsync(tenantId, rows, cancellationToken);
    }

    public async Task<IReadOnlyList<EmergencyPurchaseResponse>> ListPendingOverrideAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default) =>
        await ListAsync(tenantId, PurchaseRequestStatuses.Submitted, cancellationToken);

    public async Task<EmergencyPurchaseResponse> GetAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEmergencyAsync(tenantId, purchaseRequestId, cancellationToken);
        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<EmergencyPurchaseResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateEmergencyPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestKey = NormalizeRequestKey(request.RequestKey);
        var exists = await db.PurchaseRequests.AnyAsync(
            x => x.TenantId == tenantId && x.RequestKey == requestKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "emergency_purchase.duplicate",
                "A purchase request with this key already exists.",
                409);
        }

        if (request.Lines is not { Count: > 0 })
        {
            throw new StlApiException(
                "emergency_purchase.lines.required",
                "At least one line item is required for an emergency purchase.",
                400);
        }

        await EnsureVendorPartyAsync(tenantId, request.VendorPartyId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestKey = requestKey,
            Title = NormalizeTitle(request.Title),
            Notes = NormalizeNotes(request.Notes),
            Status = PurchaseRequestStatuses.Draft,
            VendorPartyId = request.VendorPartyId,
            RequestedByUserId = actorUserId,
            IsEmergency = true,
            EmergencyReason = NormalizeEmergencyReason(request.EmergencyReason),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PurchaseRequests.Add(entity);

        foreach (var lineRequest in request.Lines)
        {
            await AddLineInternalAsync(entity, lineRequest, now, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "emergency_purchase.create",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.EmergencyPurchaseCreated,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Emergency purchase created: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await MapAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<EmergencyPurchaseResponse> ExpeditedSubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        ExpeditedSubmitEmergencyPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEmergencyTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!PurchaseRequestStatusRules.CanTransition(entity.Status, PurchaseRequestStatuses.Submitted))
        {
            throw new StlApiException(
                "emergency_purchase.invalid_transition",
                "Only draft emergency purchases can be expedited for review.",
                409);
        }

        if (entity.Lines.Count == 0)
        {
            throw new StlApiException(
                "emergency_purchase.lines.required",
                "At least one line item is required before expedited submit.",
                400);
        }

        if (!entity.VendorPartyId.HasValue)
        {
            throw new StlApiException(
                "emergency_purchase.vendor.required",
                "Emergency purchase must have a vendor before expedited submit.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseRequestStatuses.Submitted;
        entity.SubmittedAt = now;
        entity.SubmittedByUserId = actorUserId;
        entity.EmergencyExpeditedAt = now;
        entity.EmergencyExpeditedByUserId = actorUserId;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            entity.Notes = NormalizeNotes($"{entity.Notes}\nExpedited: {request.Notes.Trim()}".Trim());
        }

        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "emergency_purchase.expedited_submit",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await demandStatusCallbacks.NotifyPurchaseRequestSubmittedAsync(
            tenantId,
            entity.Id,
            now,
            cancellationToken);

        await notificationEnqueue.TryEnqueueAsync(
            tenantId,
            ProcurementNotificationEventKinds.PurchaseRequestSubmitted,
            entity.VendorPartyId,
            "purchase_request",
            entity.Id,
            cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.EmergencyPurchaseExpeditedSubmitted,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Emergency purchase expedited: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await MapAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<EmergencyPurchaseResponse> ManagerOverrideApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        ManagerOverrideApproveEmergencyPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEmergencyTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!entity.IsEmergency)
        {
            throw new StlApiException(
                "emergency_purchase.not_emergency",
                "Manager override approval applies only to emergency purchases.",
                409);
        }

        if (!PurchaseRequestStatusRules.CanTransition(entity.Status, PurchaseRequestStatuses.Approved))
        {
            throw new StlApiException(
                "emergency_purchase.invalid_transition",
                "Only submitted emergency purchases can receive manager override approval.",
                409);
        }

        var justification = NormalizeJustification(request.Justification);
        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseRequestStatuses.Approved;
        entity.ApprovedAt = now;
        entity.ApprovedByUserId = actorUserId;
        entity.ManagerOverrideApproved = true;
        entity.ManagerOverrideJustification = justification;
        entity.ManagerOverrideApprovedByUserId = actorUserId;
        entity.ManagerOverrideApprovedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "emergency_purchase.manager_override_approve",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await demandStatusCallbacks.NotifyPurchaseRequestApprovedAsync(
            tenantId,
            entity.Id,
            now,
            cancellationToken);

        await notificationEnqueue.TryEnqueueAsync(
            tenantId,
            ProcurementNotificationEventKinds.PurchaseRequestApproved,
            entity.VendorPartyId,
            "purchase_request",
            entity.Id,
            cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.EmergencyPurchaseManagerOverrideApproved,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Emergency purchase manager override approved: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.PurchaseRequestApproved,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Purchase request approved: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await MapAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<IssueEmergencyPurchaseOrderResponse> IssuePurchaseOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        IssueEmergencyPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadEmergencyAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!string.Equals(entity.Status, PurchaseRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "emergency_purchase.not_approved",
                "Emergency purchase must be manager-override approved before issuing a purchase order.",
                409);
        }

        if (!entity.ManagerOverrideApproved)
        {
            throw new StlApiException(
                "emergency_purchase.override.required",
                "Purchase order issue requires manager override approval on emergency purchases.",
                409);
        }

        var purchaseOrder = await purchaseOrderService.CreateFromPurchaseRequestAsync(
            tenantId,
            actorUserId,
            purchaseRequestId,
            new CreatePurchaseOrderFromPurchaseRequestRequest(
                request.OrderKey,
                request.Title ?? entity.Title,
                request.Notes ?? entity.Notes),
            cancellationToken);

        await audit.WriteAsync(
            "emergency_purchase.issue_purchase_order",
            tenantId,
            actorUserId,
            "purchase_order",
            purchaseOrder.PurchaseOrderId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.EmergencyPurchaseOrderIssued,
            "purchase_order",
            purchaseOrder.PurchaseOrderId,
            new IntegrationOutboxPayload(
                tenantId,
                $"Emergency purchase order issued: {purchaseOrder.OrderKey}",
                purchaseOrder.VendorPartyId),
            cancellationToken: cancellationToken);

        var emergency = await MapAsync(tenantId, purchaseRequestId, cancellationToken);
        return new IssueEmergencyPurchaseOrderResponse(
            purchaseRequestId,
            purchaseOrder.PurchaseOrderId,
            emergency,
            purchaseOrder);
    }

    private async Task<IReadOnlyList<EmergencyPurchaseResponse>> MapListAsync(
        Guid tenantId,
        IReadOnlyList<PurchaseRequest> entities,
        CancellationToken cancellationToken)
    {
        var results = new List<EmergencyPurchaseResponse>();
        foreach (var entity in entities)
        {
            results.Add(await MapAsync(tenantId, entity, cancellationToken));
        }

        return results;
    }

    private async Task<EmergencyPurchaseResponse> MapAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        var entity = await LoadEmergencyAsync(tenantId, purchaseRequestId, cancellationToken);
        return await MapAsync(tenantId, entity, cancellationToken);
    }

    private async Task<EmergencyPurchaseResponse> MapAsync(
        Guid tenantId,
        PurchaseRequest entity,
        CancellationToken cancellationToken)
    {
        var linkedOrder = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PurchaseRequestId == entity.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new EmergencyPurchaseResponse(
            entity.Id,
            entity.RequestKey,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.VendorPartyId,
            entity.VendorParty?.PartyKey,
            entity.VendorParty?.DisplayName,
            entity.EmergencyReason,
            entity.EmergencyExpeditedAt,
            entity.ManagerOverrideApproved,
            entity.ManagerOverrideJustification,
            entity.ManagerOverrideApprovedAt,
            linkedOrder?.Id,
            linkedOrder?.OrderKey,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(MapLine)
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static PurchaseRequestLineResponse MapLine(PurchaseRequestLine line) =>
        new(
            line.Id,
            line.LineNumber,
            line.PartId,
            line.Part.PartKey,
            line.Part.DisplayName,
            line.QuantityRequested,
            line.UnitOfMeasure,
            line.Notes,
            line.CreatedAt,
            line.UpdatedAt);

    private async Task AddLineInternalAsync(
        PurchaseRequest entity,
        CreatePurchaseRequestLineRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var part = await db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == entity.TenantId && x.Id == request.PartId,
                cancellationToken)
            ?? throw new StlApiException(
                "emergency_purchase.part.not_found",
                "Part was not found.",
                404);

        var lineNumber = entity.Lines.Count == 0
            ? 1
            : entity.Lines.Max(x => x.LineNumber) + 1;

        entity.Lines.Add(new PurchaseRequestLine
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            PurchaseRequestId = entity.Id,
            LineNumber = lineNumber,
            PartId = part.Id,
            QuantityRequested = NormalizeQuantity(request.QuantityRequested),
            UnitOfMeasure = part.UnitOfMeasure,
            Notes = NormalizeNotes(request.Notes),
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    private Task EnsureVendorPartyAsync(
        Guid tenantId,
        Guid vendorPartyId,
        CancellationToken cancellationToken) =>
        vendorProcurementGuard.EnsureVendorAllowedForScopeAsync(
            tenantId,
            vendorPartyId,
            VendorRestrictionScopes.PurchaseRequests,
            cancellationToken);

    private async Task<PurchaseRequest> LoadEmergencyAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        var entity = await db.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseRequestId && x.IsEmergency,
                cancellationToken)
            ?? throw new StlApiException(
                "emergency_purchase.not_found",
                "Emergency purchase was not found.",
                404);

        return entity;
    }

    private async Task<PurchaseRequest> LoadEmergencyTrackedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        var entity = await db.PurchaseRequests
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseRequestId && x.IsEmergency,
                cancellationToken)
            ?? throw new StlApiException(
                "emergency_purchase.not_found",
                "Emergency purchase was not found.",
                404);

        return entity;
    }

    private static string NormalizeRequestKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException("emergency_purchase.request_key.required", "Request key is required.", 400);
        }

        return key.Length > 128 ? key[..128] : key;
    }

    private static string NormalizeTitle(string value)
    {
        var title = (value ?? string.Empty).Trim();
        if (title.Length == 0)
        {
            throw new StlApiException("emergency_purchase.title.required", "Title is required.", 400);
        }

        return title.Length > 256 ? title[..256] : title;
    }

    private static string NormalizeNotes(string value)
    {
        var notes = (value ?? string.Empty).Trim();
        return notes.Length > 1024 ? notes[..1024] : notes;
    }

    private static string NormalizeEmergencyReason(string value)
    {
        var reason = (value ?? string.Empty).Trim();
        if (reason.Length == 0)
        {
            throw new StlApiException(
                "emergency_purchase.reason.required",
                "Emergency reason is required.",
                400);
        }

        return reason.Length > 512 ? reason[..512] : reason;
    }

    private static string NormalizeJustification(string value)
    {
        var justification = (value ?? string.Empty).Trim();
        if (justification.Length == 0)
        {
            throw new StlApiException(
                "emergency_purchase.justification.required",
                "Manager override justification is required.",
                400);
        }

        return justification.Length > 512 ? justification[..512] : justification;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "emergency_purchase.quantity.invalid",
                "Quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }
}
