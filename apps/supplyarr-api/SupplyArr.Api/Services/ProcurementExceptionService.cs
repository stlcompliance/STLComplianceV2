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
    public async Task<IReadOnlyList<ProcurementExceptionResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        string? subjectType = null,
        Guid? subjectId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ProcurementExceptions.AsNoTracking().Where(x => x.TenantId == tenantId);

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

        var rows = await query
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
        entity.ResolutionNotes = ProcurementExceptionRules.NormalizeResolutionNotes(request.ResolutionNotes);
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

    private sealed record SubjectSnapshot(string SubjectKey, Guid? VendorPartyId);

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
            entity.WaiveRequestedByUserId,
            entity.WaiveRequestedAt,
            entity.WaivedByUserId,
            entity.WaivedAt,
            entity.ResolvedAt,
            entity.ClosedAt,
            entity.CreatedAt,
            entity.UpdatedAt);
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

        return entities.Select(entity =>
        {
            vendors.TryGetValue(entity.VendorPartyId ?? Guid.Empty, out var vendor);
            return new ProcurementExceptionResponse(
                entity.Id,
                entity.ExceptionKey,
                entity.SubjectType,
                entity.SubjectId,
                entity.SubjectKey,
                entity.VendorPartyId,
                vendor?.PartyKey,
                vendor?.DisplayName,
                entity.ExceptionCategory,
                entity.Title,
                entity.Description,
                entity.Status,
                entity.ResolutionNotes,
                entity.WaiveJustification,
                entity.WaiveRejectionReason,
                entity.CreatedByUserId,
                entity.AssignedToUserId,
                entity.WaiveRequestedByUserId,
                entity.WaiveRequestedAt,
                entity.WaivedByUserId,
                entity.WaivedAt,
                entity.ResolvedAt,
                entity.ClosedAt,
                entity.CreatedAt,
                entity.UpdatedAt);
        }).ToList();
    }
}
