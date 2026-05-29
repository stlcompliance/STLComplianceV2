using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class TripCaptureAttachmentService(
    RoutArrDbContext db,
    TripService tripService,
    RoutArrAuthorizationService authorization,
    RoutArrCaptureAttachmentStorageService storage,
    IRoutArrAuditService audit)
{
    public const string UploadAction = "trip_capture_attachment.upload";
    public const string ListAction = "trip_capture_attachment.list";
    public const string DownloadAction = "trip_capture_attachment.download";

    public async Task<TripCaptureAttachmentResponse> UploadAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        string subjectType,
        Guid subjectId,
        UploadTripCaptureAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofWrite(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: true);

        var normalizedSubjectType = TripCaptureAttachmentRules.NormalizeSubjectType(subjectType);
        await EnsureSubjectExistsAsync(tenantId, tripId, normalizedSubjectType, subjectId, cancellationToken);

        var attachmentKind = TripCaptureAttachmentRules.NormalizeAttachmentKind(request.AttachmentKind);
        var fileName = TripCaptureAttachmentRules.NormalizeFileName(request.FileName);
        var contentType = TripCaptureAttachmentRules.NormalizeContentType(request.ContentType);
        var notes = TripCaptureAttachmentRules.NormalizeNotes(request.Notes);
        var contentBytes = TripCaptureAttachmentRules.DecodeContent(request.ContentBase64);
        TripCaptureAttachmentRules.ValidateContent(contentBytes);

        var attachmentId = Guid.NewGuid();
        await using var contentStream = new MemoryStream(contentBytes);
        var storageKey = await storage.SaveAsync(
            tenantId,
            tripId,
            attachmentId,
            fileName,
            contentStream,
            cancellationToken);

        var entity = new TripCaptureAttachment
        {
            Id = attachmentId,
            TenantId = tenantId,
            TripId = tripId,
            SubjectType = normalizedSubjectType,
            SubjectId = subjectId,
            AttachmentKind = attachmentKind,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Notes = notes,
            CapturedByPersonId = actorPersonId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.TripCaptureAttachments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            UploadAction,
            tenantId,
            actorUserId,
            "trip_capture_attachment",
            entity.Id.ToString(),
            $"{normalizedSubjectType}:{attachmentKind}",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<TripCaptureAttachmentListResponse> ListForSubjectAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: false);

        var normalizedSubjectType = TripCaptureAttachmentRules.NormalizeSubjectType(subjectType);
        await EnsureSubjectExistsAsync(tenantId, tripId, normalizedSubjectType, subjectId, cancellationToken);

        var items = await db.TripCaptureAttachments
            .AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.TripId == tripId
                && x.SubjectType == normalizedSubjectType
                && x.SubjectId == subjectId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        await audit.WriteAsync(
            ListAction,
            tenantId,
            actorUserId,
            "trip_capture_attachment",
            subjectId.ToString(),
            items.Count.ToString(),
            cancellationToken: cancellationToken);

        return new TripCaptureAttachmentListResponse(
            tripId,
            normalizedSubjectType,
            subjectId,
            items.Select(Map).ToList());
    }

    public async Task<(TripCaptureAttachment Entity, FileStream Stream)> OpenContentAsync(
        ClaimsPrincipal principal,
        Guid tripId,
        string subjectType,
        Guid subjectId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireTripProofRead(principal);
        var tenantId = principal.GetTenantId();
        var actorUserId = principal.GetUserId();

        var trip = await tripService.GetAsync(tenantId, tripId, cancellationToken);
        EnsureTripProofAccess(principal, trip.AssignedDriverPersonId, write: false);

        var normalizedSubjectType = TripCaptureAttachmentRules.NormalizeSubjectType(subjectType);

        var entity = await db.TripCaptureAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.TenantId == tenantId
                    && x.TripId == tripId
                    && x.SubjectType == normalizedSubjectType
                    && x.SubjectId == subjectId
                    && x.Id == attachmentId,
                cancellationToken);

        if (entity is null)
        {
            throw new StlApiException(
                "trip_capture_attachment.not_found",
                "Capture attachment was not found.",
                404);
        }

        if (!storage.TryOpenReadStream(entity.StorageKey, out var stream) || stream is null)
        {
            throw new StlApiException(
                "trip_capture_attachment.content_missing",
                "Attachment file is missing from storage.",
                404);
        }

        await audit.WriteAsync(
            DownloadAction,
            tenantId,
            actorUserId,
            "trip_capture_attachment",
            entity.Id.ToString(),
            entity.FileName,
            cancellationToken: cancellationToken);

        return (entity, stream);
    }

    public async Task<IReadOnlyList<TripCaptureAttachmentResponse>> ListForTripAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default) =>
        await db.TripCaptureAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);

    public async Task<TripCaptureAttachmentState> BuildAttachmentStateAsync(
        Guid tenantId,
        Guid tripId,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.TripCaptureAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => new
            {
                x.SubjectType,
                x.SubjectId,
                x.AttachmentKind,
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return TripCaptureAttachmentState.Empty;
        }

        var proofIdsByType = await db.TripProofRecords
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => new { x.Id, x.ProofType })
            .ToListAsync(cancellationToken);

        var dvirIdsByPhase = await db.TripDvirInspections
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TripId == tripId)
            .Select(x => new { x.Id, x.Phase })
            .ToListAsync(cancellationToken);

        var pickupProofIds = proofIdsByType
            .Where(x => string.Equals(x.ProofType, TripProofTypes.Pickup, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();
        var deliveryProofIds = proofIdsByType
            .Where(x => string.Equals(x.ProofType, TripProofTypes.Delivery, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();
        var preTripDvirIds = dvirIdsByPhase
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PreTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();
        var postTripDvirIds = dvirIdsByPhase
            .Where(x => string.Equals(x.Phase, DvirInspectionPhases.PostTrip, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();

        bool HasProofAttachment(HashSet<Guid> proofIds, string kind) =>
            rows.Any(x =>
                string.Equals(x.SubjectType, TripCaptureAttachmentSubjects.Proof, StringComparison.OrdinalIgnoreCase)
                && proofIds.Contains(x.SubjectId)
                && string.Equals(x.AttachmentKind, kind, StringComparison.OrdinalIgnoreCase));

        bool HasDvirAttachment(HashSet<Guid> dvirIds, string kind) =>
            rows.Any(x =>
                string.Equals(x.SubjectType, TripCaptureAttachmentSubjects.Dvir, StringComparison.OrdinalIgnoreCase)
                && dvirIds.Contains(x.SubjectId)
                && string.Equals(x.AttachmentKind, kind, StringComparison.OrdinalIgnoreCase));

        return new TripCaptureAttachmentState(
            HasProofAttachment(pickupProofIds, TripCaptureAttachmentKinds.Photo),
            HasProofAttachment(deliveryProofIds, TripCaptureAttachmentKinds.Photo),
            HasProofAttachment(deliveryProofIds, TripCaptureAttachmentKinds.Signature),
            HasDvirAttachment(preTripDvirIds, TripCaptureAttachmentKinds.Photo),
            HasDvirAttachment(postTripDvirIds, TripCaptureAttachmentKinds.Photo));
    }

    private async Task EnsureSubjectExistsAsync(
        Guid tenantId,
        Guid tripId,
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var exists = subjectType switch
        {
            TripCaptureAttachmentSubjects.Proof => await db.TripProofRecords.AnyAsync(
                x => x.TenantId == tenantId && x.TripId == tripId && x.Id == subjectId,
                cancellationToken),
            TripCaptureAttachmentSubjects.Dvir => await db.TripDvirInspections.AnyAsync(
                x => x.TenantId == tenantId && x.TripId == tripId && x.Id == subjectId,
                cancellationToken),
            _ => false,
        };

        if (!exists)
        {
            throw new StlApiException(
                "trip_capture_attachment.subject_not_found",
                "Proof or DVIR subject was not found for this trip.",
                404);
        }
    }

    private void EnsureTripProofAccess(ClaimsPrincipal principal, string? assignedPersonId, bool write)
    {
        if (!write && authorization.CanViewAllTrips(principal))
        {
            return;
        }

        var actorPersonId = principal.GetPersonId().ToString();
        if (!string.IsNullOrWhiteSpace(assignedPersonId)
            && string.Equals(assignedPersonId.Trim(), actorPersonId, StringComparison.Ordinal))
        {
            return;
        }

        throw new StlApiException(
            write ? "trip_capture_attachment.not_assigned" : "auth.forbidden",
            write
                ? "Only the assigned driver can upload capture attachments for this trip."
                : "You can only read capture attachments for trips assigned to you.",
            403);
    }

    private static TripCaptureAttachmentResponse Map(TripCaptureAttachment entity) =>
        new(
            entity.Id,
            entity.TripId,
            entity.SubjectType,
            entity.SubjectId,
            entity.AttachmentKind,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Notes,
            entity.CapturedByPersonId,
            entity.CreatedAt);
}

public sealed record TripCaptureAttachmentState(
    bool HasPickupProofPhoto,
    bool HasDeliveryProofPhoto,
    bool HasDeliverySignature,
    bool HasPreTripDvirPhoto,
    bool HasPostTripDvirPhoto)
{
    public static TripCaptureAttachmentState Empty { get; } = new(false, false, false, false, false);
}
