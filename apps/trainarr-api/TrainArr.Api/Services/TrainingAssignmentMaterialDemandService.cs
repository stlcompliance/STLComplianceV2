using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingAssignmentMaterialDemandService(
    TrainArrDbContext db,
    SupplyArrDemandClient supplyArrDemandClient,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingAssignmentMaterialDemandLineResponse>> ListAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssignmentExistsAsync(tenantId, assignmentId, cancellationToken);

        return await db.TrainingAssignmentMaterialDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingAssignmentMaterialDemandLineResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assignmentId,
        CreateTrainingAssignmentMaterialDemandLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await GetEditableAssignmentAsync(tenantId, assignmentId, cancellationToken);
        ValidateLineRequest(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingAssignmentMaterialDemandLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingAssignmentId = assignmentId,
            LineNumber = await GetNextLineNumberAsync(tenantId, assignmentId, cancellationToken),
            SupplyarrPartId = request.SupplyarrPartId,
            PartNumber = NormalizePartNumber(request.PartNumber, request.SupplyarrPartId),
            Description = request.Description?.Trim() ?? string.Empty,
            QuantityRequested = request.QuantityRequested,
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = TrainingAssignmentMaterialDemandStatuses.Pending,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TrainingAssignmentMaterialDemandLines.Add(entity);
        assignment.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "training_assignment_material_demand.create",
            tenantId,
            actorUserId,
            "training_assignment_material_demand",
            entity.Id.ToString(),
            assignmentId.ToString(),
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<PublishTrainingAssignmentMaterialDemandResponse> PublishAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assignmentId,
        PublishTrainingAssignmentMaterialDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await db.TrainingAssignments
            .Include(x => x.TrainingDefinition)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assignmentId, cancellationToken)
            ?? throw new StlApiException("training_assignments.not_found", "Training assignment was not found.", 404);

        var pendingLines = await db.TrainingAssignmentMaterialDemandLines
            .Where(x =>
                x.TenantId == tenantId
                && x.TrainingAssignmentId == assignmentId
                && x.Status == TrainingAssignmentMaterialDemandStatuses.Pending)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        if (pendingLines.Count == 0)
        {
            throw new StlApiException(
                "training_assignment_material_demand.no_pending",
                "No pending material demand lines are available to publish.",
                400);
        }

        var publicationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        foreach (var line in pendingLines)
        {
            line.Status = TrainingAssignmentMaterialDemandStatuses.Published;
            line.TrainarrPublicationId = publicationId;
            line.PublishedAt = now;
            line.UpdatedAt = now;
        }

        assignment.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var definition = assignment.TrainingDefinition;
        var ingestRequest = new SupplyArrIngestTrainarrDemandPayload(
            tenantId,
            publicationId,
            assignment.Id,
            definition.DefinitionKey,
            assignment.StaffarrPersonId,
            definition.Name,
            definition.Description,
            request.CreatePurchaseRequestDraft,
            pendingLines.Select(line => new SupplyArrIngestTrainarrDemandLinePayload(
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
            "training_assignment_material_demand.publish",
            tenantId,
            actorUserId,
            "trainarr_demand_publication",
            publicationId.ToString(),
            assignmentId.ToString(),
            cancellationToken: cancellationToken);

        var publishedLines = await ListAsync(tenantId, assignmentId, cancellationToken);
        return new PublishTrainingAssignmentMaterialDemandResponse(
            publicationId,
            intake.DemandRefId,
            intake.PurchaseRequestId,
            intake.CreatedPurchaseRequestDraft,
            publishedLines);
    }

    private static TrainingAssignmentMaterialDemandLineResponse MapResponse(TrainingAssignmentMaterialDemandLine entity) =>
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
            entity.TrainarrPublicationId,
            entity.SupplyarrDemandRefId,
            entity.PublishedAt,
            entity.ProcurementStatus,
            entity.SupplyarrPurchaseRequestId,
            entity.SupplyarrPurchaseOrderId,
            entity.QuantityReceived,
            entity.ProcurementStatusMessage,
            entity.LastProcurementStatusAt,
            entity.CreatedAt,
            entity.UpdatedAt);

    private async Task EnsureAssignmentExistsAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var exists = await db.TrainingAssignments.AnyAsync(
            x => x.TenantId == tenantId && x.Id == assignmentId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("training_assignments.not_found", "Training assignment was not found.", 404);
        }
    }

    private async Task<TrainingAssignment> GetEditableAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var assignment = await db.TrainingAssignments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assignmentId,
            cancellationToken)
            ?? throw new StlApiException("training_assignments.not_found", "Training assignment was not found.", 404);

        if (string.Equals(assignment.Status, "completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(assignment.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "training_assignment_material_demand.assignment_not_editable",
                "Material demand can only be added while the assignment is active.",
                409);
        }

        return assignment;
    }

    private async Task<int> GetNextLineNumberAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var maxLine = await db.TrainingAssignmentMaterialDemandLines
            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)
            .MaxAsync(x => (int?)x.LineNumber, cancellationToken);
        return (maxLine ?? 0) + 1;
    }

    private static void ValidateLineRequest(CreateTrainingAssignmentMaterialDemandLineRequest request)
    {
        if (request.QuantityRequested <= 0)
        {
            throw new StlApiException(
                "training_assignment_material_demand.invalid_quantity",
                "Quantity requested must be greater than zero.",
                400);
        }

        if (!request.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(request.PartNumber))
        {
            throw new StlApiException(
                "training_assignment_material_demand.part_required",
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

        return normalized.Length > 128 ? normalized[..128] : normalized;
    }

    private static string NormalizeUnitOfMeasure(string? unitOfMeasure)
    {
        var normalized = string.IsNullOrWhiteSpace(unitOfMeasure) ? "each" : unitOfMeasure.Trim();
        return normalized.Length > 32 ? normalized[..32] : normalized;
    }
}
