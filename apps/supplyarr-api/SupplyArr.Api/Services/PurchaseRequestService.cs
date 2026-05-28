using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PurchaseRequestService(
    SupplyArrDbContext db,
    VendorProcurementGuardService vendorProcurementGuard,
    StaffarrProcurementApprovalAuthorityService approvalAuthority,
    SupplyArrDemandStatusCallbackCoordinator demandStatusCallbacks,
    ProcurementNotificationEnqueueService notificationEnqueue,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<PurchaseRequestResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        var requests = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(Map).ToList();
    }

    public async Task<PurchaseRequestResponse> GetAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, purchaseRequestId, cancellationToken);
        return Map(entity);
    }

    public async Task<PurchaseRequestResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePurchaseRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestKey = NormalizeRequestKey(request.RequestKey);
        var exists = await db.PurchaseRequests.AnyAsync(
            x => x.TenantId == tenantId && x.RequestKey == requestKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "purchase_requests.duplicate",
                "A purchase request with this key already exists.",
                409);
        }

        if (request.VendorPartyId.HasValue)
        {
            await EnsureVendorPartyAsync(tenantId, request.VendorPartyId.Value, cancellationToken);
        }

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
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PurchaseRequests.Add(entity);

        if (request.Lines is { Count: > 0 })
        {
            foreach (var lineRequest in request.Lines)
            {
                await AddLineInternalAsync(entity, lineRequest, now, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.create",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        UpdatePurchaseRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        EnsureEditable(entity);

        if (request.VendorPartyId.HasValue)
        {
            await EnsureVendorPartyAsync(tenantId, request.VendorPartyId.Value, cancellationToken);
        }

        entity.Title = NormalizeTitle(request.Title);
        entity.Notes = NormalizeNotes(request.Notes);
        entity.VendorPartyId = request.VendorPartyId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.update",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> AddLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        AddPurchaseRequestLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        EnsureEditable(entity);

        var now = DateTimeOffset.UtcNow;
        await AddLineInternalAsync(
            entity,
            new CreatePurchaseRequestLineRequest(request.PartId, request.QuantityRequested, request.Notes),
            now,
            cancellationToken);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.line.add",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> UpdateLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        Guid lineId,
        UpdatePurchaseRequestLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "purchase_requests.line.not_found",
                "Purchase request line was not found.",
                404);

        line.QuantityRequested = NormalizeQuantity(request.QuantityRequested);
        line.Notes = NormalizeNotes(request.Notes);
        line.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedAt = line.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.line.update",
            tenantId,
            actorUserId,
            "purchase_request_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> RemoveLineAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        Guid lineId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        EnsureEditable(entity);

        var line = entity.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new StlApiException(
                "purchase_requests.line.not_found",
                "Purchase request line was not found.",
                404);

        db.PurchaseRequestLines.Remove(line);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        RenumberLines(entity);

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.line.remove",
            tenantId,
            actorUserId,
            "purchase_request_line",
            line.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!PurchaseRequestStatusRules.CanTransition(entity.Status, PurchaseRequestStatuses.Submitted))
        {
            throw new StlApiException(
                "purchase_requests.invalid_transition",
                "Only draft purchase requests can be submitted.",
                409);
        }

        if (entity.Lines.Count == 0)
        {
            throw new StlApiException(
                "purchase_requests.lines.required",
                "At least one line item is required before submission.",
                400);
        }

        await approvalAuthority.EnsureCanSubmitPurchaseRequestAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            entity,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseRequestStatuses.Submitted;
        entity.SubmittedAt = now;
        entity.SubmittedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.submit",
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
            IntegrationOutboxEventKinds.PurchaseRequestSubmitted,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Purchase request submitted: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!PurchaseRequestStatusRules.CanTransition(entity.Status, PurchaseRequestStatuses.Approved))
        {
            throw new StlApiException(
                "purchase_requests.invalid_transition",
                "Only submitted purchase requests can be approved.",
                409);
        }

        await approvalAuthority.EnsureCanApprovePurchaseRequestAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            entity,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseRequestStatuses.Approved;
        entity.ApprovedAt = now;
        entity.ApprovedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.approve",
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
            IntegrationOutboxEventKinds.PurchaseRequestApproved,
            "purchase_request",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Purchase request approved: {entity.RequestKey}", entity.VendorPartyId),
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    public async Task<PurchaseRequestResponse> RejectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid purchaseRequestId,
        RejectPurchaseRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, purchaseRequestId, cancellationToken);
        if (!PurchaseRequestStatusRules.CanTransition(entity.Status, PurchaseRequestStatuses.Rejected))
        {
            throw new StlApiException(
                "purchase_requests.invalid_transition",
                "Only submitted purchase requests can be rejected.",
                409);
        }

        var reason = NormalizeRejectionReason(request.Reason);
        var now = DateTimeOffset.UtcNow;
        entity.Status = PurchaseRequestStatuses.Rejected;
        entity.RejectedAt = now;
        entity.RejectedByUserId = actorUserId;
        entity.RejectionReason = reason;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "purchase_request.reject",
            tenantId,
            actorUserId,
            "purchase_request",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: "rejected",
            cancellationToken: cancellationToken);

        await demandStatusCallbacks.NotifyPurchaseRequestRejectedAsync(
            tenantId,
            entity.Id,
            reason,
            now,
            cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

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
                "purchase_requests.part.not_found",
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
            UpdatedAt = now
        });
    }

    private static void RenumberLines(PurchaseRequest entity)
    {
        var lineNumber = 1;
        foreach (var line in entity.Lines.OrderBy(x => x.LineNumber))
        {
            line.LineNumber = lineNumber++;
        }
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

    private static void EnsureEditable(PurchaseRequest entity)
    {
        if (!PurchaseRequestStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException(
                "purchase_requests.not_editable",
                "Purchase request can only be edited while in draft status.",
                409);
        }
    }

    private async Task<PurchaseRequest> LoadAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseRequestId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_requests.not_found",
                "Purchase request was not found.",
                404);
    }

    private async Task<PurchaseRequest> LoadTrackedAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        return await db.PurchaseRequests
            .Include(x => x.VendorParty)
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == purchaseRequestId,
                cancellationToken)
            ?? throw new StlApiException(
                "purchase_requests.not_found",
                "Purchase request was not found.",
                404);
    }

    private static PurchaseRequestResponse Map(PurchaseRequest entity) =>
        new(
            entity.Id,
            entity.RequestKey,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.VendorPartyId,
            entity.VendorParty?.PartyKey,
            entity.VendorParty?.DisplayName,
            entity.RequestedByUserId,
            entity.SubmittedAt,
            entity.SubmittedByUserId,
            entity.ApprovedAt,
            entity.ApprovedByUserId,
            entity.RejectedAt,
            entity.RejectedByUserId,
            entity.RejectionReason,
            entity.IsEmergency,
            entity.EmergencyReason,
            entity.EmergencyExpeditedAt,
            entity.ManagerOverrideApproved,
            entity.ManagerOverrideJustification,
            entity.ManagerOverrideApprovedAt,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(MapLine)
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

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

    private static string NormalizeRequestKey(string value)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (key.Length == 0)
        {
            throw new StlApiException(
                "purchase_requests.request_key.required",
                "Request key is required.",
                400);
        }

        if (key.Length > 128)
        {
            throw new StlApiException(
                "purchase_requests.request_key.too_long",
                "Request key must be 128 characters or fewer.",
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
                "purchase_requests.title.required",
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

    private static string NormalizeRejectionReason(string value)
    {
        var reason = (value ?? string.Empty).Trim();
        if (reason.Length == 0)
        {
            throw new StlApiException(
                "purchase_requests.rejection_reason.required",
                "Rejection reason is required.",
                400);
        }

        return reason.Length > 512 ? reason[..512] : reason;
    }

    private static decimal NormalizeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(
                "purchase_requests.quantity.invalid",
                "Quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }
}
