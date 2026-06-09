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

    public async Task<IReadOnlyList<WorkOrderPartsDemandStatusEventResponse>> ListStatusEventsAsync(
        Guid tenantId,
        Guid workOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureWorkOrderExistsAsync(tenantId, workOrderId, cancellationToken);

        var publicationIds = await db.WorkOrderPartsDemandLines
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.WorkOrderId == workOrderId
                && x.MaintainarrPublicationId != null)
            .Select(x => x.MaintainarrPublicationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (publicationIds.Count == 0)
        {
            return [];
        }

        return await db.WorkOrderPartsDemandStatusEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && publicationIds.Contains(x.MaintainarrPublicationId))
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new WorkOrderPartsDemandStatusEventResponse(
                x.Id,
                x.MaintainarrPublicationId,
                x.SupplyarrDemandRefId,
                x.EventType,
                x.ProcurementStatus,
                x.SupplyarrPurchaseRequestId,
                x.SupplyarrPurchaseOrderId,
                x.SupplyarrReceivingReceiptId,
                x.Message,
                x.OccurredAt,
                x.CreatedAt))
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
        var maintenancePart = request.MaintenancePartId.HasValue
            ? await db.MaintenanceParts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Id == request.MaintenancePartId.Value,
                    cancellationToken)
            : null;

        if (request.MaintenancePartId.HasValue && maintenancePart is null)
        {
            throw new StlApiException(
                "work_order_parts_demand.maintenance_part_not_found",
                "The selected maintenance part profile was not found.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new WorkOrderPartsDemandLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkOrderId = workOrderId,
            LineNumber = await GetNextLineNumberAsync(tenantId, workOrderId, cancellationToken),
            MaintenancePartId = maintenancePart?.Id,
            SupplyarrPartId = maintenancePart?.SupplyArrPartId ?? request.SupplyarrPartId,
            PartNumber = maintenancePart?.PartNumber ?? NormalizePartNumber(request.PartNumber, request.SupplyarrPartId),
            Description = ResolveLineDescription(request.Description, maintenancePart),
            QuantityRequested = request.QuantityRequested,
            UnitOfMeasure = maintenancePart is null
                ? NormalizeUnitOfMeasure(request.UnitOfMeasure)
                : NormalizeUnitOfMeasure(string.IsNullOrWhiteSpace(request.UnitOfMeasure) ? maintenancePart.UnitOfMeasure : request.UnitOfMeasure),
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
            entity.CreatedAt,
            entity.MaintenancePartId);

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

        if (!WorkOrderStatuses.Active.Contains(workOrder.Status)
            && !string.Equals(workOrder.Status, WorkOrderStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "work_order_parts_demand.work_order_not_editable",
                "Parts demand can only be added while the work order is draft, open, or in progress.",
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

        if (!request.MaintenancePartId.HasValue
            && !request.SupplyarrPartId.HasValue
            && string.IsNullOrWhiteSpace(request.PartNumber))
        {
            throw new StlApiException(
                "work_order_parts_demand.part_required",
                "A maintenance part profile, SupplyArr part id, or part number is required.",
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

    private static string ResolveLineDescription(
        string? requestDescription,
        MaintenancePart? maintenancePart)
    {
        if (!string.IsNullOrWhiteSpace(requestDescription))
        {
            return requestDescription.Trim();
        }

        if (maintenancePart is null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(maintenancePart.Description))
        {
            return maintenancePart.Description;
        }

        return maintenancePart.DisplayName;
    }

    public async Task<IngestPartIssueEventResponse> RecordIssueAsync(
        Guid tenantId,
        Guid actorUserId,
        IngestPartIssueEventRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId != tenantId)
        {
            throw new StlApiException(
                "part_issue.tenant_mismatch",
                "Request tenant does not match the integration tenant.",
                400);
        }

        if (request.WorkOrderId == Guid.Empty)
        {
            throw new StlApiException("part_issue.validation", "Work order id is required.", 400);
        }

        if (request.Quantity <= 0)
        {
            throw new StlApiException("part_issue.validation", "Quantity must be greater than zero.", 400);
        }

        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.WorkOrderId, cancellationToken)
            ?? throw new StlApiException("work_orders.not_found", "Work order was not found.", 404);

        var candidateLines = await db.WorkOrderPartsDemandLines
            .Where(x => x.TenantId == tenantId && x.WorkOrderId == request.WorkOrderId)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        if (candidateLines.Count == 0)
        {
            throw new StlApiException(
                "part_issue.no_demand_lines",
                "No parts demand lines exist for this work order.",
                404);
        }

        var normalizedPartNumber = NormalizeOptionalPartNumber(request.PartNumber);
        var matchingLines = candidateLines
            .Where(line =>
                (request.SupplyarrPartId.HasValue && line.SupplyarrPartId == request.SupplyarrPartId)
                || (!string.IsNullOrWhiteSpace(normalizedPartNumber)
                    && string.Equals(line.PartNumber, normalizedPartNumber, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (matchingLines.Count == 0)
        {
            throw new StlApiException(
                "part_issue.line_not_found",
                "No matching demand lines were found for the issue event.",
                404);
        }

        var now = DateTimeOffset.UtcNow;
        var remaining = request.Quantity;
        var updatedCount = 0;
        foreach (var line in matchingLines)
        {
            if (remaining <= 0)
            {
                break;
            }

            var outstanding = Math.Max(0m, line.QuantityRequested - line.QuantityReceived);
            if (outstanding <= 0)
            {
                continue;
            }

            var issued = Math.Min(outstanding, remaining);
            line.QuantityReceived += issued;
            line.LastProcurementStatusAt = request.OccurredAt;
            line.ProcurementStatusMessage = request.Message?.Trim() ?? "Issue posted.";
            line.ProcurementStatus = line.QuantityReceived >= line.QuantityRequested
                ? WorkOrderPartsDemandProcurementStatuses.ReceivedComplete
                : WorkOrderPartsDemandProcurementStatuses.PartiallyReceived;
            line.UpdatedAt = now;
            updatedCount++;
            remaining -= issued;
        }

        if (updatedCount == 0)
        {
            throw new StlApiException(
                "part_issue.no_outstanding_quantity",
                "The matching demand lines are already fully issued.",
                409);
        }

        workOrder.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var issueId = Guid.NewGuid();
        await audit.WriteAsync(
            "work_order_parts_demand.issue_posted",
            tenantId,
            actorUserId,
            "work_order",
            workOrder.Id.ToString(),
            issueId.ToString(),
            cancellationToken: cancellationToken);

        return new IngestPartIssueEventResponse(
            issueId,
            workOrder.Id,
            updatedCount,
            request.Quantity - Math.Max(0m, remaining),
            matchingLines.All(line => line.QuantityReceived >= line.QuantityRequested)
                ? WorkOrderPartsDemandProcurementStatuses.ReceivedComplete
                : WorkOrderPartsDemandProcurementStatuses.PartiallyReceived,
            request.OccurredAt,
            false);
    }

    private static string? NormalizeOptionalPartNumber(string? partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return null;
        }

        var normalized = partNumber.Trim();
        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "part_issue.part_number_too_long",
                "Part number must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }
}
