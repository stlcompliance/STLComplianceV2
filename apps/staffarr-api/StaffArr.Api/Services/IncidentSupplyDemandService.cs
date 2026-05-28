using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class IncidentSupplyDemandService(
    StaffArrDbContext db,
    SupplyArrDemandClient supplyArrDemandClient,
    IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<IncidentSupplyDemandLineResponse>> ListAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIncidentExistsAsync(tenantId, incidentId, cancellationToken);

        return await db.IncidentSupplyDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IncidentId == incidentId)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<IncidentSupplyDemandLineResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CreateIncidentSupplyDemandLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await GetEditableIncidentAsync(tenantId, incidentId, cancellationToken);
        ValidateLineRequest(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new IncidentSupplyDemandLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IncidentId = incidentId,
            LineNumber = await GetNextLineNumberAsync(tenantId, incidentId, cancellationToken),
            SupplyarrPartId = request.SupplyarrPartId,
            PartNumber = NormalizePartNumber(request.PartNumber, request.SupplyarrPartId),
            Description = request.Description?.Trim() ?? string.Empty,
            QuantityRequested = request.QuantityRequested,
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = IncidentSupplyDemandStatuses.Pending,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IncidentSupplyDemandLines.Add(entity);
        incident.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "incident_supply_demand.create",
            tenantId,
            actorUserId,
            "incident_supply_demand",
            entity.Id.ToString(),
            incidentId.ToString(),
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<PublishIncidentSupplyDemandResponse> PublishAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        PublishIncidentSupplyDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await db.PersonnelIncidents.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == incidentId,
            cancellationToken)
            ?? throw new StlApiException("incidents.not_found", "Personnel incident was not found.", 404);

        var pendingLines = await db.IncidentSupplyDemandLines
            .Where(x =>
                x.TenantId == tenantId
                && x.IncidentId == incidentId
                && x.Status == IncidentSupplyDemandStatuses.Pending)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        if (pendingLines.Count == 0)
        {
            throw new StlApiException(
                "incident_supply_demand.no_pending",
                "No pending supply demand lines are available to publish.",
                400);
        }

        var publicationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        foreach (var line in pendingLines)
        {
            line.Status = IncidentSupplyDemandStatuses.Published;
            line.StaffarrPublicationId = publicationId;
            line.PublishedAt = now;
            line.UpdatedAt = now;
        }

        incident.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var ingestRequest = new SupplyArrIngestStaffarrDemandPayload(
            tenantId,
            publicationId,
            incident.Id,
            incident.PersonId,
            incident.Title,
            incident.Title,
            incident.Description,
            request.CreatePurchaseRequestDraft,
            pendingLines.Select(line => new SupplyArrIngestStaffarrDemandLinePayload(
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
            "incident_supply_demand.publish",
            tenantId,
            actorUserId,
            "staffarr_demand_publication",
            publicationId.ToString(),
            incidentId.ToString(),
            cancellationToken: cancellationToken);

        var publishedLines = await ListAsync(tenantId, incidentId, cancellationToken);
        return new PublishIncidentSupplyDemandResponse(
            publicationId,
            intake.DemandRefId,
            intake.PurchaseRequestId,
            intake.CreatedPurchaseRequestDraft,
            publishedLines);
    }

    private static IncidentSupplyDemandLineResponse MapResponse(IncidentSupplyDemandLine entity) =>
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
            entity.StaffarrPublicationId,
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

    private async Task EnsureIncidentExistsAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var exists = await db.PersonnelIncidents.AnyAsync(
            x => x.TenantId == tenantId && x.Id == incidentId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("incidents.not_found", "Personnel incident was not found.", 404);
        }
    }

    private async Task<PersonnelIncident> GetEditableIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var incident = await db.PersonnelIncidents.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == incidentId,
            cancellationToken)
            ?? throw new StlApiException("incidents.not_found", "Personnel incident was not found.", 404);

        if (!string.Equals(incident.Status, "open", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "incident_supply_demand.incident_not_editable",
                "Supply demand can only be added while the incident is open.",
                409);
        }

        return incident;
    }

    private async Task<int> GetNextLineNumberAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var maxLine = await db.IncidentSupplyDemandLines
            .Where(x => x.TenantId == tenantId && x.IncidentId == incidentId)
            .MaxAsync(x => (int?)x.LineNumber, cancellationToken);
        return (maxLine ?? 0) + 1;
    }

    private static void ValidateLineRequest(CreateIncidentSupplyDemandLineRequest request)
    {
        if (request.QuantityRequested <= 0)
        {
            throw new StlApiException(
                "incident_supply_demand.invalid_quantity",
                "Quantity requested must be greater than zero.",
                400);
        }

        if (!request.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(request.PartNumber))
        {
            throw new StlApiException(
                "incident_supply_demand.part_required",
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
