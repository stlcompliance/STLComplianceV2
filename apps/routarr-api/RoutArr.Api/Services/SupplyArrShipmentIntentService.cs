using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class SupplyArrShipmentIntentService(
    RoutArrDbContext db,
    IRoutArrAuditService audit)
{
    public async Task<CreateSupplyArrShipmentIntentResponse> CreateAsync(
        CreateSupplyArrShipmentIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
        {
            throw new StlApiException("supplyarr_shipments.lines_required", "At least one shipment line is required.", 400);
        }

        var existing = await db.SupplyArrShipmentIntents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId && x.SupplyarrShipmentId == request.SupplyarrShipmentId,
                cancellationToken);
        if (existing is not null)
        {
            return new CreateSupplyArrShipmentIntentResponse(existing.Id, existing.RouteId, existing.Status);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new SupplyArrShipmentIntent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SupplyarrShipmentId = request.SupplyarrShipmentId,
            ShipmentKey = NormalizeText(request.ShipmentKey, 128),
            DestinationName = NormalizeText(request.DestinationName, 256),
            DestinationAddressSnapshot = NormalizeText(request.DestinationAddressSnapshot, 1024),
            Status = "created",
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new StlApiException("supplyarr_shipments.quantity_invalid", "Shipment line quantity must be greater than zero.", 400);
            }

            entity.Lines.Add(new SupplyArrShipmentIntentLine
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                ShipmentIntentId = entity.Id,
                SupplyarrShipmentLineId = line.SupplyarrShipmentLineId,
                PartId = line.PartId,
                PartDisplayName = NormalizeText(line.PartDisplayName, 256),
                Quantity = line.Quantity,
            });
        }

        db.SupplyArrShipmentIntents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr_shipment_intent.create",
            request.TenantId,
            null,
            "supplyarr_shipment_intent",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new CreateSupplyArrShipmentIntentResponse(entity.Id, entity.RouteId, entity.Status);
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
        {
            throw new StlApiException("supplyarr_shipments.validation", "Required shipment text is missing.", 400);
        }

        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
