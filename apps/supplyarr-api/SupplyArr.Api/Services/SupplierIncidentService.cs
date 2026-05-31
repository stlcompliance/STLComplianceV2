using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplierIncidentService(
    SupplyArrDbContext db,
    VendorRestrictionService vendorRestrictions,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<IReadOnlyList<SupplierIncidentResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? externalPartyId = null,
        string? severity = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.SupplierIncidents
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        if (externalPartyId is not null)
        {
            query = query.Where(x => x.ExternalPartyId == externalPartyId);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(x => x.Severity == severity.Trim().ToLowerInvariant());
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<SupplierIncidentResponse>> ListByPartyAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIncidentPartyAsync(tenantId, externalPartyId, cancellationToken);
        return await ListAsync(tenantId, externalPartyId: externalPartyId, cancellationToken: cancellationToken);
    }

    public async Task<SupplierIncidentResponse> GetAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, incidentId, cancellationToken);
        return Map(entity);
    }

    public async Task<SupplierIncidentResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var party = await EnsureIncidentPartyAsync(tenantId, request.ExternalPartyId, cancellationToken);
        var incidentKey = SupplierIncidentRules.NormalizeIncidentKey(request.IncidentKey);

        var duplicateKey = await db.SupplierIncidents.AnyAsync(
            x => x.TenantId == tenantId && x.IncidentKey == incidentKey,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "supplier_incidents.duplicate_key",
                "An incident with this key already exists.",
                409);
        }

        await ValidateRelatedEntitiesAsync(tenantId, request, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new SupplierIncident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalPartyId = party.Id,
            IncidentKey = incidentKey,
            Title = SupplierIncidentRules.NormalizeTitle(request.Title),
            Description = SupplierIncidentRules.NormalizeDescription(request.Description),
            IncidentType = SupplierIncidentRules.NormalizeIncidentType(request.IncidentType),
            Severity = SupplierIncidentRules.NormalizeSeverity(request.Severity),
            Status = SupplierIncidentStatuses.Open,
            PurchaseRequestId = request.PurchaseRequestId,
            PurchaseOrderId = request.PurchaseOrderId,
            ReceivingReceiptId = request.ReceivingReceiptId,
            ReceivingExceptionId = request.ReceivingExceptionId,
            ReportedByUserId = actorUserId,
            AssignedToUserId = request.AssignedToUserId,
            InvolvedStaffarrPersonId = request.InvolvedStaffarrPersonId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.SupplierIncidents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.create",
            IntegrationOutboxEventKinds.SupplierIncidentCreated,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident opened: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        UpdateSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        EnsureEditable(entity);

        entity.Title = SupplierIncidentRules.NormalizeTitle(request.Title);
        entity.Description = SupplierIncidentRules.NormalizeDescription(request.Description);
        entity.IncidentType = SupplierIncidentRules.NormalizeIncidentType(request.IncidentType);
        entity.Severity = SupplierIncidentRules.NormalizeSeverity(request.Severity);
        entity.AssignedToUserId = request.AssignedToUserId;
        entity.InvolvedStaffarrPersonId = request.InvolvedStaffarrPersonId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.update",
            IntegrationOutboxEventKinds.SupplierIncidentUpdated,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident updated: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> StartInvestigationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        Transition(entity, SupplierIncidentStatuses.Investigating);
        entity.AssignedToUserId ??= actorUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.investigate",
            IntegrationOutboxEventKinds.SupplierIncidentInvestigating,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident investigation started: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> ResolveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        ResolveSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        Transition(entity, SupplierIncidentStatuses.Resolved);

        var now = DateTimeOffset.UtcNow;
        entity.ResolutionNotes = SupplierIncidentRules.NormalizeResolutionNotes(request.ResolutionNotes);
        entity.ResolvedByUserId = actorUserId;
        entity.ResolvedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.resolve",
            IntegrationOutboxEventKinds.SupplierIncidentResolved,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident resolved: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> CloseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CloseSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        Transition(entity, SupplierIncidentStatuses.Closed);

        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
        {
            entity.ResolutionNotes = SupplierIncidentRules.NormalizeResolutionNotes(request.ResolutionNotes!);
        }

        entity.ClosedByUserId = actorUserId;
        entity.ClosedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.close",
            IntegrationOutboxEventKinds.SupplierIncidentClosed,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident closed: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> CancelAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CancelSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        if (!SupplierIncidentRules.CanTransition(entity.Status, SupplierIncidentStatuses.Cancelled))
        {
            throw new StlApiException(
                "supplier_incidents.invalid_transition",
                "Incident cannot be cancelled from its current status.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = SupplierIncidentStatuses.Cancelled;
        entity.CancellationReason = SupplierIncidentRules.NormalizeCancellationReason(request.Reason);
        entity.CancelledByUserId = actorUserId;
        entity.CancelledAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.cancel",
            IntegrationOutboxEventKinds.SupplierIncidentCancelled,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident cancelled: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> ReopenAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        ReopenSupplierIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        if (!string.Equals(entity.Status, SupplierIncidentStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_incidents.not_cancelled",
                "Only cancelled supplier incidents can be reopened.",
                409);
        }

        Transition(entity, SupplierIncidentStatuses.Investigating);

        var now = DateTimeOffset.UtcNow;
        entity.LastReopenReason = SupplierIncidentRules.NormalizeReopenReason(request.Reason);
        entity.ReopenedByUserId = actorUserId;
        entity.ReopenedAt = now;
        entity.ReopenCount += 1;
        entity.AssignedToUserId ??= actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.reopen",
            IntegrationOutboxEventKinds.SupplierIncidentReopened,
            tenantId,
            actorUserId,
            entity,
            $"Supplier incident reopened: {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierIncidentResponse> ApplyProcurementRestrictionAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        ApplySupplierIncidentProcurementRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, incidentId, cancellationToken);
        if (!SupplierIncidentStatuses.Active.Contains(entity.Status))
        {
            throw new StlApiException(
                "supplier_incidents.not_active",
                "Procurement restrictions can only be applied to open, investigating, or resolved incidents.",
                409);
        }

        if (entity.VendorRestrictionId is not null)
        {
            throw new StlApiException(
                "supplier_incidents.restriction_already_applied",
                "A procurement restriction was already applied for this incident.",
                409);
        }

        var reason = string.IsNullOrWhiteSpace(request.Reason)
            ? $"Supplier incident {entity.IncidentKey}: {entity.Title}"
            : request.Reason.Trim();

        var restriction = await vendorRestrictions.CreateAsync(
            tenantId,
            actorUserId,
            entity.ExternalPartyId,
            new CreateVendorRestrictionRequest(
                request.RestrictionKey,
                request.Scopes,
                reason,
                null,
                null),
            cancellationToken);

        entity.VendorRestrictionId = restriction.RestrictionId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await WriteAuditAndOutboxAsync(
            "supplier_incident.apply_restriction",
            IntegrationOutboxEventKinds.SupplierIncidentRestrictionApplied,
            tenantId,
            actorUserId,
            entity,
            $"Procurement restriction applied from incident {entity.IncidentKey}",
            cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    private async Task ValidateRelatedEntitiesAsync(
        Guid tenantId,
        CreateSupplierIncidentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PurchaseRequestId is Guid prId
            && !await db.PurchaseRequests.AnyAsync(x => x.TenantId == tenantId && x.Id == prId, cancellationToken))
        {
            throw new StlApiException("supplier_incidents.purchase_request_not_found", "Purchase request was not found.", 404);
        }

        if (request.PurchaseOrderId is Guid poId
            && !await db.PurchaseOrders.AnyAsync(x => x.TenantId == tenantId && x.Id == poId, cancellationToken))
        {
            throw new StlApiException("supplier_incidents.purchase_order_not_found", "Purchase order was not found.", 404);
        }

        if (request.ReceivingReceiptId is Guid receiptId
            && !await db.ReceivingReceipts.AnyAsync(x => x.TenantId == tenantId && x.Id == receiptId, cancellationToken))
        {
            throw new StlApiException("supplier_incidents.receiving_receipt_not_found", "Receiving receipt was not found.", 404);
        }

        if (request.ReceivingExceptionId is Guid exceptionId
            && !await db.ReceivingExceptions.AnyAsync(x => x.TenantId == tenantId && x.Id == exceptionId, cancellationToken))
        {
            throw new StlApiException("supplier_incidents.receiving_exception_not_found", "Receiving exception was not found.", 404);
        }
    }

    private static void Transition(SupplierIncident entity, string targetStatus)
    {
        if (!SupplierIncidentRules.CanTransition(entity.Status, targetStatus))
        {
            throw new StlApiException(
                "supplier_incidents.invalid_transition",
                $"Cannot transition incident from {entity.Status} to {targetStatus}.",
                409);
        }

        entity.Status = targetStatus;
    }

    private static void EnsureEditable(SupplierIncident entity)
    {
        if (!SupplierIncidentStatuses.Editable.Contains(entity.Status))
        {
            throw new StlApiException(
                "supplier_incidents.not_editable",
                "Incident can only be edited while open or investigating.",
                409);
        }
    }

    private async Task<ExternalParty> EnsureIncidentPartyAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken)
    {
        var party = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == externalPartyId, cancellationToken)
            ?? throw new StlApiException("supplier_incidents.party_not_found", "Party was not found.", 404);

        if (!VendorRestrictionPartyTypes.Allowed.Contains(party.PartyType))
        {
            throw new StlApiException(
                "supplier_incidents.party_type_not_allowed",
                "Incidents apply only to vendor or supplier parties.",
                400);
        }

        return party;
    }

    private async Task WriteAuditAndOutboxAsync(
        string auditAction,
        string outboxKind,
        Guid tenantId,
        Guid actorUserId,
        SupplierIncident entity,
        string summary,
        CancellationToken cancellationToken)
    {
        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "supplier_incident",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            outboxKind,
            "supplier_incident",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.ExternalPartyId),
            cancellationToken: cancellationToken);
    }

    private async Task<SupplierIncident> LoadAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken) =>
        await db.SupplierIncidents
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken)
            ?? throw new StlApiException("supplier_incidents.not_found", "Supplier incident was not found.", 404);

    private async Task<SupplierIncident> LoadTrackedAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken) =>
        await db.SupplierIncidents
            .Include(x => x.ExternalParty)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken)
            ?? throw new StlApiException("supplier_incidents.not_found", "Supplier incident was not found.", 404);

    private static SupplierIncidentResponse Map(SupplierIncident entity) =>
        new(
            entity.Id,
            entity.ExternalPartyId,
            entity.ExternalParty.PartyKey,
            entity.ExternalParty.DisplayName,
            entity.ExternalParty.PartyType,
            entity.IncidentKey,
            entity.Title,
            entity.Description,
            entity.IncidentType,
            entity.Severity,
            entity.Status,
            entity.PurchaseRequestId,
            entity.PurchaseOrderId,
            entity.ReceivingReceiptId,
            entity.ReceivingExceptionId,
            entity.VendorRestrictionId,
            entity.ReportedByUserId,
            entity.AssignedToUserId,
            entity.InvolvedStaffarrPersonId,
            entity.StaffarrPersonnelIncidentId,
            entity.StaffarrIncidentRoutedAt,
            entity.StaffarrIncidentRouteStatus,
            entity.TrainarrIncidentRemediationId,
            entity.TrainarrIncidentRoutedAt,
            entity.TrainarrIncidentRouteStatus,
            entity.ResolutionNotes,
            entity.ResolvedByUserId,
            entity.ResolvedAt,
            entity.ClosedByUserId,
            entity.ClosedAt,
            entity.CancellationReason,
            entity.CancelledByUserId,
            entity.CancelledAt,
            entity.ReopenedByUserId,
            entity.ReopenedAt,
            entity.LastReopenReason,
            entity.ReopenCount,
            entity.CreatedAt,
            entity.UpdatedAt);
}
