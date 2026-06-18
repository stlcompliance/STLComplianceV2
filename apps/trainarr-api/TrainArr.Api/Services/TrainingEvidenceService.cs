using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Contracts;

using TrainArr.Api.Data;

using TrainArr.Api.Entities;

using STLCompliance.Shared.Contracts;



namespace TrainArr.Api.Services;



public sealed class TrainingEvidenceService(

    TrainArrDbContext db,

    TrainArrEvidenceStorageService storage,

    TrainingAcknowledgementPublicationService acknowledgementPublicationService,

    TrainArrTenantSettingsService tenantSettingsService,

    ITrainArrAuditService audit)

{

    public async Task<IReadOnlyList<TrainingEvidenceResponse>> ListForAssignmentAsync(

        Guid tenantId,

        Guid assignmentId,

        CancellationToken cancellationToken = default)

    {

        await EnsureAssignmentExistsAsync(tenantId, assignmentId, cancellationToken);



        return await db.TrainingEvidence

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId)

            .OrderByDescending(x => x.CreatedAt)

            .Select(x => MapResponse(x))

            .ToListAsync(cancellationToken);

    }



    public async Task<TrainingEvidenceResponse> CreateForAssignmentAsync(

        Guid tenantId,

        Guid actorUserId,

        Guid assignmentId,

        CreateTrainingEvidenceRequest request,

        CancellationToken cancellationToken = default)

    {

        var assignment = await db.TrainingAssignments

            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assignmentId, cancellationToken);

        if (assignment is null)

        {

            throw new StlApiException("assignments.not_found", "Training assignment was not found.", 404);

        }



        if (assignment.Status is "completed" or "cancelled")

        {

            throw new StlApiException(

                "evidence.assignment_closed",

                "Evidence cannot be added to a completed or cancelled assignment.",

                409);

        }

        await acknowledgementPublicationService.SyncMirrorFromStaffArrAsync(assignment, cancellationToken);
        if (TrainingAcknowledgementPublicationService.RequiresAcknowledgement(assignment))
        {
            throw new StlApiException(
                "evidence.acknowledgement_required",
                "Acknowledge this training assignment in StaffArr before uploading evidence.",
                409);
        }



        var tenantSettings = await tenantSettingsService.LoadPayloadAsync(tenantId, cancellationToken);

        var evidenceTypeKey = NormalizeEvidenceTypeKey(
            request.EvidenceTypeKey,
            tenantSettings.EvidenceRecords.AllowedEvidenceTypes);

        var fileName = NormalizeFileName(request.FileName);

        var contentType = NormalizeContentType(request.ContentType);

        var notes = NormalizeNotes(request.Notes);

        var contentBytes = DecodeContent(request.ContentBase64);



        if (contentBytes.Length == 0)

        {

            throw new StlApiException("evidence.validation", "Evidence content is required.", 400);

        }



        var maxEvidenceBytes = tenantSettings.EvidenceRecords.MaxEvidenceFileSizeMb * 1024L * 1024L;

        if (contentBytes.Length > maxEvidenceBytes)

        {

            throw new StlApiException(

                "evidence.validation",

                $"Evidence file must be {tenantSettings.EvidenceRecords.MaxEvidenceFileSizeMb} MB or smaller.",

                400);

        }



        var evidenceId = Guid.NewGuid();

        await using var contentStream = new MemoryStream(contentBytes);

        var storageKey = await storage.SaveAsync(

            tenantId,

            assignmentId,

            evidenceId,

            fileName,

            contentStream,

            cancellationToken);



        var entity = new TrainingEvidence

        {

            Id = evidenceId,

            TenantId = tenantId,

            TrainingAssignmentId = assignmentId,

            EvidenceTypeKey = evidenceTypeKey,

            FileName = fileName,

            ContentType = contentType,

            SizeBytes = contentBytes.Length,

            StorageKey = storageKey,

            Notes = notes,

            UploadedByUserId = actorUserId,

            CreatedAt = DateTimeOffset.UtcNow

        };



        if (assignment.Status == "assigned")

        {

            assignment.Status = "in_progress";

            assignment.UpdatedAt = DateTimeOffset.UtcNow;

        }



        db.TrainingEvidence.Add(entity);

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(

            "training_evidence.create",

            tenantId,

            actorUserId,

            "training_evidence",

            entity.Id.ToString(),

            "Succeeded",

            cancellationToken: cancellationToken);



        return MapResponse(entity);

    }



    private async Task EnsureAssignmentExistsAsync(

        Guid tenantId,

        Guid assignmentId,

        CancellationToken cancellationToken)

    {

        var exists = await db.TrainingAssignments.AnyAsync(

            x => x.TenantId == tenantId && x.Id == assignmentId,

            cancellationToken);

        if (!exists)

        {

            throw new StlApiException("assignments.not_found", "Training assignment was not found.", 404);

        }

    }



    private static TrainingEvidenceResponse MapResponse(TrainingEvidence entity) =>

        new(

            entity.Id,

            entity.TrainingAssignmentId,

            entity.EvidenceTypeKey,

            entity.FileName,

            entity.ContentType,

            entity.SizeBytes,

            entity.Notes,

            entity.UploadedByUserId,

            entity.CreatedAt);



    private static byte[] DecodeContent(string contentBase64)

    {

        if (string.IsNullOrWhiteSpace(contentBase64))

        {

            return [];

        }



        var payload = contentBase64.Trim();

        var commaIndex = payload.IndexOf(',');

        if (commaIndex >= 0 && payload[..commaIndex].Contains("base64", StringComparison.OrdinalIgnoreCase))

        {

            payload = payload[(commaIndex + 1)..];

        }



        try

        {

            return Convert.FromBase64String(payload);

        }

        catch (FormatException)

        {

            throw new StlApiException("evidence.validation", "Evidence content must be valid base64.", 400);

        }

    }



    private static string NormalizeEvidenceTypeKey(
        string evidenceTypeKey,
        IReadOnlyList<string> allowedEvidenceTypes)

    {

        var normalized = evidenceTypeKey.Trim().ToLowerInvariant();

        if (normalized.Length < 3 || normalized.Length > 64)

        {

            throw new StlApiException(

                "evidence.validation",

                "Evidence type key must be between 3 and 64 characters.",

                400);

        }

        if (!allowedEvidenceTypes.Contains(normalized, StringComparer.OrdinalIgnoreCase))

        {

            throw new StlApiException(

                "evidence.validation",

                $"Evidence type must be one of: {string.Join(", ", allowedEvidenceTypes.OrderBy(x => x))}.",

                400);

        }



        return normalized;

    }



    private static string NormalizeFileName(string fileName)

    {

        var trimmed = fileName.Trim();

        if (trimmed.Length < 1 || trimmed.Length > 255)

        {

            throw new StlApiException(

                "evidence.validation",

                "File name must be between 1 and 255 characters.",

                400);

        }



        return trimmed;

    }



    private static string NormalizeContentType(string contentType)

    {

        var trimmed = contentType.Trim().ToLowerInvariant();

        if (trimmed.Length < 3 || trimmed.Length > 128)

        {

            throw new StlApiException(

                "evidence.validation",

                "Content type must be between 3 and 128 characters.",

                400);

        }



        return trimmed;

    }



    private static string? NormalizeNotes(string? notes)

    {

        if (string.IsNullOrWhiteSpace(notes))

        {

            return null;

        }



        var trimmed = notes.Trim();

        if (trimmed.Length > 1024)

        {

            throw new StlApiException(

                "evidence.validation",

                "Notes must be 1024 characters or fewer.",

                400);

        }



        return trimmed;

    }

}


