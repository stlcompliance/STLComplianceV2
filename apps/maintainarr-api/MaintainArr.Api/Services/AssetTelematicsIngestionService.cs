using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetTelematicsIngestionService(MaintainArrDbContext db)
{
    private const int MaxLimit = 25;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AssetTelematicsIngestionResponse> ListAsync(
        Guid tenantId,
        Guid assetId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit ?? 8, 1, MaxLimit);

        var asset = await db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var query = from inbound in db.MaintenanceInboundPlatformEvents.AsNoTracking()
                    join defect in db.Defects.AsNoTracking() on inbound.CreatedDefectId equals defect.Id into defectJoin
                    from defect in defectJoin.DefaultIfEmpty()
                    where inbound.TenantId == tenantId
                        && (
                            (inbound.RelatedEntityType == "asset" && inbound.RelatedEntityId == assetId)
                            || (defect != null && defect.AssetId == assetId)
                        )
                    select inbound;

        var totalCount = await query.CountAsync(cancellationToken);
        var processedCount = await query.CountAsync(x => x.Outcome == MaintenanceInboundEventOutcomes.Processed, cancellationToken);
        var ignoredCount = await query.CountAsync(x => x.Outcome == MaintenanceInboundEventOutcomes.Ignored, cancellationToken);
        var defectCount = await query.CountAsync(x => x.CreatedDefectId != null, cancellationToken);

        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return new AssetTelematicsIngestionResponse(
            asset.Id,
            asset.AssetTag,
            asset.Name,
            totalCount,
            normalizedLimit,
            processedCount,
            ignoredCount,
            defectCount,
            items.Select(Map).ToList());
    }

    private static AssetTelematicsIngestionEventResponse Map(MaintenanceInboundPlatformEvent inbound)
    {
        var payload = TryDeserializePayload(inbound.PayloadJson);

        return new AssetTelematicsIngestionEventResponse(
            inbound.Id,
            inbound.SourceEventId,
            inbound.SourceProduct,
            inbound.EventKind,
            inbound.Outcome,
            BuildSummary(inbound, payload),
            payload?.VehicleRefKey,
            payload?.TripNumber,
            payload?.IncidentType,
            payload?.IncidentSeverity,
            payload?.DvirResult,
            inbound.CreatedDefectId,
            inbound.CorrelationId,
            inbound.OccurredAt,
            inbound.CreatedAt);
    }

    private static RoutarrEventPayload? TryDeserializePayload(string payloadJson)
    {
        try
        {
            return JsonSerializer.Deserialize<RoutarrEventPayload>(payloadJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildSummary(MaintenanceInboundPlatformEvent inbound, RoutarrEventPayload? payload)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(payload?.Summary))
        {
            parts.Add(payload.Summary.Trim());
        }
        else
        {
            parts.Add(inbound.EventKind.Replace('_', ' '));
        }

        AddIfPresent(parts, "Trip", payload?.TripNumber);
        AddIfPresent(parts, "Vehicle", payload?.VehicleRefKey);
        AddIfPresent(parts, "DVIR", payload?.DvirResult);
        AddIfPresent(parts, "Incident", payload?.IncidentType);
        AddIfPresent(parts, "Severity", payload?.IncidentSeverity);
        AddIfPresent(parts, "Exception", payload?.ExceptionCategory);

        return Truncate(string.Join(" · ", parts.Where(value => !string.IsNullOrWhiteSpace(value))), 240);
    }

    private static void AddIfPresent(ICollection<string> parts, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parts.Add($"{label} {value.Trim()}");
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..Math.Max(0, maxLength - 1)].TrimEnd() + "…";
    }
}
