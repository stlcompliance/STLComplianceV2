using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

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
        "injury",
        "equipment",
        "training_compliance",
        "policy",
        "other"
    };

    private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "low",
        "medium",
        "high",
        "critical"
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
        var title = NormalizeTitle(request.Title);
        var description = NormalizeDescription(request.Description);
        ValidateOccurredAt(request.OccurredAt);

        var now = DateTimeOffset.UtcNow;
        var entity = new PersonnelIncident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = request.PersonId,
            ReasonCategoryKey = reasonCategoryKey,
            Severity = severity,
            Status = "open",
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
        if (occurredAt > DateTimeOffset.UtcNow.AddHours(1))
        {
            throw new StlApiException(
                "incidents.validation",
                "Incident occurrence time cannot be in the future.",
                400);
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
            entity.SourceProduct,
            entity.SourceIncidentId,
            entity.SourceEventKind,
            entity.SourceReferenceKey);
}
