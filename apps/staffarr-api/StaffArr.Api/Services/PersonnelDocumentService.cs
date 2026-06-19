using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonnelDocumentService(
    StaffArrDbContext db,
    StaffArrDocumentStorageService storage,
    IStaffArrAuditService audit)
{
    private const long MaxDocumentBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedDocumentTypeKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "id_verification",
        "employment_contract",
        "certification_copy",
        "medical_form",
        "policy_acknowledgment",
        "offer_letter",
        "employment_agreement",
        "handbook_acknowledgment",
        "emergency_contact",
        "job_description_acknowledgment",
        "corrective_action",
        "performance_review",
        "leave_paperwork",
        "termination_paperwork",
        "work_authorization",
        "medical_accommodation",
        "eeo_self_id",
        "other"
    };

    private static readonly HashSet<string> AllowedAccessLevels = new(StringComparer.OrdinalIgnoreCase)
    {
        "employee",
        "manager",
        "hr",
        "restricted"
    };

    private static readonly HashSet<string> AllowedRetentionCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "personnel_file",
        "employment_eligibility",
        "discipline",
        "performance",
        "leave",
        "termination",
        "medical",
        "eeo",
        "other"
    };

    public async Task<PersonnelDocumentDetailResponse> CreateDocumentAsync(
        Guid tenantId,
        Guid personId,
        Guid actorUserId,
        CreatePersonnelDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var documentTypeKey = NormalizeDocumentTypeKey(request.DocumentTypeKey);
        var accessLevel = NormalizeAccessLevel(request.AccessLevel);
        var retentionCategory = NormalizeRetentionCategory(request.RetentionCategory);
        var title = NormalizeTitle(request.Title);
        var fileName = NormalizeFileName(request.FileName);
        var contentType = NormalizeContentType(request.ContentType);
        var description = NormalizeDescription(request.Description);
        ValidateExpiresAt(request.ExpiresAt);

        var contentBytes = DecodeContent(request.ContentBase64);
        if (contentBytes.Length == 0)
        {
            throw new StlApiException("personnel_documents.validation", "Document content is required.", 400);
        }

        if (contentBytes.Length > MaxDocumentBytes)
        {
            throw new StlApiException(
                "personnel_documents.validation",
                $"Document file must be {MaxDocumentBytes / (1024 * 1024)} MB or smaller.",
                400);
        }

        var documentId = Guid.NewGuid();
        await using var contentStream = new MemoryStream(contentBytes);
        var storageKey = await storage.SaveAsync(
            tenantId,
            personId,
            documentId,
            fileName,
            contentStream,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new PersonnelDocument
        {
            Id = documentId,
            TenantId = tenantId,
            PersonId = personId,
            DocumentTypeKey = documentTypeKey,
            AccessLevel = accessLevel,
            RetentionCategory = retentionCategory,
            RestrictedData = accessLevel == "restricted" || retentionCategory is "medical" or "eeo",
            Title = title,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length,
            StorageKey = storageKey,
            Description = description,
            ExpiresAt = request.ExpiresAt,
            Status = "active",
            UploadedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PersonnelDocuments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "personnel_document.upload",
            tenantId,
            actorUserId,
            "personnel_document",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDetail(entity);
    }

    public async Task<IReadOnlyList<PersonnelDocumentSummaryResponse>> ListDocumentsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        return await db.PersonnelDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId && x.Status == "active")
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PersonnelDocumentSummaryResponse(
                x.Id,
                x.PersonId,
                x.DocumentTypeKey,
                x.AccessLevel,
                x.RetentionCategory,
                x.RestrictedData,
                x.Title,
                x.FileName,
                x.ContentType,
                x.SizeBytes,
                x.Description,
                x.ExpiresAt,
                x.Status,
                x.UploadedByUserId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonnelDocumentDetailResponse> GetDocumentAsync(
        Guid tenantId,
        Guid personId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonnelDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PersonId == personId && x.Id == documentId,
                cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("personnel_documents.not_found", "Personnel document was not found.", 404);
        }

        return MapDetail(entity);
    }

    public async Task<(PersonnelDocumentDetailResponse Metadata, FileStream Stream)> OpenDocumentContentAsync(
        Guid tenantId,
        Guid personId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonnelDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PersonId == personId && x.Id == documentId,
                cancellationToken);

        if (entity is null)
        {
            throw new StlApiException("personnel_documents.not_found", "Personnel document was not found.", 404);
        }

        if (!storage.TryOpenReadStream(entity.StorageKey, out var stream) || stream is null)
        {
            throw new StlApiException("personnel_documents.content_missing", "Document content is unavailable.", 404);
        }

        return (MapDetail(entity), stream);
    }

    private async Task EnsurePersonExistsAsync(Guid tenantId, Guid personId, CancellationToken cancellationToken)
    {
        var personExists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!personExists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private static string NormalizeDocumentTypeKey(string documentTypeKey)
    {
        var normalized = documentTypeKey.Trim().ToLowerInvariant();
        if (!AllowedDocumentTypeKeys.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_documents.validation",
                $"Document type must be one of: {string.Join(", ", AllowedDocumentTypeKeys.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeAccessLevel(string accessLevel)
    {
        var normalized = accessLevel.Trim().ToLowerInvariant();
        if (!AllowedAccessLevels.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_documents.validation",
                $"Access level must be one of: {string.Join(", ", AllowedAccessLevels.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRetentionCategory(string retentionCategory)
    {
        var normalized = retentionCategory.Trim().ToLowerInvariant();
        if (!AllowedRetentionCategories.Contains(normalized))
        {
            throw new StlApiException(
                "personnel_documents.validation",
                $"Retention category must be one of: {string.Join(", ", AllowedRetentionCategories.OrderBy(x => x))}.",
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
                "personnel_documents.validation",
                "Document title must be at least 4 characters.",
                400);
        }

        if (trimmed.Length > 200)
        {
            throw new StlApiException(
                "personnel_documents.validation",
                "Document title must be 200 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeFileName(string fileName)
    {
        var trimmed = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new StlApiException("personnel_documents.validation", "File name is required.", 400);
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

    private static string? NormalizeDescription(string? description)
    {
        if (description is null)
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > 1024)
        {
            throw new StlApiException(
                "personnel_documents.validation",
                "Document description must be 1024 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static void ValidateExpiresAt(DateTimeOffset? expiresAt)
    {
        if (expiresAt is DateTimeOffset value && value <= DateTimeOffset.UtcNow.AddMinutes(-1))
        {
            throw new StlApiException(
                "personnel_documents.validation",
                "Document expiration must be in the future.",
                400);
        }
    }

    private static byte[] DecodeContent(string contentBase64)
    {
        try
        {
            return Convert.FromBase64String(contentBase64);
        }
        catch (FormatException)
        {
            throw new StlApiException(
                "personnel_documents.validation",
                "Document content must be valid base64.",
                400);
        }
    }

    private static PersonnelDocumentDetailResponse MapDetail(PersonnelDocument entity) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.DocumentTypeKey,
            entity.AccessLevel,
            entity.RetentionCategory,
            entity.RestrictedData,
            entity.Title,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Description,
            entity.ExpiresAt,
            entity.Status,
            entity.UploadedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);
}
