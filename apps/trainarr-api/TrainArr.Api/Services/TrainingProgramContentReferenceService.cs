using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingProgramContentReferenceService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingProgramContentReferenceResponse>> ListAsync(
        Guid tenantId,
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProgramExistsAsync(tenantId, programId, cancellationToken);

        return await db.TrainingProgramContentReferences
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TrainingProgramId == programId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new TrainingProgramContentReferenceResponse(
                x.Id,
                x.TrainingProgramId,
                x.ContentType,
                x.Title,
                x.ReferenceValue,
                x.Notes,
                x.LocaleTag,
                x.CreatedByUserId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingProgramContentReferenceResponse> AttachAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid programId,
        CreateTrainingProgramContentReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureProgramExistsAsync(tenantId, programId, cancellationToken);

        var contentType = NormalizeContentType(request.ContentType);
        var title = NormalizeTitle(request.Title);
        var referenceValue = NormalizeReferenceValue(request.ReferenceValue);
        var notes = NormalizeNotes(request.Notes);
        var localeTag = NormalizeLocaleTag(request.LocaleTag);

        var duplicate = await db.TrainingProgramContentReferences.AnyAsync(
            x => x.TenantId == tenantId
                && x.TrainingProgramId == programId
                && x.ContentType == contentType
                && x.ReferenceValue == referenceValue
                && x.LocaleTag == localeTag,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "program_content_reference.duplicate",
                "This content reference is already attached to the program.",
                409);
        }

        var entity = new TrainingProgramContentReference
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingProgramId = programId,
            ContentType = contentType,
            Title = title,
            ReferenceValue = referenceValue,
            Notes = notes,
            LocaleTag = localeTag,
            CreatedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.TrainingProgramContentReferences.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "program_content_reference.attach",
            tenantId,
            actorUserId,
            "training_program",
            programId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task RemoveAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid programId,
        Guid contentReferenceId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProgramExistsAsync(tenantId, programId, cancellationToken);

        var entity = await db.TrainingProgramContentReferences.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.TrainingProgramId == programId
                && x.Id == contentReferenceId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException(
                "program_content_reference.not_found",
                "Program content reference was not found.",
                404);
        }

        db.TrainingProgramContentReferences.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "program_content_reference.detach",
            tenantId,
            actorUserId,
            "training_program",
            programId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
    }

    private async Task EnsureProgramExistsAsync(
        Guid tenantId,
        Guid programId,
        CancellationToken cancellationToken)
    {
        var exists = await db.TrainingPrograms.AnyAsync(
            x => x.TenantId == tenantId && x.Id == programId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException(
                "training_programs.not_found",
                "Training program was not found.",
                404);
        }
    }

    private static TrainingProgramContentReferenceResponse Map(TrainingProgramContentReference entity) =>
        new(
            entity.Id,
            entity.TrainingProgramId,
            entity.ContentType,
            entity.Title,
            entity.ReferenceValue,
            entity.Notes,
            entity.LocaleTag,
            entity.CreatedByUserId,
            entity.CreatedAt);

    private static string NormalizeContentType(string contentType)
    {
        var trimmed = contentType.Trim().ToLowerInvariant();
        if (!TrainingProgramContentReferenceTypeSet.Allowed.Contains(trimmed))
        {
            throw new StlApiException(
                "program_content_reference.validation",
                "Unsupported training content type.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeTitle(string title)
    {
        var trimmed = title.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 200)
        {
            throw new StlApiException(
                "program_content_reference.validation",
                "Content title must be between 2 and 200 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeReferenceValue(string referenceValue)
    {
        var trimmed = referenceValue.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "program_content_reference.validation",
                "Content reference value must be between 2 and 1024 characters.",
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
                "program_content_reference.validation",
                "Notes must be 1024 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeLocaleTag(string? localeTag)
    {
        if (string.IsNullOrWhiteSpace(localeTag))
        {
            return null;
        }

        var trimmed = localeTag.Trim().ToLowerInvariant();
        if (trimmed.Length is < 2 or > 32)
        {
            throw new StlApiException(
                "program_content_reference.validation",
                "Locale tag must be between 2 and 32 characters.",
                400);
        }

        return trimmed;
    }
}

public static class TrainingProgramContentReferenceTypeSet
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.UploadedPdf,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.UploadedVideo,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.ExternalUrl,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.InternalDocumentReference,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.PolicyDocument,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.ComplianceCoreCitation,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.MaintainArrAssetProcedure,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.StaffArrPolicy,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.SupplyArrVendorDocument,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.EmbeddedTextLesson,
        global::TrainArr.Api.Contracts.TrainingProgramContentReferenceTypes.QuizBank,
    };
}
