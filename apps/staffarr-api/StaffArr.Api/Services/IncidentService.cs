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
    StaffArrDocumentStorageService storage,
    IStaffArrAuditService audit)
{
    private static readonly Guid ProductIncidentServiceActorId = Guid.Parse("00000000-0000-0000-0000-0000000005fa");
    private const long MaxIncidentAttachmentBytes = 10 * 1024 * 1024;

    private static readonly IReadOnlySet<string> AllowedSourceProducts = StaffArrControlledFieldCatalog.SourceProductKeys;
    private static readonly IReadOnlySet<string> AllowedReasonCategories = StaffArrControlledFieldCatalog.IncidentReasonCategoryKeys;
    private static readonly IReadOnlySet<string> AllowedStatuses = StaffArrControlledFieldCatalog.IncidentStatusKeys;
    private static readonly IReadOnlySet<string> AllowedSeverities = StaffArrControlledFieldCatalog.IncidentSeverityKeys;
    private static readonly IReadOnlySet<string> AllowedIncidentSources = StaffArrControlledFieldCatalog.IncidentSourceKeys;
    private static readonly IReadOnlySet<string> AllowedIncidentTypes = StaffArrControlledFieldCatalog.IncidentTypeKeys;
    private static readonly IReadOnlySet<string> AllowedReadinessDecisions = StaffArrControlledFieldCatalog.ReadinessDecisionKeys;
    private static readonly IReadOnlySet<string> AllowedWorkRestrictions = StaffArrControlledFieldCatalog.WorkRestrictionKeys;
    private static readonly IReadOnlySet<string> AllowedYesNoPending = StaffArrControlledFieldCatalog.YesNoPendingKeys;
    private static readonly IReadOnlySet<string> AllowedMedicalAttention = StaffArrControlledFieldCatalog.MedicalAttentionKeys;
    private static readonly IReadOnlySet<string> AllowedPpeConcerns = StaffArrControlledFieldCatalog.PpeConcernKeys;
    private static readonly IReadOnlySet<string> AllowedFollowUpStates = StaffArrControlledFieldCatalog.FollowUpKeys;
    private static readonly IReadOnlySet<string> AllowedTrainingReviewReasons = StaffArrControlledFieldCatalog.TrainingReviewReasonKeys;
    private static readonly IReadOnlySet<string> AllowedIncidentNoteTypes = StaffArrControlledFieldCatalog.IncidentNoteTypeKeys;
    private static readonly IReadOnlySet<string> AllowedIncidentNoteStatuses = StaffArrControlledFieldCatalog.IncidentNoteStatusKeys;

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
            RelatedAssetReference = NormalizeOptional(request.RelatedAssetReference, 2048, "Related asset"),
            RelatedWorkOrderReference = NormalizeOptional(request.RelatedWorkOrderReference, 128, "Related work order"),
            RelatedRouteReference = NormalizeOptional(request.RelatedRouteReference, 128, "Related route or trip"),
            RelatedSupplierReference = NormalizeOptional(request.RelatedSupplierReference, 2048, "Related supplier or party"),
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
                IdempotentReplay: true,
                BuildSourceSnapshot(existing));
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
            IdempotentReplay: false,
            BuildSourceSnapshot(entity));
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

    public async Task<PersonnelIncidentDetailResponse> UpdateIncidentStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        UpdatePersonnelIncidentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonnelIncidents
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("incidents.not_found", "Incident was not found.", 404);
        }

        var nextStatus = NormalizeStatus(request.Status, entity.Status);
        if (string.Equals(entity.Status, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return await GetIncidentAsync(tenantId, incidentId, cancellationToken);
        }

        entity.Status = nextStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "incident.status_update",
            tenantId,
            actorUserId,
            "personnel_incident",
            incidentId.ToString(),
            "Succeeded",
            nextStatus,
            cancellationToken: cancellationToken);

        return await GetIncidentAsync(tenantId, incidentId, cancellationToken);
    }

    public async Task<PersonnelIncidentDetailResponse> CreateIncidentNoteAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CreateIncidentNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await RequireIncidentAsync(tenantId, incidentId, cancellationToken);
        var noteTypeKey = NormalizeIncidentNoteTypeKey(request.NoteTypeKey);
        var subject = NormalizeTitle(request.Subject);
        var body = NormalizeDescription(request.Body);
        ValidateTimestampNotFuture(request.DueAt, "Corrective action due date", allowFuture: true);

        var now = DateTimeOffset.UtcNow;
        var note = new IncidentNote
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IncidentId = incidentId,
            NoteTypeKey = noteTypeKey,
            Subject = subject,
            Body = body,
            Status = "open",
            DueAt = request.DueAt,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.IncidentNotes.Add(note);
        incident.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            noteTypeKey == "corrective_action" ? "incident.corrective_action.create" : "incident.note.create",
            tenantId,
            actorUserId,
            "personnel_incident",
            incidentId.ToString(),
            "Succeeded",
            note.NoteTypeKey,
            cancellationToken: cancellationToken);

        return await GetIncidentAsync(tenantId, incidentId, cancellationToken);
    }

    public async Task<PersonnelIncidentDetailResponse> UpdateIncidentNoteStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        Guid noteId,
        UpdateIncidentNoteStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await RequireIncidentAsync(tenantId, incidentId, cancellationToken);
        var note = await db.IncidentNotes
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IncidentId == incidentId && x.Id == noteId, cancellationToken);

        if (note is null)
        {
            throw new StlApiException("incident_notes.not_found", "Incident note was not found.", 404);
        }

        if (!string.Equals(note.NoteTypeKey, "corrective_action", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "incident_notes.validation",
                "Only corrective actions can be marked completed.",
                400);
        }

        var nextStatus = NormalizeIncidentNoteStatus(request.Status);
        note.Status = nextStatus;
        note.CompletedAt = string.Equals(nextStatus, "completed", StringComparison.OrdinalIgnoreCase)
            ? DateTimeOffset.UtcNow
            : null;
        note.UpdatedAt = DateTimeOffset.UtcNow;
        incident.UpdatedAt = note.UpdatedAt;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident.corrective_action.status_update",
            tenantId,
            actorUserId,
            "personnel_incident",
            incidentId.ToString(),
            "Succeeded",
            nextStatus,
            cancellationToken: cancellationToken);

        return await GetIncidentAsync(tenantId, incidentId, cancellationToken);
    }

    public async Task<PersonnelIncidentDetailResponse> CreateIncidentAttachmentAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CreateIncidentAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var incident = await RequireIncidentAsync(tenantId, incidentId, cancellationToken);
        var title = NormalizeTitle(request.Title);
        var fileName = NormalizeFileName(request.FileName);
        var contentType = NormalizeContentType(request.ContentType);
        var description = NormalizeOptional(request.Description, 1024, "Attachment description");
        var contentBytes = DecodeBase64(request.ContentBase64);
        if (contentBytes.Length == 0)
        {
            throw new StlApiException("incident_attachments.validation", "Attachment content is required.", 400);
        }

        if (contentBytes.Length > MaxIncidentAttachmentBytes)
        {
            throw new StlApiException(
                "incident_attachments.validation",
                $"Attachment file must be {MaxIncidentAttachmentBytes / (1024 * 1024)} MB or smaller.",
                400);
        }

        var attachmentId = Guid.NewGuid();
        await using var contentStream = new MemoryStream(contentBytes);
        var storageKey = await storage.SaveAsync(
            tenantId,
            incident.PersonId,
            attachmentId,
            fileName,
            contentStream,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var attachment = new IncidentAttachment
        {
            Id = attachmentId,
            TenantId = tenantId,
            IncidentId = incidentId,
            Title = title,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Description = description,
            UploadedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.IncidentAttachments.Add(attachment);
        incident.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "incident.attachment.upload",
            tenantId,
            actorUserId,
            "personnel_incident",
            incidentId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetIncidentAsync(tenantId, incidentId, cancellationToken);
    }

    public async Task<(IncidentAttachmentSummaryResponse Metadata, FileStream Stream)> OpenIncidentAttachmentContentAsync(
        Guid tenantId,
        Guid incidentId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var attachment = await db.IncidentAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.IncidentId == incidentId && x.Id == attachmentId,
                cancellationToken);

        if (attachment is null)
        {
            throw new StlApiException("incident_attachments.not_found", "Incident attachment was not found.", 404);
        }

        if (!storage.TryOpenReadStream(attachment.StorageKey, out var stream) || stream is null)
        {
            throw new StlApiException("incident_attachments.content_missing", "Attachment content is unavailable.", 404);
        }

        return (MapAttachmentSummary(attachment), stream);
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
        var notes = await db.IncidentNotes
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IncidentId == incidentId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        var attachments = await db.IncidentAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IncidentId == incidentId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return MapDetail(
            entity,
            routing,
            notes.Select(MapNoteSummary).ToList(),
            attachments.Select(MapAttachmentSummary).ToList());
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

    private static string NormalizeIncidentNoteTypeKey(string noteTypeKey)
    {
        var normalized = noteTypeKey.Trim().ToLowerInvariant();
        if (!AllowedIncidentNoteTypes.Contains(normalized))
        {
            throw new StlApiException(
                "incident_notes.validation",
                $"Note type must be one of: {string.Join(", ", AllowedIncidentNoteTypes.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeIncidentNoteStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedIncidentNoteStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "incident_notes.validation",
                $"Status must be one of: {string.Join(", ", AllowedIncidentNoteStatuses.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeFileName(string fileName)
    {
        var trimmed = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("incident_attachments.validation", "File name is required.", 400);
        }

        return trimmed.Length > 255 ? trimmed[..255] : trimmed;
    }

    private static string NormalizeContentType(string contentType)
    {
        var trimmed = contentType.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "application/octet-stream";
        }

        return trimmed.Length > 128 ? trimmed[..128] : trimmed;
    }

    private static byte[] DecodeBase64(string contentBase64)
    {
        try
        {
            return Convert.FromBase64String(contentBase64);
        }
        catch (FormatException)
        {
            throw new StlApiException("incident_attachments.validation", "Attachment content must be valid base64.", 400);
        }
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

    private async Task<PersonnelIncident> RequireIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        var incident = await db.PersonnelIncidents
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken);

        if (incident is null)
        {
            throw new StlApiException("incidents.not_found", "Incident was not found.", 404);
        }

        return incident;
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
        IReadOnlySet<string> allowedValues,
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
        IReadOnlySet<string> allowedValues,
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
            entity.SourceReferenceKey,
            BuildSourceSnapshot(entity));

    private static PersonnelIncidentDetailResponse MapDetail(
        PersonnelIncident entity,
        IncidentTrainarrRoutingResponse? routing,
        IReadOnlyList<IncidentNoteSummaryResponse>? notes = null,
        IReadOnlyList<IncidentAttachmentSummaryResponse>? attachments = null) =>
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
            entity.SourceReferenceKey,
            BuildSourceSnapshot(entity),
            notes,
            attachments);

    private static IncidentNoteSummaryResponse MapNoteSummary(IncidentNote entity) =>
        new(
            entity.Id,
            entity.IncidentId,
            entity.NoteTypeKey,
            entity.Subject,
            entity.Body,
            entity.Status,
            entity.DueAt,
            entity.CompletedAt,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static IncidentAttachmentSummaryResponse MapAttachmentSummary(IncidentAttachment entity) =>
        new(
            entity.Id,
            entity.IncidentId,
            entity.Title,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Description,
            entity.UploadedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static ProductSourceReferenceSnapshotResponse? BuildSourceSnapshot(PersonnelIncident entity)
    {
        if (string.IsNullOrWhiteSpace(entity.SourceProduct) || entity.SourceIncidentId is not Guid sourceIncidentId)
        {
            return null;
        }

        return new ProductSourceReferenceSnapshotResponse(
            entity.SourceProduct,
            entity.SourceEventKind ?? "product_incident",
            sourceIncidentId.ToString(),
            entity.SourceReferenceKey ?? entity.Title,
            entity.Status,
            entity.ReportedAt,
            entity.UpdatedAt,
            entity.UpdatedAt,
            IsAuthoritative: false);
    }
}
