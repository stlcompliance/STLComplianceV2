using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ProcurementExceptionService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public IReadOnlyList<ProcurementExceptionResolutionTemplateResponse> ListResolutionTemplates() =>
        ProcurementExceptionResolutionTemplates.All
            .Select(x => new ProcurementExceptionResolutionTemplateResponse(
                x.TemplateKey,
                x.Label,
                x.DefaultResolutionNotes))
            .ToList();

    public async Task<IReadOnlyList<ProcurementExceptionResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        string? subjectType = null,
        Guid? subjectId = null,
        bool overdueOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.ProcurementExceptions.AsNoTracking().Where(x => x.TenantId == tenantId);
        var asOf = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(subjectType))
        {
            var normalized = ProcurementExceptionRules.NormalizeSubjectType(subjectType);
            query = query.Where(x => x.SubjectType == normalized);
        }

        if (subjectId is not null)
        {
            query = query.Where(x => x.SubjectId == subjectId);
        }

        if (overdueOnly)
        {
            query = query.Where(x =>
                x.SlaDueAt != null
                && x.SlaDueAt < asOf
                && (x.Status == ProcurementExceptionStatuses.Open
                    || x.Status == ProcurementExceptionStatuses.Investigating
                    || x.Status == ProcurementExceptionStatuses.WaivePending));
        }

        var rows = overdueOnly
            ? await query
                .OrderBy(x => x.SlaDueAt)
                .ThenByDescending(x => x.UpdatedAt)
                .Take(200)
                .ToListAsync(cancellationToken)
            : await query
                .OrderByDescending(x => x.UpdatedAt)
                .Take(200)
                .ToListAsync(cancellationToken);

        return await MapListAsync(tenantId, rows, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> GetAsync(
        Guid tenantId,
        Guid exceptionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, exceptionId, cancellationToken);
        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> CreateForSubjectAsync(
        Guid tenantId,
        Guid actorUserId,
        string subjectType,
        Guid subjectId,
        CreateProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedSubjectType = ProcurementExceptionRules.NormalizeSubjectType(subjectType);
        var subject = await ResolveSubjectAsync(tenantId, normalizedSubjectType, subjectId, cancellationToken);
        var exceptionKey = ProcurementExceptionRules.NormalizeExceptionKey(request.ExceptionKey);

        var duplicateKey = await db.ProcurementExceptions.AnyAsync(
            x => x.TenantId == tenantId && x.ExceptionKey == exceptionKey,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "procurement_exceptions.duplicate_key",
                "A procurement exception with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ProcurementException
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExceptionKey = exceptionKey,
            SubjectType = normalizedSubjectType,
            SubjectId = subjectId,
            SubjectKey = subject.SubjectKey,
            VendorPartyId = subject.VendorPartyId,
            ExceptionCategory = ProcurementExceptionRules.NormalizeCategory(request.ExceptionCategory),
            Title = ProcurementExceptionRules.NormalizeTitle(request.Title),
            Description = ProcurementExceptionRules.NormalizeDescription(request.Description),
            Status = ProcurementExceptionStatuses.Open,
            CreatedByUserId = actorUserId,
            AssignedToUserId = request.AssignedToUserId,
            SlaDueAt = ProcurementExceptionRules.NormalizeSlaDueAt(
                request.SlaDueAt,
                ProcurementExceptionRules.NormalizeCategory(request.ExceptionCategory),
                now),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ProcurementExceptions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.create",
            IntegrationOutboxEventKinds.ProcurementExceptionCreated,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception opened: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        UpdateProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        EnsureEditable(entity);

        entity.Title = ProcurementExceptionRules.NormalizeTitle(request.Title);
        entity.Description = ProcurementExceptionRules.NormalizeDescription(request.Description);
        entity.ExceptionCategory = ProcurementExceptionRules.NormalizeCategory(request.ExceptionCategory);
        entity.AssignedToUserId = request.AssignedToUserId;
        entity.SlaDueAt = ProcurementExceptionRules.NormalizeSlaDueAt(
            request.SlaDueAt,
            entity.ExceptionCategory,
            entity.CreatedAt);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.update",
            IntegrationOutboxEventKinds.ProcurementExceptionUpdated,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception updated: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> AssignAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        AssignProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ProcurementExceptionRules.EnsureAssignee(request.AssignedToUserId);

        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        EnsureEditable(entity);

        entity.AssignedToUserId = request.AssignedToUserId;
        if (request.SlaDueAt is not null)
        {
            entity.SlaDueAt = request.SlaDueAt;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.assign",
            IntegrationOutboxEventKinds.ProcurementExceptionUpdated,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception assigned: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> LinkActionsAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        LinkProcurementExceptionActionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        EnsureEditable(entity);

        if (request.LinkedPurchaseRequestId is Guid linkedPrId)
        {
            await EnsurePurchaseRequestExistsAsync(tenantId, linkedPrId, cancellationToken);
            entity.LinkedPurchaseRequestId = linkedPrId;
        }
        else
        {
            entity.LinkedPurchaseRequestId = null;
        }

        if (request.LinkedPurchaseOrderId is Guid linkedPoId)
        {
            await EnsurePurchaseOrderExistsAsync(tenantId, linkedPoId, cancellationToken);
            entity.LinkedPurchaseOrderId = linkedPoId;
        }
        else
        {
            entity.LinkedPurchaseOrderId = null;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.link_actions",
            IntegrationOutboxEventKinds.ProcurementExceptionUpdated,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception linked to PR/PO actions: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> StartInvestigationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        Transition(entity, ProcurementExceptionStatuses.Investigating);

        var now = DateTimeOffset.UtcNow;
        entity.AssignedToUserId ??= actorUserId;
        entity.InvestigatedByUserId = actorUserId;
        entity.InvestigatedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.investigate",
            IntegrationOutboxEventKinds.ProcurementExceptionInvestigating,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception investigation started: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> ResolveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        ResolveProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        Transition(entity, ProcurementExceptionStatuses.Resolved);

        var now = DateTimeOffset.UtcNow;
        var templateKey = string.IsNullOrWhiteSpace(request.ResolutionTemplateKey)
            ? string.Empty
            : ProcurementExceptionRules.NormalizeResolutionTemplateKey(request.ResolutionTemplateKey);
        entity.ResolutionTemplateKey = templateKey;
        entity.ResolutionNotes = ProcurementExceptionRules.BuildResolutionNotes(
            templateKey,
            request.ResolutionNotes);
        entity.ResolvedByUserId = actorUserId;
        entity.ResolvedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.resolve",
            IntegrationOutboxEventKinds.ProcurementExceptionResolved,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception resolved: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> RequestWaiveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        RequestProcurementExceptionWaiveRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        Transition(entity, ProcurementExceptionStatuses.WaivePending);

        var now = DateTimeOffset.UtcNow;
        entity.WaiveJustification = ProcurementExceptionRules.NormalizeWaiveJustification(request.WaiveJustification);
        entity.WaiveRequestedByUserId = actorUserId;
        entity.WaiveRequestedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.waive_requested",
            IntegrationOutboxEventKinds.ProcurementExceptionWaiveRequested,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception waive requested: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> ApproveWaiveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        if (!string.Equals(entity.Status, ProcurementExceptionStatuses.WaivePending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "procurement_exceptions.not_pending_waive",
                "Only exceptions pending waive approval can be waived.",
                409);
        }

        Transition(entity, ProcurementExceptionStatuses.Waived);

        var now = DateTimeOffset.UtcNow;
        entity.WaivedByUserId = actorUserId;
        entity.WaivedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.waive_approved",
            IntegrationOutboxEventKinds.ProcurementExceptionWaived,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception waived: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> RejectWaiveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        RejectProcurementExceptionWaiveRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        if (!string.Equals(entity.Status, ProcurementExceptionStatuses.WaivePending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "procurement_exceptions.not_pending_waive",
                "Only exceptions pending waive approval can have waive rejected.",
                409);
        }

        Transition(entity, ProcurementExceptionStatuses.Investigating);

        entity.WaiveRejectionReason = request.Reason.Trim()[..Math.Min(request.Reason.Trim().Length, 512)];
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.waive_rejected",
            IntegrationOutboxEventKinds.ProcurementExceptionWaiveRejected,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception waive rejected: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> CloseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        CloseProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        Transition(entity, ProcurementExceptionStatuses.Closed);

        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
        {
            entity.ResolutionNotes = ProcurementExceptionRules.NormalizeResolutionNotes(request.ResolutionNotes!);
        }

        entity.ClosedByUserId = actorUserId;
        entity.ClosedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.close",
            IntegrationOutboxEventKinds.ProcurementExceptionClosed,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception closed: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    public async Task<ProcurementExceptionResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid exceptionId,
        CancelProcurementExceptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, exceptionId, cancellationToken);
        Transition(entity, ProcurementExceptionStatuses.Cancelled);

        var now = DateTimeOffset.UtcNow;
        entity.CancellationReason = request.Reason.Trim()[..Math.Min(request.Reason.Trim().Length, 512)];
        entity.CancelledByUserId = actorUserId;
        entity.CancelledAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "procurement_exception.cancel",
            IntegrationOutboxEventKinds.ProcurementExceptionCancelled,
            tenantId,
            actorUserId,
            entity,
            $"Procurement exception cancelled: {entity.ExceptionKey}",
            cancellationToken);

        return await MapAsync(tenantId, entity, cancellationToken);
    }

    private async Task EnsurePurchaseRequestExistsAsync(
        Guid tenantId,
        Guid purchaseRequestId,
        CancellationToken cancellationToken)
    {
        var exists = await db.PurchaseRequests.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == purchaseRequestId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "procurement_exceptions.linked_pr_not_found",
                "Linked purchase request was not found.",
                404);
        }
    }

    private async Task EnsurePurchaseOrderExistsAsync(
        Guid tenantId,
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        var exists = await db.PurchaseOrders.AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.Id == purchaseOrderId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "procurement_exceptions.linked_po_not_found",
                "Linked purchase order was not found.",
                404);
        }
    }

    private sealed record SubjectSnapshot(string SubjectKey, Guid? VendorPartyId);

    private sealed record LinkedActionKeys(string? PurchaseRequestKey, string? PurchaseOrderKey);

    private async Task<SubjectSnapshot> ResolveSubjectAsync(
        Guid tenantId,
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        if (string.Equals(subjectType, ProcurementExceptionSubjectTypes.PurchaseRequest, StringComparison.OrdinalIgnoreCase))
        {
            var pr = await db.PurchaseRequests.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new StlApiException("procurement_exceptions.subject_not_found", "Purchase request was not found.", 404);
            return new SubjectSnapshot(pr.RequestKey, pr.VendorPartyId);
        }

        if (string.Equals(subjectType, ProcurementExceptionSubjectTypes.PurchaseOrder, StringComparison.OrdinalIgnoreCase))
        {
            var po = await db.PurchaseOrders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new StlApiException("procurement_exceptions.subject_not_found", "Purchase order was not found.", 404);
            return new SubjectSnapshot(po.OrderKey, po.VendorPartyId);
        }

        if (string.Equals(subjectType, ProcurementExceptionSubjectTypes.Rfq, StringComparison.OrdinalIgnoreCase))
        {
            var rfq = await db.Rfqs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == subjectId, cancellationToken)
                ?? throw new StlApiException("procurement_exceptions.subject_not_found", "RFQ was not found.", 404);
            return new SubjectSnapshot(rfq.RfqKey, rfq.AwardedVendorPartyId);
        }

        throw new StlApiException("procurement_exceptions.invalid_subject_type", "Subject type is not supported.", 400);
    }

    private static void Transition(ProcurementException entity, string targetStatus)
    {
        if (!ProcurementExceptionRules.CanTransition(entity.Status, targetStatus))
        {
            throw new StlApiException(
                "procurement_exceptions.invalid_transition",
                $"Cannot transition exception from {entity.Status} to {targetStatus}.",
                409);
        }

        entity.Status = targetStatus;
    }

    private static void EnsureEditable(ProcurementException entity)
    {
        if (!ProcurementExceptionStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException(
                "procurement_exceptions.not_editable",
                "Exception can only be edited while open or investigating.",
                409);
        }
    }

    private async Task WriteAuditAndOutboxAsync(
        string auditAction,
        string outboxKind,
        Guid tenantId,
        Guid actorUserId,
        ProcurementException entity,
        string summary,
        CancellationToken cancellationToken)
    {
        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "procurement_exception",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            outboxKind,
            "procurement_exception",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.VendorPartyId),
            cancellationToken: cancellationToken);
    }

    private async Task<ProcurementException> LoadAsync(
        Guid tenantId,
        Guid exceptionId,
        CancellationToken cancellationToken) =>
        await db.ProcurementExceptions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == exceptionId, cancellationToken)
            ?? throw new StlApiException("procurement_exceptions.not_found", "Procurement exception was not found.", 404);

    private async Task<ProcurementException> LoadTrackedAsync(
        Guid tenantId,
        Guid exceptionId,
        CancellationToken cancellationToken) =>
        await db.ProcurementExceptions
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == exceptionId, cancellationToken)
            ?? throw new StlApiException("procurement_exceptions.not_found", "Procurement exception was not found.", 404);

    private async Task<LinkedActionKeys> LoadLinkedActionKeysAsync(
        Guid tenantId,
        ProcurementException entity,
        CancellationToken cancellationToken)
    {
        string? prKey = null;
        string? poKey = null;

        if (entity.LinkedPurchaseRequestId is Guid prId)
        {
            prKey = await db.PurchaseRequests.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == prId)
                .Select(x => x.RequestKey)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (entity.LinkedPurchaseOrderId is Guid poId)
        {
            poKey = await db.PurchaseOrders.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Id == poId)
                .Select(x => x.OrderKey)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new LinkedActionKeys(prKey, poKey);
    }

    private static ProcurementExceptionResponse MapEntity(
        ProcurementException entity,
        string? vendorKey,
        string? vendorName,
        string? linkedPurchaseRequestKey,
        string? linkedPurchaseOrderKey)
    {
        var asOf = DateTimeOffset.UtcNow;
        return new ProcurementExceptionResponse(
            entity.Id,
            entity.ExceptionKey,
            entity.SubjectType,
            entity.SubjectId,
            entity.SubjectKey,
            entity.VendorPartyId,
            vendorKey,
            vendorName,
            entity.ExceptionCategory,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.ResolutionNotes,
            entity.WaiveJustification,
            entity.WaiveRejectionReason,
            entity.CreatedByUserId,
            entity.AssignedToUserId,
            entity.SlaDueAt,
            ProcurementExceptionRules.IsSlaBreached(entity, asOf),
            entity.ResolutionTemplateKey,
            entity.LinkedPurchaseRequestId,
            linkedPurchaseRequestKey,
            entity.LinkedPurchaseOrderId,
            linkedPurchaseOrderKey,
            entity.WaiveRequestedByUserId,
            entity.WaiveRequestedAt,
            entity.WaivedByUserId,
            entity.WaivedAt,
            entity.ResolvedAt,
            entity.ClosedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private async Task<ProcurementExceptionResponse> MapAsync(
        Guid tenantId,
        ProcurementException entity,
        CancellationToken cancellationToken)
    {
        string? vendorKey = null;
        string? vendorName = null;
        if (entity.VendorPartyId is Guid vendorId)
        {
            var vendor = await db.ExternalParties.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorId, cancellationToken);
            vendorKey = vendor?.PartyKey;
            vendorName = vendor?.DisplayName;
        }

        var linked = await LoadLinkedActionKeysAsync(tenantId, entity, cancellationToken);
        return MapEntity(entity, vendorKey, vendorName, linked.PurchaseRequestKey, linked.PurchaseOrderKey);
    }

    private async Task<IReadOnlyList<ProcurementExceptionResponse>> MapListAsync(
        Guid tenantId,
        IReadOnlyList<ProcurementException> entities,
        CancellationToken cancellationToken)
    {
        var vendorIds = entities.Where(x => x.VendorPartyId.HasValue).Select(x => x.VendorPartyId!.Value).Distinct().ToList();
        var vendors = vendorIds.Count == 0
            ? []
            : await db.ExternalParties.AsNoTracking()
                .Where(x => x.TenantId == tenantId && vendorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var linkedPrIds = entities.Where(x => x.LinkedPurchaseRequestId.HasValue)
            .Select(x => x.LinkedPurchaseRequestId!.Value)
            .Distinct()
            .ToList();
        var linkedPrs = linkedPrIds.Count == 0
            ? []
            : await db.PurchaseRequests.AsNoTracking()
                .Where(x => x.TenantId == tenantId && linkedPrIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.RequestKey, cancellationToken);

        var linkedPoIds = entities.Where(x => x.LinkedPurchaseOrderId.HasValue)
            .Select(x => x.LinkedPurchaseOrderId!.Value)
            .Distinct()
            .ToList();
        var linkedPos = linkedPoIds.Count == 0
            ? []
            : await db.PurchaseOrders.AsNoTracking()
                .Where(x => x.TenantId == tenantId && linkedPoIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.OrderKey, cancellationToken);

        return entities.Select(entity =>
        {
            vendors.TryGetValue(entity.VendorPartyId ?? Guid.Empty, out var vendor);
            linkedPrs.TryGetValue(entity.LinkedPurchaseRequestId ?? Guid.Empty, out var prKey);
            linkedPos.TryGetValue(entity.LinkedPurchaseOrderId ?? Guid.Empty, out var poKey);
            return MapEntity(
                entity,
                vendor?.PartyKey,
                vendor?.DisplayName,
                prKey,
                poKey);
        }).ToList();
    }
}
