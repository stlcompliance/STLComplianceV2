using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class RoutarrEventIngestionService(
    MaintainArrDbContext db,
    AssetReadinessService assetReadinessService,
    AssetDowntimeService assetDowntimeService,
    IMaintainArrAuditService audit)
{
    public static readonly Guid RoutarrEventActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f6");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlySet<string> DefectEventKinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "driver_reported_defect.created",
        "routarr.driver_reported_defect.created",
        "driver.defect_reported",
    };

    private static readonly IReadOnlySet<string> IncidentEventKinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "incident.created",
        "routarr.incident.created",
        "exception.created",
        "routarr.exception.created",
        "route.exception_created",
    };

    public async Task<IngestRoutarrEventResponse> IngestAsync(
        IngestRoutarrEventRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var existing = await db.MaintenanceInboundPlatformEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SourceProduct == "routarr"
                    && x.SourceEventId == request.SourceEventId,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestRoutarrEventResponse(
                existing.Id,
                existing.Outcome,
                existing.CreatedDefectId,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var occurredAt = request.OccurredAt ?? now;
        var shouldCreateDefect = ShouldCreateDefect(request);
        var payloadJson = JsonSerializer.Serialize(request.Payload, JsonOptions);

        Defect? defect = null;
        if (shouldCreateDefect)
        {
            defect = await CreateDefectAsync(request, now, cancellationToken);
        }

        var inbound = new MaintenanceInboundPlatformEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SourceProduct = "routarr",
            SourceEventId = request.SourceEventId,
            EventKind = request.EventKind.Trim().ToLowerInvariant(),
            RelatedEntityType = NormalizeOptional(request.RelatedEntityType) ?? "routarr_event",
            RelatedEntityId = request.RelatedEntityId,
            CorrelationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId,
            PayloadJson = payloadJson,
            Outcome = defect is null ? MaintenanceInboundEventOutcomes.Ignored : MaintenanceInboundEventOutcomes.Processed,
            CreatedDefectId = defect?.Id,
            OccurredAt = occurredAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.MaintenanceInboundPlatformEvents.Add(inbound);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "routarr.event.ingest",
            request.TenantId,
            null,
            "routarr_event",
            inbound.Id.ToString(),
            inbound.Outcome,
            reasonCode: inbound.EventKind,
            cancellationToken: cancellationToken);

        return new IngestRoutarrEventResponse(
            inbound.Id,
            inbound.Outcome,
            inbound.CreatedDefectId,
            false);
    }

    private async Task<Defect> CreateDefectAsync(
        IngestRoutarrEventRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var asset = await assetReadinessService.ResolveAssetForDispatchAsync(
            request.TenantId,
            null,
            payload.VehicleRefKey,
            null,
            cancellationToken);

        var severity = MapSeverity(payload);
        var defect = new Defect
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            AssetId = asset.Id,
            Title = BuildTitle(request),
            Description = BuildDescription(request),
            Severity = severity,
            Status = DefectStatuses.Open,
            Source = DefectSources.RoutArr,
            ReportedByUserId = RoutarrEventActorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Defects.Add(defect);

        if (string.Equals(severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase))
        {
            var trackedAsset = await db.Assets
                .FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.Id == asset.Id, cancellationToken);
            if (trackedAsset is not null
                && string.Equals(trackedAsset.LifecycleStatus, "active", StringComparison.OrdinalIgnoreCase))
            {
                trackedAsset.LifecycleStatus = "out_of_service";
                trackedAsset.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        if (string.Equals(severity, DefectSeverities.Critical, StringComparison.OrdinalIgnoreCase))
        {
            await assetDowntimeService.TryOpenCriticalDefectOutOfServiceDowntimeAsync(
                request.TenantId,
                RoutarrEventActorUserId,
                defect.Id,
                asset.Id,
                asset.AssetTag,
                asset.Name,
                cancellationToken);
        }

        await audit.WriteAsync(
            "defect.create_from_routarr",
            request.TenantId,
            RoutarrEventActorUserId,
            "defect",
            defect.Id.ToString(),
            request.SourceEventId.ToString(),
            reasonCode: request.EventKind,
            cancellationToken: cancellationToken);

        return defect;
    }

    private static bool ShouldCreateDefect(IngestRoutarrEventRequest request)
    {
        if (DefectEventKinds.Contains(request.EventKind))
        {
            return true;
        }

        if (!IncidentEventKinds.Contains(request.EventKind))
        {
            throw new StlApiException(
                "routarr_event.unsupported_event_kind",
                "Unsupported RoutArr event kind for MaintainArr ingestion.",
                400);
        }

        var payload = request.Payload;
        return IsMaintainArrRouted(payload.IncidentRoutedProduct)
            || IsEquipmentIncident(payload.IncidentType)
            || string.Equals(payload.ExceptionCategory, "vehicle", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildTitle(IngestRoutarrEventRequest request)
    {
        var payload = request.Payload;
        if (DefectEventKinds.Contains(request.EventKind))
        {
            return Truncate(
                string.IsNullOrWhiteSpace(payload.TripNumber)
                    ? "Driver-reported RoutArr defect"
                    : $"Driver-reported RoutArr defect from trip {payload.TripNumber}",
                256);
        }

        var incidentKey = NormalizeOptional(payload.ExceptionKey)
            ?? payload.ExceptionId?.ToString()
            ?? request.RelatedEntityId.ToString();

        return Truncate($"RoutArr equipment incident {incidentKey}", 256);
    }

    private static string BuildDescription(IngestRoutarrEventRequest request)
    {
        var payload = request.Payload;
        var parts = new List<string>
        {
            payload.Summary,
            $"RoutArr event {request.EventKind} ({request.SourceEventId:D}).",
        };

        AddIfPresent(parts, "Trip", payload.TripNumber ?? payload.TripId?.ToString());
        AddIfPresent(parts, "Driver", payload.DriverPersonId);
        AddIfPresent(parts, "Vehicle reference", payload.VehicleRefKey);
        AddIfPresent(parts, "DVIR result", payload.DvirResult);
        AddIfPresent(parts, "Driver notes", payload.DefectNotes);
        AddIfPresent(parts, "Incident type", payload.IncidentType);
        AddIfPresent(parts, "Incident severity", payload.IncidentSeverity);
        AddIfPresent(parts, "Exception category", payload.ExceptionCategory);

        return Truncate(string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x))), 1024);
    }

    private static string MapSeverity(RoutarrEventPayload payload)
    {
        var normalized = NormalizeOptional(payload.IncidentSeverity)?.ToLowerInvariant();
        return normalized switch
        {
            "critical" => DefectSeverities.Critical,
            "high" => DefectSeverities.High,
            "medium" => DefectSeverities.Medium,
            "low" => DefectSeverities.Low,
            _ when string.Equals(payload.DvirResult, "fail", StringComparison.OrdinalIgnoreCase) => DefectSeverities.Medium,
            _ => DefectSeverities.Medium,
        };
    }

    private static void ValidateRequest(IngestRoutarrEventRequest request)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException("routarr_event.validation", "Tenant id is required.", 400);
        }

        if (request.SourceEventId == Guid.Empty)
        {
            throw new StlApiException("routarr_event.validation", "Source event id is required.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.EventKind))
        {
            throw new StlApiException("routarr_event.validation", "Event kind is required.", 400);
        }

        if (request.Payload.TenantId != request.TenantId)
        {
            throw new StlApiException("routarr_event.validation", "Payload tenant id must match request tenant id.", 400);
        }
    }

    private static bool IsMaintainArrRouted(string? product) =>
        string.Equals(product, "maintainarr", StringComparison.OrdinalIgnoreCase);

    private static bool IsEquipmentIncident(string? incidentType) =>
        string.Equals(incidentType, "equipment_abuse", StringComparison.OrdinalIgnoreCase);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void AddIfPresent(List<string> parts, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parts.Add($"{label}: {value.Trim()}.");
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
