using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class WorkOrderPartsDemandService(
    MaintainArrDbContext db,
    SupplyArrDemandClient supplyArrDemandClient,
    IMaintainArrAuditService audit)
{
    public async Task<IReadOnlyList<WorkOrderPartsDemandLineResponse>> ListAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        return await db.WorkOrderPartsDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrderPartsDemandLineResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        CreateWorkOrderPartsDemandLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await GetEditableWorkOrderAsync(tenantId, workOrderId, cancellationToken);
        ValidateLineRequest(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrderPartsDemandLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            LineNumber = await GetNextLineNumberAsync(tenantId, workOrderId, cancellationToken),
            SupplyarrPartId = request.SupplyarrPartId,
            PartNumber = NormalizePartNumber(request.PartNumber, request.SupplyarrPartId),
            Description = request.Description?.Trim() ?? string.Empty,
            QuantityRequested = request.QuantityRequested,
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = WorkOrderPartsDemandStatuses.Pending,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.WorkOrderPartsDemandLines.Add(entity);
        workOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_parts_demand.create",
            tenantId,
            actorUserId,
            "work_order_parts_demand",
            entity.Id.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<PublishWorkOrderPartsDemandResponse> PublishAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid workOrderId,
        PublishWorkOrderPartsDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await db.WorkOrders
            .Include(x => x.Asset)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workOrderId, cancellationToken)
            ?? throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);

        var pendingLines = await db.WorkOrderPartsDemandLines
            .Where(x =>
                x.TenantId == tenantId
                && x.WorkOrderId == workOrderId
                && x.Status == WorkOrderPartsDemandStatuses.Pending)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        if (pendingLines.Count == 0)
        {
            throw new StlApiException(
                "work_order_parts_demand.no_pending",
                "No pending parts demand lines are available to publish.",
                400);
        }

        var publicationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        foreach (var line in pendingLines)
        {
            line.Status = WorkOrderPartsDemandStatuses.Published;
            line.MaintainarrPublicationId = publicationId;
            line.PublishedAt = now;
            line.ProcurementStatus = WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement;
            line.UpdatedAt = now;
        }

        workOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var ingestRequest = new SupplyArrIngestDemandPayload(
            tenantId,
            publicationId,
            workOrder.Id,
            workOrder.WorkOrderNumber,
            workOrder.AssetId,
            workOrder.Title,
            workOrder.Description,
            request.CreatePurchaseRequestDraft,
            pendingLines.Select(line => new SupplyArrIngestDemandLinePayload(
                line.Id,
                line.SupplyarrPartId,
                line.PartNumber,
                line.Description,
                line.QuantityRequested,
                line.UnitOfMeasure,
                line.Notes)).ToList());

        var intake = await supplyArrDemandClient.PublishDemandAsync(ingestRequest, cancellationToken);

        foreach (var line in pendingLines)
        {
            line.SupplyarrDemandRefId = intake.DemandRefId;
            line.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "work_order_parts_demand.publish",
            tenantId,
            actorUserId,
            "maintainarr_demand_publication",
            publicationId.ToString(),
            workOrderId.ToString(),
            cancellationToken: cancellationToken);

        var publishedLines = await ListAsync(tenantId, workOrderId, cancellationToken);
        return new PublishWorkOrderPartsDemandResponse(
            publicationId,
            intake.DemandRefId,
            intake.PurchaseRequestId,
            intake.CreatedPurchaseRequestDraft,
            publishedLines);
    }

    private static WorkOrderPartsDemandLineResponse MapResponse(WorkOrderPartsDemandLine entity) =>
        new(
            entity.Id,
            entity.LineNumber,
            entity.SupplyarrPartId,
            entity.PartNumber,
            entity.Description,
            entity.QuantityRequested,
            entity.UnitOfMeasure,
            entity.Notes,
            entity.Status,
            entity.MaintainarrPublicationId,
            entity.SupplyarrDemandRefId,
            entity.PublishedAt,
            entity.ProcurementStatus,
            entity.SupplyarrPurchaseRequestId,
            entity.SupplyarrPurchaseOrderId,
            entity.QuantityReceived,
            entity.ProcurementStatusMessage,
            entity.LastProcurementStatusAt,
            entity.CreatedAt);

    private async Task EnsureWorkOrderExistsAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var exists = await db.WorkOrders.AnyAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);
        }
    }

    private async Task<WorkOrder> GetEditableWorkOrderAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var workOrder = await db.WorkOrders.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == workOrderId,
            cancellationToken)
            ?? throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);

        if (!WorkOrderStatuses.Active.Contains(workOrder.Status))
        {
            throw new StlApiException(
                "work_order_parts_demand.work_order_not_editable",
                "Parts demand can only be added while the work order is open or in progress.",
                409);
        }

        return workOrder;
    }

    private async Task<int> GetNextLineNumberAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken)
    {
        var maxLine = await db.WorkOrderPartsDemandLines
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == workOrderId)
            .MaxAsync(x => (int?)x.LineNumber, cancellationToken);
        return (maxLine ?? 0) + 1;
    }

    private static void ValidateLineRequest(CreateWorkOrderPartsDemandLineRequest request)
    {
        if (request.QuantityRequested <= 0)
        {
            throw new StlApiException(
                "work_order_parts_demand.invalid_quantity",
                "Quantity requested must be greater than zero.",
                400);
        }

        if (!request.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(request.PartNumber))
        {
            throw new StlApiException(
                "work_order_parts_demand.part_required",
                "Either a SupplyArr part id or part number is required.",
                400);
        }
    }

    private static string NormalizePartNumber(string? partNumber, Guid? supplyarrPartId)
    {
        var normalized = partNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized) && supplyarrPartId.HasValue)
        {
            return supplyarrPartId.Value.ToString("N")[..12].ToUpperInvariant();
        }

        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "work_order_parts_demand.part_number_too_long",
                "Part number must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeUnitOfMeasure(string? unitOfMeasure)
    {
        var normalized = string.IsNullOrWhiteSpace(unitOfMeasure) ? "each" : unitOfMeasure.Trim();
        if (normalized.Length > 32)
        {
            throw new StlApiException(
                "work_order_parts_demand.uom_too_long",
                "Unit of measure must be 32 characters or fewer.",
                400);
        }

        return normalized;
    }
}
