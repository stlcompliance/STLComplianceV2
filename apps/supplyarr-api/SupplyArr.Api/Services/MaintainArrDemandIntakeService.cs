using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class MaintainArrDemandIntakeService(
    SupplyArrDbContext db,
    PurchaseRequestService purchaseRequests,
    MaintainArrDemandStatusCallbackService demandStatusCallbacks,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    public async Task<MaintainarrDemandIntakeResponse> IngestAsync(
        IngestMaintainarrDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateIngestRequest(request);

        var existing = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.MaintainarrPublicationId == request.MaintainarrPublicationId,
                cancellationToken);

        if (existing is not null)
        {
            return new MaintainarrDemandIntakeResponse(
                existing.Id,
                existing.Status,
                existing.PurchaseRequestId,
                existing.Status == MaintainArrDemandRefStatuses.PrDrafted,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new MaintainArrDemandRef
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            MaintainarrPublicationId = request.MaintainarrPublicationId,
            MaintainarrWorkOrderId = request.MaintainarrWorkOrderId,
            MaintainarrWorkOrderNumber = request.MaintainarrWorkOrderNumber.Trim(),
            MaintainarrAssetId = request.MaintainarrAssetId,
            Title = request.Title.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = MaintainArrDemandRefStatuses.Received,
            ReceivedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.MaintainArrDemandRefs.Add(entity);

        var lineNumber = 1;
        foreach (var lineRequest in request.Lines)
        {
            var partId = lineRequest.SupplyarrPartId;
            if (partId.HasValue)
            {
                await EnsurePartExistsAsync(request.TenantId, partId.Value, cancellationToken);
            }

            entity.Lines.Add(new MaintainArrDemandRefLine
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                DemandRefId = entity.Id,
                LineNumber = lineNumber++,
                MaintainarrDemandLineId = lineRequest.MaintainarrDemandLineId,
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
            "demand_intake.maintainarr.ingest",
            request.TenantId,
            null,
            "maintainarr_demand_ref",
            entity.Id.ToString(),
            request.MaintainarrWorkOrderId.ToString(),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            request.TenantId,
            IntegrationOutboxEventKinds.MaintainarrDemandReceived,
            "maintainarr_demand_ref",
            entity.Id,
            new IntegrationOutboxPayload(
                request.TenantId,
                $"MaintainArr demand received: {entity.MaintainarrWorkOrderNumber}"),
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

        return new MaintainarrDemandIntakeResponse(
            entity.Id,
            entity.Status,
            purchaseRequestId,
            createdPurchaseRequestDraft,
            false);
    }

    public async Task<IReadOnlyList<MaintainArrDemandRefResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MaintainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        var refs = await query
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync(cancellationToken);

        return refs.Select(Map).ToList();
    }

    public async Task<MaintainArrDemandRefResponse> GetAsync(
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
        CreatePurchaseRequestFromDemandRefRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, demandRefId, cancellationToken);
        if (entity.PurchaseRequestId.HasValue)
        {
            throw new StlApiException(
                "demand_intake.purchase_request_exists",
                "A purchase request already exists for this demand reference.",
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
                "demand_intake.no_catalog_parts",
                "Demand reference has no catalog-linked parts eligible for purchase request creation.",
                400);
        }

        return draft;
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        MaintainArrDemandRef entity,
        CancellationToken cancellationToken)
    {
        var requestKey = $"maintainarr-{entity.MaintainarrWorkOrderNumber}-{entity.MaintainarrPublicationId:N}".ToLowerInvariant();
        return await CreatePurchaseRequestDraftInternalAsync(
            tenantId,
            actorUserId,
            entity,
            new CreatePurchaseRequestFromDemandRefRequest(
                requestKey,
                $"MaintainArr WO {entity.MaintainarrWorkOrderNumber}",
                entity.Notes),
            cancellationToken);
    }

    private async Task<PurchaseRequestResponse?> CreatePurchaseRequestDraftInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        MaintainArrDemandRef entity,
        CreatePurchaseRequestFromDemandRefRequest request,
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
        entity.Status = MaintainArrDemandRefStatuses.PrDrafted;
        entity.ProcurementStatus = MaintainArrDemandRefProcurementStatuses.PrDrafted;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await demandStatusCallbacks.NotifyPrDraftedAsync(
            entity,
            created.PurchaseRequestId,
            entity.UpdatedAt,
            cancellationToken);

        await audit.WriteAsync(
            "demand_intake.create_purchase_request",
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
                "demand_intake.part_not_found",
                "Referenced SupplyArr part was not found.",
                404);
        }
    }

    private async Task<MaintainArrDemandRef> LoadAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken)
    {
        return await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
            ?? throw new StlApiException("demand_intake.not_found", "Demand reference was not found.", 404);
    }

    private async Task<MaintainArrDemandRef> LoadTrackedAsync(
        Guid tenantId,
        Guid demandRefId,
        CancellationToken cancellationToken)
    {
        return await db.MaintainArrDemandRefs
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == demandRefId, cancellationToken)
            ?? throw new StlApiException("demand_intake.not_found", "Demand reference was not found.", 404);
    }

    private static MaintainArrDemandRefResponse Map(MaintainArrDemandRef entity) =>
        new(
            entity.Id,
            entity.MaintainarrPublicationId,
            entity.MaintainarrWorkOrderId,
            entity.MaintainarrWorkOrderNumber,
            entity.MaintainarrAssetId,
            entity.Title,
            entity.Notes,
            entity.Status,
            entity.ProcurementStatus,
            entity.PurchaseRequestId,
            entity.PurchaseOrderId,
            entity.LastStatusCallbackAt,
            entity.ReceivedAt,
            entity.UpdatedAt,
            entity.Lines
                .OrderBy(x => x.LineNumber)
                .Select(line => new MaintainArrDemandRefLineResponse(
                    line.Id,
                    line.LineNumber,
                    line.MaintainarrDemandLineId,
                    line.PartId,
                    line.PartNumber,
                    line.Description,
                    line.QuantityRequested,
                    line.UnitOfMeasure,
                    line.Notes))
                .ToList());

    private static void ValidateIngestRequest(IngestMaintainarrDemandRequest request)
    {
        if (request.Lines is not { Count: > 0 })
        {
            throw new StlApiException(
                "demand_intake.validation",
                "At least one demand line is required.",
                400);
        }

        foreach (var line in request.Lines)
        {
            if (line.QuantityRequested <= 0)
            {
                throw new StlApiException(
                    "demand_intake.validation",
                    "Each demand line quantity must be greater than zero.",
                    400);
            }

            if (!line.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(line.PartNumber))
            {
                throw new StlApiException(
                    "demand_intake.validation",
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
