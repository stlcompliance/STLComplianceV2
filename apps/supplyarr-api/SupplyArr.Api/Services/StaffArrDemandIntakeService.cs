using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class StaffArrDemandIntakeService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    StaffArrDemandStatusCallbackService demandStatusCallbacks,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<StaffarrDemandIntakeResponse> IngestAsync(
        IngestStaffarrDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateIngestRequest(request);

        var existing = await db.StaffArrDemandRefs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.StaffarrPublicationId == request.StaffarrPublicationId,
                cancellationToken);

        if (existing is not null)
        {
            return new StaffarrDemandIntakeResponse(
                existing.Id,
                existing.Status,
                existing.PurchaseRequestId,
                existing.Status == StaffArrDemandRefStatuses.PrDrafted,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new StaffArrDemandRef
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrPublicationId = request.StaffarrPublicationId,
            StaffarrIncidentId = request.StaffarrIncidentId,
            StaffarrIncidentTitle = request.StaffarrIncidentTitle.Trim(),
            StaffarrPersonId = request.StaffarrPersonId,
            Title = request.Title.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = StaffArrDemandRefStatuses.Received,
            ReceivedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.StaffArrDemandRefs.Add(entity);

        var lineNumber = 1;
        foreach (var lineRequest in request.Lines)
        {
            var partId = lineRequest.SupplyarrPartId;
            if (partId.HasValue)
            {
                await EnsurePartExistsAsync(request.TenantId, partId.Value, cancellationToken);
            }

            entity.Lines.Add(new StaffArrDemandRefLine
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                DemandRefId = entity.Id,
                LineNumber = lineNumber++,
                StaffarrDemandLineId = lineRequest.StaffarrDemandLineId,
                PartId = partId,
                PartNumber = NormalizePartNumber(lineRequest.PartNumber, partId),
                Description = lineRequest.Description?.Trim() ?? string.Empty,
                QuantityRequested = lineRequest.QuantityRequested,
                UnitOfMeasure = NormalizeUnitOfMeasure(lineRequest.UnitOfMeasure),
                Notes = lineRequest.Notes?.Trim() ?? string.Empty,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "demand_intake.staffarr.ingest",
            request.TenantId,
            null,
            "staffarr_demand_ref",
            entity.Id.ToString(),
            request.StaffarrIncidentId.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            request.TenantId,
            IntegrationOutboxEventKinds.StaffarrDemandReceived,
            "staffarr_demand_ref",
            entity.Id,
            new IntegrationOutboxPayload(
                request.TenantId,
                $"StaffArr demand received: {entity.StaffarrIncidentTitle}"),
            cancellationToken: cancellationToken);

        var createdPurchaseRequestDraft = false;
        Guid? purchaseRequestId = null;
        if (request.CreatePurchaseRequestDraft)
        {
            var draft = await CreatePurchaseRequestDraftInternalAsync(
                request.TenantId,
                Guid.Empty,
                entity,
                cancellationToken);
            if (draft is not null)
            {
                createdPurchaseRequestDraft = true;
                purchaseRequestId = draft.PurchaseRequestId;
                await demandStatusCallbacks.NotifyPrDraftedAsync(
                    entity,
                    draft.PurchaseRequestId,
                    now,
                    cancellationToken);
            }
        }

        return new StaffarrDemandIntakeResponse(
            entity.Id,
            entity.Status,
            purchaseRequestId,
            createdPurchaseRequestDraft,
            false);
    }

    public async Task<IReadOnlyList<StaffArrDemandRefResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.StaffArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        var refs = await query.OrderByDescending(x => x.ReceivedAt).ToListAsync(cancellationToken);
        return refs.Select(Map).ToList();
    }

    public async Task<StaffArrDemandRefResponse> GetAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, demandRefId, cancellationToken);
        return Map(entity);
    }

    public async Task<PurchaseRequestResponse> CreatePurchaseRequestFromDemandRefAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid demandRefId,
        CreatePurchaseRequestFromStaffarrDemandRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, demandRefId, cancellationToken);
        if (entity.PurchaseRequestId.HasValue)
        {
            throw new StlApiException(
                "staffarr_demand_intake.purchase_request_exists",
                "A purchase request already exists for this StaffArr demand reference.",
                409);
        }

        var draft = await CreatePurchaseRequestDraftInternalAsync(
            tenantId,
            actorUserId,
            entity,
            request,
            cancellationToken);

        if (draft is null)
        {
            throw new StlApiException(
                "staffarr_demand_intake.no_catalog_parts",
                "StaffArr demand reference has no catalog-linked parts eligible for purchase request creation.",
                400);
        }

        return draft;
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        StaffArrDemandRef entity,
        CancellationToken cancellationToken)
    {
        var requestKey = $"staffarr-{entity.StaffarrIncidentId:N}-{entity.StaffarrPublicationId:N}".ToLowerInvariant();
        return await CreatePurchaseRequestDraftInternalAsync(
            tenantId,
            actorUserId,
            entity,
            new CreatePurchaseRequestFromStaffarrDemandRefRequest(
                requestKey,
                $"StaffArr incident {entity.StaffarrIncidentTitle}",
                entity.Notes),
            cancellationToken);
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        StaffArrDemandRef entity,
        CreatePurchaseRequestFromStaffarrDemandRefRequest request,
        CancellationToken cancellationToken)
    {
        var lines = entity.Lines
            .Where(x => x.PartId.HasValue)
            .Select(x => new CreatePurchaseRequestLineRequest(
                x.PartId!.Value,
                x.QuantityRequested,
                x.Notes))
            .ToList();

        if (lines.Count == 0)
        {
            return null;
        }

        var created = await purchaseRequests.CreateAsync(
            tenantId,
            actorUserId,
            new CreatePurchaseRequestRequest(
                request.RequestKey,
                request.Title,
                request.Notes ?? string.Empty,
                null,
                lines),
            cancellationToken);

        entity.PurchaseRequestId = created.PurchaseRequestId;
        entity.Status = StaffArrDemandRefStatuses.PrDrafted;
        entity.ProcurementStatus = StaffArrDemandRefProcurementStatuses.PrDrafted;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await demandStatusCallbacks.NotifyPrDraftedAsync(
            entity,
            created.PurchaseRequestId,
            entity.UpdatedAt,
            cancellationToken);

        await audit.WriteAsync(
            "staffarr_demand_intake.create_purchase_request",
            tenantId,
            actorUserId == Guid.Empty ? null : actorUserId,
            "purchase_request",
            created.PurchaseRequestId.ToString(),
            entity.Id.ToString(),
            cancellationToken: cancellationToken);

        return created;
    }

    private async Task EnsurePartExistsAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Parts.AnyAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "staffarr_demand_intake.part_not_found",
                "Referenced SupplyArr part was not found.",
                404);
        }
    }

    private async Task<StaffArrDemandRef> LoadAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken) =>
        await db.StaffArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
        ?? throw new StlApiException("staffarr_demand_intake.not_found", "StaffArr demand reference was not found.", 404);

    private async Task<StaffArrDemandRef> LoadTrackedAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken) =>
        await db.StaffArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
        ?? throw new StlApiException("staffarr_demand_intake.not_found", "StaffArr demand reference was not found.", 404);

    private static StaffArrDemandRefResponse Map(StaffArrDemandRef entity) =>
        new(
            entity.Id,
            entity.StaffarrPublicationId,
            entity.StaffarrIncidentId,
            entity.StaffarrPersonId,
            entity.StaffarrIncidentTitle,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.ProcurementStatus,
            entity.PurchaseRequestId,
            entity.PurchaseOrderId,
            entity.ReceivedAt,
            entity.UpdatedAt,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(line => new StaffArrDemandRefLineResponse(
                    line.Id,
                    line.LineNumber,
                    line.StaffarrDemandLineId,
                    line.PartId,
                    line.PartNumber,
                    line.Description,
                    line.QuantityRequested,
                    line.UnitOfMeasure,
                    line.Notes))
                .ToList());

    private static void ValidateIngestRequest(IngestStaffarrDemandRequest request)
    {
        if (request.Lines is not { Count: > 0 })
        {
            throw new StlApiException(
                "staffarr_demand_intake.validation",
                "At least one demand line is required.",
                400);
        }

        foreach (var line in request.Lines)
        {
            if (line.QuantityRequested <= 0)
            {
                throw new StlApiException(
                    "staffarr_demand_intake.validation",
                    "Each demand line quantity must be greater than zero.",
                    400);
            }

            if (!line.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(line.PartNumber))
            {
                throw new StlApiException(
                    "staffarr_demand_intake.validation",
                    "Each demand line requires a SupplyArr part id or part number.",
                    400);
            }
        }
    }

    private static string NormalizePartNumber(string? partNumber, Guid? partId)
    {
        var normalized = partNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized) && partId.HasValue)
        {
            return partId.Value.ToString("N")[..12].ToUpperInvariant();
        }

        return normalized.Length > 128 ? normalized[..128] : normalized;
    }

    private static string NormalizeUnitOfMeasure(string? unitOfMeasure)
    {
        var normalized = string.IsNullOrWhiteSpace(unitOfMeasure) ? "each" : unitOfMeasure.Trim();
        return normalized.Length > 32 ? normalized[..32] : normalized;
    }
}
