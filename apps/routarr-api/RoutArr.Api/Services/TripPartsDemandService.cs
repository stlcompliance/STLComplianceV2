using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripPartsDemandService(
    RoutArrDbContext db,
    SupplyArrDemandClient supplyArrDemandClient,
    IRoutArrAuditService audit)
{
    public async Task<IReadOnlyList<TripPartsDemandLineResponse>> ListAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        await EnsureTripExistsAsync(tenantId, tripId, cancellationToken);

        return await db.TripPartsDemandLines
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapResponse(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TripPartsDemandLineResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        CreateTripPartsDemandLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var trip = await GetEditableTripAsync(tenantId, tripId, cancellationToken);
        ValidateLineRequest(request);

        var now = DateTimeOffset.UtcNow;
        var entity = new TripPartsDemandLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TripId = tripId,
            LineNumber = await GetNextLineNumberAsync(tenantId, tripId, cancellationToken),
            SupplyarrPartId = request.SupplyarrPartId,
            PartNumber = NormalizePartNumber(request.PartNumber, request.SupplyarrPartId),
            Description = request.Description?.Trim() ?? string.Empty,
            QuantityRequested = request.QuantityRequested,
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            Notes = request.Notes?.Trim() ?? string.Empty,
            Status = TripPartsDemandStatuses.Pending,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.TripPartsDemandLines.Add(entity);
        trip.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trip_parts_demand.create",
            tenantId,
            actorUserId,
            "trip_parts_demand",
            entity.Id.ToString(),
            tripId.ToString(),
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    public async Task<PublishTripPartsDemandResponse> PublishAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid tripId,
        PublishTripPartsDemandRequest request,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == tripId,
            cancellationToken)
            ?? throw new StlApiException("trips.not_found", "Trip was not found.", 404);

        var pendingLines = await db.TripPartsDemandLines
            .Where(x =>
                x.TenantId == tenantId
                && x.TripId == tripId
                && x.Status == TripPartsDemandStatuses.Pending)
            .OrderBy(x => x.LineNumber)
            .ToListAsync(cancellationToken);

        if (pendingLines.Count == 0)
        {
            throw new StlApiException(
                "trip_parts_demand.no_pending",
                "No pending parts demand lines are available to publish.",
                400);
        }

        var publicationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        foreach (var line in pendingLines)
        {
            line.Status = TripPartsDemandStatuses.Published;
            line.RoutarrPublicationId = publicationId;
            line.PublishedAt = now;
            line.UpdatedAt = now;
        }

        trip.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var ingestRequest = new SupplyArrIngestRoutarrDemandPayload(
            tenantId,
            publicationId,
            trip.Id,
            trip.TripNumber,
            trip.VehicleRefKey ?? string.Empty,
            trip.Title,
            trip.Description,
            request.CreatePurchaseRequestDraft,
            pendingLines.Select(line => new SupplyArrIngestRoutarrDemandLinePayload(
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
            "trip_parts_demand.publish",
            tenantId,
            actorUserId,
            "routarr_demand_publication",
            publicationId.ToString(),
            tripId.ToString(),
            cancellationToken: cancellationToken);

        var publishedLines = await ListAsync(tenantId, tripId, cancellationToken);
        return new PublishTripPartsDemandResponse(
            publicationId,
            intake.DemandRefId,
            intake.PurchaseRequestId,
            intake.CreatedPurchaseRequestDraft,
            publishedLines);
    }

    private static TripPartsDemandLineResponse MapResponse(TripPartsDemandLine entity) =>
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
            entity.RoutarrPublicationId,
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

    private async Task EnsureTripExistsAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var exists = await db.Trips.AnyAsync(
            x => x.TenantId == tenantId && x.Id == tripId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("trips.not_found", "Trip was not found.", 404);
        }
    }

    private async Task<Trip> GetEditableTripAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var trip = await db.Trips.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == tripId,
            cancellationToken)
            ?? throw new StlApiException("trips.not_found", "Trip was not found.", 404);

        if (string.Equals(trip.DispatchStatus, TripDispatchStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(trip.DispatchStatus, TripDispatchStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "trip_parts_demand.trip_not_editable",
                "Parts demand can only be added while the trip is active.",
                409);
        }

        return trip;
    }

    private async Task<int> GetNextLineNumberAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken)
    {
        var maxLine = await db.TripPartsDemandLines
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .MaxAsync(x => (int?)x.LineNumber, cancellationToken);
        return (maxLine ?? 0) + 1;
    }

    private static void ValidateLineRequest(CreateTripPartsDemandLineRequest request)
    {
        if (request.QuantityRequested <= 0)
        {
            throw new StlApiException(
                "trip_parts_demand.invalid_quantity",
                "Quantity requested must be greater than zero.",
                400);
        }

        if (!request.SupplyarrPartId.HasValue && string.IsNullOrWhiteSpace(request.PartNumber))
        {
            throw new StlApiException(
                "trip_parts_demand.part_required",
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
