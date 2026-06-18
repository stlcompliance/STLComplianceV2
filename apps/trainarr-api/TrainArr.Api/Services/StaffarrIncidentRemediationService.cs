using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class StaffarrIncidentRemediationService(
    TrainArrDbContext db,
    IntegrationSettingsService integrationSettingsService,
    TrainArrTenantSettingsService tenantSettingsService,
    TrainingEventEnqueueService trainingEventEnqueueService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> AllowedReasonCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "training_compliance"
    };

    private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "medium",
        "high",
        "critical"
    };

    public async Task<StaffarrIncidentRemediationResponse> IngestAsync(
        IngestStaffarrIncidentRemediationRequest request,
        CancellationToken cancellationToken = default)
    {
        await integrationSettingsService.EnsureStaffArrIncidentIntakeEnabledAsync(
            request.TenantId,
            cancellationToken);
        await EnsureIncidentRetrainingAcceptedAsync(request.TenantId, cancellationToken);

        var reasonCategoryKey = NormalizeReasonCategoryKey(request.ReasonCategoryKey);
        var severity = NormalizeSeverity(request.Severity);
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);

        var existing = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SourceProduct == "staffarr"
                    && x.SourceIncidentId == request.StaffarrIncidentId,
                cancellationToken);

        if (existing is not null)
        {
            return MapResponse(existing);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new StaffarrIncidentRemediation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrIncidentId = request.StaffarrIncidentId,
            StaffarrPersonId = request.StaffarrPersonId,
            SourceProduct = "staffarr",
            SourceIncidentId = request.StaffarrIncidentId,
            SourceEventKind = "staffarr.incident.created",
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Title = title,
            Description = description,
            OccurredAt = request.OccurredAt,
            ReportedAt = request.ReportedAt,
            Status = "intake_received",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.StaffarrIncidentRemediations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident_remediation.intake",
            request.TenantId,
            null,
            "staffarr_incident_remediation",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueueRemediationRequiredEventAsync(entity, cancellationToken);

        return MapResponse(entity);
    }

    public async Task<IngestRoutarrIncidentRemediationResponse> IngestRoutarrAsync(
        IngestRoutarrIncidentRemediationRequest request,
        CancellationToken cancellationToken = default)
    {
        await integrationSettingsService.EnsureRoutarrIncidentIntakeEnabledAsync(
            request.TenantId,
            cancellationToken);
        await EnsureIncidentRetrainingAcceptedAsync(request.TenantId, cancellationToken);

        ValidateRoutarrRequest(request);

        var existing = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SourceProduct == "routarr"
                    && x.SourceIncidentId == request.SourceEventId,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestRoutarrIncidentRemediationResponse(
                existing.Id,
                existing.TenantId,
                existing.SourceIncidentId,
                existing.StaffarrPersonId,
                existing.Status,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var severity = NormalizeSeverity(request.Payload.IncidentSeverity ?? "medium");
        var title = NormalizeTitle(BuildRoutarrTitle(request));
        var description = NormalizeDescription(BuildRoutarrDescription(request));
        var personId = Guid.Parse(request.Payload.DriverPersonId!);
        var occurredAt = request.OccurredAt ?? now;
        var entity = new StaffarrIncidentRemediation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrIncidentId = request.SourceEventId,
            StaffarrPersonId = personId,
            SourceProduct = "routarr",
            SourceIncidentId = request.SourceEventId,
            SourceEventKind = request.EventKind.Trim().ToLowerInvariant(),
            ReasonCategoryKey = "training_compliance",
            Severity = severity,
            Title = title,
            Description = description,
            OccurredAt = occurredAt,
            ReportedAt = now,
            Status = "intake_received",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.StaffarrIncidentRemediations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "incident_remediation.routarr_intake",
            request.TenantId,
            null,
            "staffarr_incident_remediation",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: entity.SourceEventKind,
            cancellationToken: cancellationToken);

        await EnqueueRemediationRequiredEventAsync(entity, cancellationToken);

        return new IngestRoutarrIncidentRemediationResponse(
            entity.Id,
            entity.TenantId,
            entity.SourceIncidentId,
            entity.StaffarrPersonId,
            entity.Status,
            false);
    }

    public async Task<IngestSupplyarrIncidentRemediationResponse> IngestSupplyarrAsync(
        IngestSupplyarrIncidentRemediationRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureIncidentRetrainingAcceptedAsync(request.TenantId, cancellationToken);
        ValidateSupplyarrRequest(request);

        var existing = await db.StaffarrIncidentRemediations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SourceProduct == "supplyarr"
                    && x.SourceIncidentId == request.SourceEventId,
                cancellationToken);

        if (existing is not null)
        {
            return new IngestSupplyarrIncidentRemediationResponse(
                existing.Id,
                existing.TenantId,
                existing.SourceIncidentId,
                existing.StaffarrPersonId,
                existing.Status,
                true);
        }

        var now = DateTimeOffset.UtcNow;
        var severity = NormalizeSeverity(request.Payload.Severity);
        var title = NormalizeTitle(BuildSupplyarrTitle(request));
        var description = NormalizeDescription(BuildSupplyarrDescription(request));
        var occurredAt = request.OccurredAt ?? now;
        var entity = new StaffarrIncidentRemediation
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StaffarrIncidentId = request.SourceEventId,
            StaffarrPersonId = request.StaffarrPersonId,
            SourceProduct = "supplyarr",
            SourceIncidentId = request.SourceEventId,
            SourceEventKind = request.EventKind.Trim().ToLowerInvariant(),
            ReasonCategoryKey = "training_compliance",
            Severity = severity,
            Title = title,
            Description = description,
            OccurredAt = occurredAt,
            ReportedAt = now,
            Status = "intake_received",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.StaffarrIncidentRemediations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "incident_remediation.supplyarr_intake",
            request.TenantId,
            null,
            "staffarr_incident_remediation",
            entity.Id.ToString(),
            "Succeeded",
            reasonCode: entity.SourceEventKind,
            cancellationToken: cancellationToken);

        await EnqueueRemediationRequiredEventAsync(entity, cancellationToken);

        return new IngestSupplyarrIncidentRemediationResponse(
            entity.Id,
            entity.TenantId,
            entity.SourceIncidentId,
            entity.StaffarrPersonId,
            entity.Status,
            false);
    }

    private Task EnqueueRemediationRequiredEventAsync(
        StaffarrIncidentRemediation remediation,
        CancellationToken cancellationToken) =>
        trainingEventEnqueueService.TryEnqueueAsync(
            remediation.TenantId,
            TrainingDomainEventKinds.RemediationRequired,
            TrainingEventPayloadBuilder.ForRemediationRequired(remediation),
            cancellationToken);

    private async Task EnsureIncidentRetrainingAcceptedAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await tenantSettingsService.LoadPayloadAsync(tenantId, cancellationToken);
        if (!settings.Remediation.AcceptIncidentRetrainingRequests)
        {
            throw new StlApiException(
                "incident_remediations.disabled",
                "TrainArr incident-driven retraining intake is disabled for this tenant.",
                409);
        }
    }

    private static StaffarrIncidentRemediationResponse MapResponse(StaffarrIncidentRemediation entity) =>
        new(
            entity.Id,
            entity.TenantId,
            entity.StaffarrIncidentId,
            entity.StaffarrPersonId,
            entity.ReasonCategoryKey,
            entity.Status,
            entity.CreatedAt);

    private static string NormalizeReasonCategoryKey(string reasonCategoryKey)
    {
        var normalized = reasonCategoryKey.Trim().ToLowerInvariant();
        if (!AllowedReasonCategories.Contains(normalized))
        {
            throw new StlApiException(
                "incident_remediations.validation",
                $"Reason category must be one of: {string.Join(", ", AllowedReasonCategories.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSeverity(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();
        if (!AllowedSeverities.Contains(normalized))
        {
            throw new StlApiException(
                "incident_remediations.validation",
                $"Severity must be one of: {string.Join(", ", AllowedSeverities.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length < 4 || trimmed.Length > 200)
        {
            throw new StlApiException(
                "incident_remediations.validation",
                "Incident title must be between 4 and 200 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 16 || trimmed.Length > 4096)
        {
            throw new StlApiException(
                "incident_remediations.validation",
                "Incident description must be between 16 and 4096 characters.",
                400);
        }

        return trimmed;
    }

    private static void ValidateRoutarrRequest(IngestRoutarrIncidentRemediationRequest request)
    {
        if (request.TenantId == Guid.Empty || request.SourceEventId == Guid.Empty)
        {
            throw new StlApiException("incident_remediations.validation", "Tenant id and source event id are required.", 400);
        }

        if (request.Payload.TenantId != request.TenantId)
        {
            throw new StlApiException("incident_remediations.validation", "Payload tenant id must match request tenant id.", 400);
        }

        if (!string.Equals(request.EventKind, "incident.created", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.EventKind, "routarr.incident.created", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("incident_remediations.validation", "Only RoutArr incident.created events are supported.", 400);
        }

        if (!string.Equals(request.Payload.IncidentRoutedProduct, "trainarr", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.Payload.IncidentType, "training_related", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("incident_remediations.validation", "RoutArr incident is not routed to TrainArr.", 400);
        }

        if (!Guid.TryParse(request.Payload.DriverPersonId, out _))
        {
            throw new StlApiException("incident_remediations.validation", "RoutArr driver person id must be a StaffArr person GUID.", 400);
        }
    }

    private static void ValidateSupplyarrRequest(IngestSupplyarrIncidentRemediationRequest request)
    {
        if (request.TenantId == Guid.Empty || request.SourceEventId == Guid.Empty || request.SupplierIncidentId == Guid.Empty)
        {
            throw new StlApiException("incident_remediations.validation", "Tenant id, source event id, and supplier incident id are required.", 400);
        }

        if (request.StaffarrPersonId == Guid.Empty)
        {
            throw new StlApiException("incident_remediations.validation", "SupplyArr incident remediation requires a StaffArr person id.", 400);
        }

        if (request.Payload.TenantId != request.TenantId || request.Payload.SupplierIncidentId != request.SupplierIncidentId)
        {
            throw new StlApiException("incident_remediations.validation", "Payload identifiers must match the request identifiers.", 400);
        }

        if (!string.Equals(request.EventKind, "supplier_incident.created", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.EventKind, "supplyarr.supplier_incident.created", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("incident_remediations.validation", "Only SupplyArr supplier incident created events are supported.", 400);
        }
    }

    private static string BuildRoutarrTitle(IngestRoutarrIncidentRemediationRequest request)
    {
        var key = string.IsNullOrWhiteSpace(request.Payload.ExceptionKey)
            ? request.Payload.ExceptionId?.ToString() ?? request.RelatedEntityId.ToString()
            : request.Payload.ExceptionKey.Trim();

        return $"RoutArr training incident {key}";
    }

    private static string BuildRoutarrDescription(IngestRoutarrIncidentRemediationRequest request)
    {
        var payload = request.Payload;
        var details = new List<string>
        {
            string.IsNullOrWhiteSpace(payload.Summary) ? "RoutArr training-related incident was routed for remediation." : payload.Summary,
            $"Source event: {request.EventKind} ({request.SourceEventId:D}).",
        };

        AddIfPresent(details, "Trip", payload.TripNumber ?? payload.TripId?.ToString());
        AddIfPresent(details, "Driver", payload.DriverPersonId);
        AddIfPresent(details, "Vehicle reference", payload.VehicleRefKey);
        AddIfPresent(details, "Incident type", payload.IncidentType);
        AddIfPresent(details, "Incident severity", payload.IncidentSeverity);
        AddIfPresent(details, "Exception category", payload.ExceptionCategory);

        return string.Join(" ", details);
    }

    private static string BuildSupplyarrTitle(IngestSupplyarrIncidentRemediationRequest request) =>
        $"SupplyArr training incident {request.Payload.IncidentKey.Trim()}";

    private static string BuildSupplyarrDescription(IngestSupplyarrIncidentRemediationRequest request)
    {
        var payload = request.Payload;
        var details = new List<string>
        {
            string.IsNullOrWhiteSpace(payload.Summary) ? "SupplyArr supplier incident was routed for remediation." : payload.Summary,
            $"Source event: {request.EventKind} ({request.SourceEventId:D}).",
            $"Supplier incident: {payload.IncidentKey}.",
            $"External party: {payload.PartyDisplayName}.",
            $"Incident type: {payload.IncidentType}.",
            $"Severity: {payload.Severity}.",
        };

        AddIfPresent(details, "Purchase request", payload.PurchaseRequestId?.ToString());
        AddIfPresent(details, "Purchase order", payload.PurchaseOrderId?.ToString());
        AddIfPresent(details, "Receiving receipt", payload.ReceivingReceiptId?.ToString());
        AddIfPresent(details, "Receiving exception", payload.ReceivingExceptionId?.ToString());

        return string.Join(" ", details);
    }

    private static void AddIfPresent(List<string> details, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            details.Add($"{label}: {value.Trim()}.");
        }
    }
}
