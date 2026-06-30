using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class TrainArrDemandIntakeService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    TrainArrDemandStatusCallbackService demandStatusCallbacks,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<TrainarrDemandIntakeResponse> IngestAsync(
        IngestTrainarrDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateIngestRequest(request);

        var existing = await db.TrainArrDemandRefs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.TrainarrPublicationId == request.TrainarrPublicationId,
                cancellationToken);

        if (existing is not null)
        {
            return new TrainarrDemandIntakeResponse(
                existing.Id,
                existing.Status,
                existing.PurchaseRequestId,
                existing.Status == TrainArrDemandRefStatuses.PrDrafted,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainArrDemandRef
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            TrainarrPublicationId = request.TrainarrPublicationId,
            TrainarrAssignmentId = request.TrainarrAssignmentId,
            TrainarrAssignmentRefKey = request.TrainarrAssignmentRefKey.Trim(),
            StaffarrPersonId = request.StaffarrPersonId,
            Title = request.Title.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = TrainArrDemandRefStatuses.Received,
            ReceivedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainArrDemandRefs.Add(entity);

        var lineNumber = 1;
        foreach (var lineRequest in request.Lines)
        {
            var partId = lineRequest.SupplyarrPartId;
            if (partId.HasValue)
            {
                await EnsurePartExistsAsync(request.TenantId, partId.Value, cancellationToken);
            }

            entity.Lines.Add(new TrainArrDemandRefLine
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                DemandRefId = entity.Id,
                LineNumber = lineNumber++,
                TrainarrDemandLineId = lineRequest.TrainarrDemandLineId,
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
            "demand_intake.trainarr.ingest",
            request.TenantId,
            null,
            "trainarr_demand_ref",
            entity.Id.ToString(),
            request.TrainarrAssignmentId.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            request.TenantId,
            IntegrationOutboxEventKinds.TrainarrDemandReceived,
            "trainarr_demand_ref",
            entity.Id,
            new IntegrationOutboxPayload(
                request.TenantId,
                $"TrainArr demand received: {entity.TrainarrAssignmentRefKey}"),
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

        return new TrainarrDemandIntakeResponse(
            entity.Id,
            entity.Status,
            purchaseRequestId,
            createdPurchaseRequestDraft,
            false);
    }

    public async Task<IReadOnlyList<TrainArrDemandRefResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainArrDemandRefs
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

    public async Task<TrainArrDemandRefResponse> GetAsync(
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
        CreatePurchaseRequestFromTrainarrDemandRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, demandRefId, cancellationToken);
        if (entity.PurchaseRequestId.HasValue)
        {
            throw new StlApiException(
                "trainarr_demand_intake.purchase_request_exists",
                "A purchase request already exists for this TrainArr demand reference.",
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
                "trainarr_demand_intake.no_catalog_parts",
                "TrainArr demand reference has no catalog-linked parts eligible for purchase request creation.",
                400);
        }

        return draft;
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        TrainArrDemandRef entity,
        CancellationToken cancellationToken)
    {
        var requestKey = $"trainarr-{entity.TrainarrAssignmentRefKey}-{entity.TrainarrPublicationId:N}".ToLowerInvariant();
        return await CreatePurchaseRequestDraftInternalAsync(
            tenantId,
            actorUserId,
            entity,
            new CreatePurchaseRequestFromTrainarrDemandRefRequest(
                requestKey,
                $"TrainArr assignment {entity.TrainarrAssignmentRefKey}",
                entity.Notes),
            cancellationToken);
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        TrainArrDemandRef entity,
        CreatePurchaseRequestFromTrainarrDemandRefRequest request,
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
                RequestKey: request.RequestKey,
                Title: request.Title,
                Notes: request.Notes ?? string.Empty,
                SupplierId: null,
                Lines: lines),
            cancellationToken);

        entity.PurchaseRequestId = created.PurchaseRequestId;
        entity.Status = TrainArrDemandRefStatuses.PrDrafted;
        entity.ProcurementStatus = TrainArrDemandRefProcurementStatuses.PrDrafted;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await demandStatusCallbacks.NotifyPrDraftedAsync(
            entity,
            created.PurchaseRequestId,
            entity.UpdatedAt,
            cancellationToken);

        await audit.WriteAsync(
            "trainarr_demand_intake.create_purchase_request",
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
                "trainarr_demand_intake.part_not_found",
                "Referenced SupplyArr part was not found.",
                404);
        }
    }

    private async Task<TrainArrDemandRef> LoadAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken) =>
        await db.TrainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
        ?? throw new StlApiException("trainarr_demand_intake.not_found", "TrainArr demand reference was not found.", 404);

    private async Task<TrainArrDemandRef> LoadTrackedAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken) =>
        await db.TrainArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
        ?? throw new StlApiException("trainarr_demand_intake.not_found", "TrainArr demand reference was not found.", 404);

    private static TrainArrDemandRefResponse Map(TrainArrDemandRef entity) =>
        new(
            entity.Id,
            entity.TrainarrPublicationId,
            entity.TrainarrAssignmentId,
            entity.TrainarrAssignmentRefKey,
            entity.StaffarrPersonId,
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
                .Select(line => new TrainArrDemandRefLineResponse(
                    line.Id,
                    line.LineNumber,
                    line.TrainarrDemandLineId,
                    line.PartId,
                    line.PartNumber,
                    line.Description,
                    line.QuantityRequested,
                    line.UnitOfMeasure,
                    line.Notes))
                .ToList());

    private static void ValidateIngestRequest(IngestTrainarrDemandRequest request)
    {
        if (request.Lines is not { Count: > 0 })
        {
            throw new StlApiException(
                "trainarr_demand_intake.validation",
                "At least one demand line is required.",
                400);
        }

        foreach (var line in request.Lines)
        {
            if (line.QuantityRequested <= 0)
            {
                throw new StlApiException(
                    "trainarr_demand_intake.validation",
                    "Each demand line quantity must be greater than zero.",
                    400);
            }

            if (!line.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(line.PartNumber))
            {
                throw new StlApiException(
                    "trainarr_demand_intake.validation",
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
