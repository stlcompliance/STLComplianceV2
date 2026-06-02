using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Text.Json;

namespace StaffArr.Api.Services;

public sealed class IncidentService(
    StaffArrDbContext db,
    IncidentRoutingService routingService,
    IStaffArrAuditService audit)
{
    private static readonly Guid ProductIncidentServiceActorId = Guid.Parse("00000000-0000-0000-0000-0000000005fa");

    private static readonly HashSet<string> AllowedSourceProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "compliancecore",
        "maintainarr",
        "routarr",
        "supplyarr",
        "trainarr"
    };

    private static readonly HashSet<string> AllowedReasonCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "safety",
        "conduct",
        "behavior",
        "injury",
        "equipment",
        "equipment_damage",
        "training_compliance",
        "training_issue",
        "policy",
        "policy_violation",
        "attendance",
        "near_miss",
        "other"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "draft",
        "submitted",
        "open",
        "in_review",
        "closed"
    };

    private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "medium",
        "high",
        "critical"
    };

    private static readonly HashSet<string> AllowedIncidentSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "staffarr",
        "self_report",
        "manager_report",
        "safety_observation",
        "compliancecore",
        "maintainarr",
        "routarr",
        "supplyarr",
        "trainarr",
        "other"
    };

    private static readonly HashSet<string> AllowedIncidentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "injury",
        "safety",
        "behavior",
        "equipment_damage",
        "policy_violation",
        "training_issue",
        "attendance",
        "near_miss",
        "other"
    };

    private static readonly HashSet<string> AllowedReadinessDecisions = new(StringComparer.OrdinalIgnoreCase)
    {
        "allowed",
        "watched",
        "restricted"
    };

    private static readonly HashSet<string> AllowedWorkRestrictions = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "modified_duty",
        "restricted_duty",
        "removed_from_duty"
    };

    private static readonly HashSet<string> AllowedYesNoPending = new(StringComparer.OrdinalIgnoreCase)
    {
        "no",
        "yes",
        "pending"
    };

    private static readonly HashSet<string> AllowedMedicalAttention = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "first_aid",
        "clinic",
        "emergency",
        "unknown"
    };

    private static readonly HashSet<string> AllowedPpeConcerns = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "damaged",
        "missing",
        "inadequate",
        "unknown"
    };

    private static readonly HashSet<string> AllowedFollowUpStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "no",
        "yes",
        "conditional"
    };

    private static readonly HashSet<string> AllowedTrainingReviewReasons = new(StringComparer.OrdinalIgnoreCase)
    {
        "certification_gap",
        "procedure_gap",
        "behavior_coaching",
        "remedial_training",
        "other"
    };

    public async Task<PersonnelIncidentDetailResponse> CreateIncidentAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePersonnelIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == request.PersonId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var reasonCategoryKey = NormalizeReasonCategoryKey(request.ReasonCategoryKey);
        var severity = NormalizeSeverity(request.Severity);
        var status = NormalizeStatus(request.Status, "open");
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);
        ValidateOccurredAt(request.OccurredAt);
        ValidateTimestampNotFuture(request.DiscoveredAt, "Incident discovery time");
        ValidateTimestampNotFuture(request.FollowUpDueAt, "Follow-up due date", allowFuture: true);

        if (request.ReporterPersonId is Guid reporterPersonId)
        {
            await RequirePersonExistsAsync(tenantId, reporterPersonId, "Reporter person", cancellationToken);
        }

        if (request.ManagerPersonId is Guid managerPersonId)
        {
            await RequirePersonExistsAsync(tenantId, managerPersonId, "Manager or supervisor", cancellationToken);
        }

        if (request.WitnessPersonIds is { Count: > 0 })
        {
            await RequirePeopleExistAsync(tenantId, request.WitnessPersonIds, "Witnesses", cancellationToken);
        }

        if (request.AdditionalInvolvedPersonIds is { Count: > 0 })
        {
            await RequirePeopleExistAsync(
                tenantId,
                request.AdditionalInvolvedPersonIds,
                "Additional involved persons",
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Status = status,
            Title = title,
            Description = description,
            OccurredAt = request.OccurredAt,
            DiscoveredAt = request.DiscoveredAt,
            ReportedAt = now,
            ReportedByUserId = actorUserId,
            ReporterPersonId = request.ReporterPersonId,
            ManagerPersonId = request.ManagerPersonId,
            IncidentSource = NormalizeControlledOptional(
                request.IncidentSource,
                AllowedIncidentSources,
                "Incident source"),
            IncidentType = NormalizeControlledOptional(
                request.IncidentType,
                AllowedIncidentTypes,
                "Incident type"),
            SiteOrgUnitId = request.SiteOrgUnitId,
            DepartmentOrgUnitId = request.DepartmentOrgUnitId,
            LocationDetail = NormalizeOptional(request.LocationDetail, 256, "Location detail"),
            WitnessPersonIdsJson = SerializeGuidList(request.WitnessPersonIds),
            AdditionalInvolvedPersonIdsJson = SerializeGuidList(request.AdditionalInvolvedPersonIds),
            EmployeeSelfReport = request.EmployeeSelfReport,
            ImmediateActionsTaken = NormalizeOptional(request.ImmediateActionsTaken, 2000, "Immediate actions taken"),
            RootCause = NormalizeOptional(request.RootCause, 2000, "Root cause"),
            CategoryKeysJson = SerializeStringList(
                request.CategoryKeys,
                AllowedIncidentTypes,
                "Incident categories"),
            ReadinessDecision = NormalizeControlledOptional(
                request.ReadinessDecision,
                AllowedReadinessDecisions,
                "Readiness decision"),
            WorkRestriction = NormalizeControlledOptional(
                request.WorkRestriction,
                AllowedWorkRestrictions,
                "Work restriction"),
            ReturnToWorkNeeded = NormalizeControlledOptional(
                request.ReturnToWorkNeeded,
                AllowedYesNoPending,
                "Return-to-work needed"),
            PpeConcern = NormalizeControlledOptional(request.PpeConcern, AllowedPpeConcerns, "PPE concern"),
            MedicalAttention = NormalizeControlledOptional(
                request.MedicalAttention,
                AllowedMedicalAttention,
                "Medical attention"),
            OutOfServiceRemoveFromDuty = NormalizeControlledOptional(
                request.OutOfServiceRemoveFromDuty,
                AllowedYesNoPending,
                "Out-of-service or remove-from-duty"),
            FollowUpRequired = NormalizeControlledOptional(
                request.FollowUpRequired,
                AllowedFollowUpStates,
                "Follow-up required"),
            TrainingReviewRequired = request.TrainingReviewRequired,
            TrainingReviewReason = NormalizeControlledOptional(
                request.TrainingReviewReason,
                AllowedTrainingReviewReasons,
                "Training review reason"),
            RelatedAssetReference = NormalizeOptional(request.RelatedAssetReference, 128, "Related asset"),
            RelatedWorkOrderReference = NormalizeOptional(request.RelatedWorkOrderReference, 128, "Related work order"),
            RelatedRouteReference = NormalizeOptional(request.RelatedRouteReference, 128, "Related route or trip"),
            RelatedSupplierReference = NormalizeOptional(request.RelatedSupplierReference, 128, "Related supplier or party"),
            RelatedDocumentReference = NormalizeOptional(request.RelatedDocumentReference, 128, "Related document"),
            RelatedPolicyReference = NormalizeOptional(request.RelatedPolicyReference, 128, "Related policy"),
            EvidencePackageRequested = request.EvidencePackageRequested,
            NotifyManager = request.NotifyManager,
            NotifySafetyCompliance = request.NotifySafetyCompliance,
            NotifyHr = request.NotifyHr,
            CreateFollowUpTask = request.CreateFollowUpTask,
            FollowUpDueAt = request.FollowUpDueAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonnelIncidents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident.intake",
            tenantId,
            actorUserId,
            "personnel_incident",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(entity, null);
    }

    public async Task<IngestProductIncidentResponse> CreateProductIncidentAsync(
        IngestProductIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var sourceProduct = NormalizeSourceProduct(request.SourceProduct);
        if (request.SourceIncidentId == Guid.Empty)
        {
            throw new StlApiException(
                "incidents.validation",
                "Source incident identifier must be a valid identifier.",
                400);
        }

        if (request.PersonId == Guid.Empty)
        {
            throw new StlApiException(
                "incidents.validation",
                "Person identifier must be a valid identifier.",
                400);
        }

        var sourceEventKind = NormalizeOptional(request.SourceEventKind, 64, "Source event kind");
        var sourceReferenceKey = NormalizeOptional(request.SourceReferenceKey, 128, "Source reference key");

        var existing = await db.PersonnelIncidents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == request.TenantId
                    && x.SourceProduct == sourceProduct
                    && x.SourceIncidentId == request.SourceIncidentId,
                cancellationToken);
        if (existing is not null)
        {
            return new IngestProductIncidentResponse(
                existing.Id,
                existing.PersonId,
                sourceProduct,
                request.SourceIncidentId,
                existing.Status,
                IdempotentReplay: true);
        }

        var personExists = await db.People.AnyAsync(
            x => x.TenantId == request.TenantId && x.Id == request.PersonId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var reasonCategoryKey = NormalizeReasonCategoryKey(request.ReasonCategoryKey);
        var severity = NormalizeSeverity(request.Severity);
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);
        ValidateOccurredAt(request.OccurredAt);

        var now = DateTimeOffset.UtcNow;
        var entity = new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            PersonId = request.PersonId,
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Status = "open",
            Title = title,
            Description = description,
            OccurredAt = request.OccurredAt,
            ReportedAt = now,
            ReportedByUserId = ProductIncidentServiceActorId,
            SourceProduct = sourceProduct,
            SourceIncidentId = request.SourceIncidentId,
            SourceEventKind = sourceEventKind,
            SourceReferenceKey = sourceReferenceKey,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonnelIncidents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident.product_intake",
            request.TenantId,
            ProductIncidentServiceActorId,
            "personnel_incident",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new IngestProductIncidentResponse(
            entity.Id,
            entity.PersonId,
            sourceProduct,
            request.SourceIncidentId,
            entity.Status,
            IdempotentReplay: false);
    }

    public async Task<PersonnelIncidentDetailResponse> CreateSelfReportAsync(
        Guid tenantId,
        Guid personId,
        Guid actorUserId,
        SubmitSelfReportedPersonnelIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }

        var reasonCategoryKey = NormalizeReasonCategoryKey(request.ReasonCategoryKey);
        var severity = NormalizeSeverity(request.Severity);
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);
        ValidateOccurredAt(request.OccurredAt);

        var now = DateTimeOffset.UtcNow;
        var entity = new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Status = "submitted",
            Title = title,
            Description = description,
            OccurredAt = request.OccurredAt,
            ReportedAt = now,
            ReportedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonnelIncidents.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident.self_report.submitted",
            tenantId,
            actorUserId,
            "personnel_incident",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(entity, null);
    }

    public async Task<IReadOnlyList<PersonnelIncidentSummaryResponse>> ListIncidentsAsync(
        Guid tenantId,
        Guid? personId,
        CancellationToken cancellationToken = default)
    {
        var query = db.PersonnelIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (personId is Guid filterPersonId)
        {
            query = query.Where(x => x.PersonId == filterPersonId);
        }

        var incidents = await query
            .OrderByDescending(x => x.ReportedAt)
            .ThenByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);

        var incidentIds = incidents.Select(x => x.Id).ToList();
        var routings = await db.IncidentTrainarrRoutings
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && incidentIds.Contains(x.IncidentId))
            .ToDictionaryAsync(x => x.IncidentId, cancellationToken);

        return incidents
            .Select(x => MapSummary(x, routings.GetValueOrDefault(x.Id)))
            .ToList();
    }

    public async Task<PersonnelIncidentDetailResponse> GetIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonnelIncidents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("incidents.not_found", "Incident was not found.", 404);
        }

        var routing = await routingService.GetRoutingForIncidentAsync(tenantId, incidentId, cancellationToken);
        return MapDetail(entity, routing);
    }

    private static string NormalizeReasonCategoryKey(string reasonCategoryKey)
    {
        var normalized = reasonCategoryKey.Trim().ToLowerInvariant();
        if (!AllowedReasonCategories.Contains(normalized))
        {
            throw new StlApiException(
                "incidents.validation",
                $"Reason category must be one of: {string.Join(", ", AllowedReasonCategories.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSourceProduct(string sourceProduct)
    {
        var normalized = (sourceProduct ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedSourceProducts.Contains(normalized))
        {
            throw new StlApiException(
                "incidents.validation",
                $"Source product must be one of: {string.Join(", ", AllowedSourceProducts.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeSeverity(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();
        if (!AllowedSeverities.Contains(normalized))
        {
            throw new StlApiException(
                "incidents.validation",
                $"Severity must be one of: {string.Join(", ", AllowedSeverities.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string? status, string defaultStatus)
    {
        var normalized = string.IsNullOrWhiteSpace(status)
            ? defaultStatus
            : status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "incidents.validation",
                $"Status must be one of: {string.Join(", ", AllowedStatuses.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length < 4)
        {
            throw new StlApiException(
                "incidents.validation",
                "Incident title must be at least 4 characters.",
                400);
        }

        if (trimmed.Length > 200)
        {
            throw new StlApiException(
                "incidents.validation",
                "Incident title must be 200 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 16)
        {
            throw new StlApiException(
                "incidents.validation",
                "Incident description must be at least 16 characters.",
                400);
        }

        if (trimmed.Length > 4096)
        {
            throw new StlApiException(
                "incidents.validation",
                "Incident description must be 4096 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static void ValidateOccurredAt(DateTimeOffset occurredAt)
    {
        ValidateTimestampNotFuture(occurredAt, "Incident occurrence time");
    }

    private static void ValidateTimestampNotFuture(
        DateTimeOffset? value,
        string displayName,
        bool allowFuture = false)
    {
        if (!allowFuture && value > DateTimeOffset.UtcNow.AddHours(1))
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} cannot be in the future.",
                400);
        }
    }

    private async Task RequirePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        string displayName,
        CancellationToken cancellationToken)
    {
        if (personId == Guid.Empty)
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} must be a valid person identifier.",
                400);
        }

        var exists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "people.not_found",
                $"{displayName} was not found.",
                404);
        }
    }

    private async Task RequirePeopleExistAsync(
        Guid tenantId,
        IReadOnlyList<Guid> personIds,
        string displayName,
        CancellationToken cancellationToken)
    {
        var distinct = personIds.Where(x => x != Guid.Empty).Distinct().ToArray();
        if (distinct.Length != personIds.Count)
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} must only include valid person identifiers.",
                400);
        }

        var foundCount = await db.People.CountAsync(
            x => x.TenantId == tenantId && distinct.Contains(x.Id),
            cancellationToken);
        if (foundCount != distinct.Length)
        {
            throw new StlApiException(
                "people.not_found",
                $"{displayName} included a person that was not found.",
                404);
        }
    }

    private static string? NormalizeControlledOptional(
        string? value,
        HashSet<string> allowedValues,
        string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} must be one of: {string.Join(", ", allowedValues.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string? SerializeGuidList(IReadOnlyList<Guid>? values)
    {
        var normalized = values?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();
        return normalized is { Length: > 0 } ? JsonSerializer.Serialize(normalized) : null;
    }

    private static string? SerializeStringList(
        IReadOnlyList<string>? values,
        HashSet<string> allowedValues,
        string displayName)
    {
        var normalized = values?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();
        if (normalized is not { Length: > 0 })
        {
            return null;
        }

        var invalid = normalized.FirstOrDefault(x => !allowedValues.Contains(x));
        if (invalid is not null)
        {
            throw new StlApiException(
                "incidents.validation",
                $"{displayName} must be one of: {string.Join(", ", allowedValues.OrderBy(x => x))}.",
                400);
        }

        return JsonSerializer.Serialize(normalized);
    }

    private static IReadOnlyList<Guid> DeserializeGuidList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<Guid>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static PersonnelIncidentSummaryResponse MapSummary(
        PersonnelIncident entity,
        IncidentTrainarrRouting? routing = null) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.ReasonCategoryKey,
            entity.Severity,
            entity.Status,
            entity.Title,
            entity.OccurredAt,
            entity.ReportedAt,
            entity.ReportedByUserId,
            routing is null ? null : IncidentRoutingService.MapRouting(routing),
            entity.IncidentSource,
            entity.IncidentType,
            entity.DiscoveredAt,
            entity.ReporterPersonId,
            entity.ManagerPersonId,
            DeserializeStringList(entity.CategoryKeysJson),
            entity.ReadinessDecision,
            entity.TrainingReviewRequired,
            entity.SourceProduct,
            entity.SourceIncidentId,
            entity.SourceEventKind,
            entity.SourceReferenceKey);

    private static PersonnelIncidentDetailResponse MapDetail(
        PersonnelIncident entity,
        IncidentTrainarrRoutingResponse? routing) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.ReasonCategoryKey,
            entity.Severity,
            entity.Status,
            entity.Title,
            entity.Description,
            entity.OccurredAt,
            entity.ReportedAt,
            entity.ReportedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt,
            routing,
            entity.IncidentSource,
            entity.IncidentType,
            entity.DiscoveredAt,
            entity.SiteOrgUnitId,
            entity.DepartmentOrgUnitId,
            entity.LocationDetail,
            entity.ReporterPersonId,
            entity.ManagerPersonId,
            DeserializeGuidList(entity.WitnessPersonIdsJson),
            DeserializeGuidList(entity.AdditionalInvolvedPersonIdsJson),
            entity.EmployeeSelfReport,
            entity.ImmediateActionsTaken,
            entity.RootCause,
            DeserializeStringList(entity.CategoryKeysJson),
            entity.ReadinessDecision,
            entity.WorkRestriction,
            entity.ReturnToWorkNeeded,
            entity.PpeConcern,
            entity.MedicalAttention,
            entity.OutOfServiceRemoveFromDuty,
            entity.FollowUpRequired,
            entity.TrainingReviewRequired,
            entity.TrainingReviewReason,
            entity.RelatedAssetReference,
            entity.RelatedWorkOrderReference,
            entity.RelatedRouteReference,
            entity.RelatedSupplierReference,
            entity.RelatedDocumentReference,
            entity.RelatedPolicyReference,
            entity.EvidencePackageRequested,
            entity.NotifyManager,
            entity.NotifySafetyCompliance,
            entity.NotifyHr,
            entity.CreateFollowUpTask,
            entity.FollowUpDueAt,
            entity.SourceProduct,
            entity.SourceIncidentId,
            entity.SourceEventKind,
            entity.SourceReferenceKey);
}
