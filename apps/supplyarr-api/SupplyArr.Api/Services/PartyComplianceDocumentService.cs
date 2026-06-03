using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PartyComplianceDocumentService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    SupplyArrDocumentStorageService storage,
    ISupplyArrAuditService audit)
{
    private const long MaxDocumentBytes = 10 * 1024 * 1024;

    public async Task<IReadOnlyList<PartyComplianceDocumentResponse>> ListForPartyAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePartyExistsAsync(tenantId, externalPartyId, cancellationToken);

        var rows = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExternalPartyId == externalPartyId)
            .OrderBy(x => x.DocumentTypeKey)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<PartyComplianceDocumentResponse> RegisterAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid externalPartyId,
        RegisterPartyComplianceDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePartyExistsAsync(tenantId, externalPartyId, cancellationToken);

        var documentKey = NormalizeDocumentKey(request.DocumentKey);
        var documentTypeKey = NormalizeDocumentTypeKey(request.DocumentTypeKey);
        var latestVersion = await db.PartyComplianceDocuments
            .Where(x => x.TenantId == tenantId
                && x.ExternalPartyId == externalPartyId
                && x.DocumentKey == documentKey)
            .MaxAsync(x => (int?)x.Version, cancellationToken) ?? 0;

        var fileName = NormalizeFileName(request.FileName);
        var contentType = NormalizeContentType(request.ContentType);
        var contentBytes = DecodeOptionalContent(request.ContentBase64);
        if (contentBytes.Length > MaxDocumentBytes)
        {
            throw new StlApiException(
                "party_compliance_document.content_too_large",
                $"Document file must be {MaxDocumentBytes / (1024 * 1024)} MB or smaller.",
                400);
        }

        var documentId = Guid.NewGuid();
        string? storageKey = null;
        if (contentBytes.Length > 0)
        {
            await using var contentStream = new MemoryStream(contentBytes);
            storageKey = await storage.SaveAsync(
                tenantId,
                externalPartyId,
                documentId,
                fileName,
                contentStream,
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PartyComplianceDocument
        {
            Id = documentId,
            TenantId = tenantId,
            ExternalPartyId = externalPartyId,
            DocumentKey = documentKey,
            DocumentTypeKey = documentTypeKey,
            Title = NormalizeTitle(request.Title),
            Version = latestVersion + 1,
            ReviewStatus = PartyComplianceDocumentReviewStatuses.Pending,
            ExpiresAt = request.ExpiresAt,
            EffectiveAt = request.EffectiveAt,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = contentBytes.Length > 0 ? contentBytes.Length : Math.Max(0, request.SizeBytes),
            StorageKey = storageKey,
            Notes = NormalizeNotes(request.Notes),
            UploadedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.PartyComplianceDocuments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "party_compliance_document.register",
            tenantId,
            actorUserId,
            "party_compliance_document",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
        await EnqueueDocumentFactEventAsync(
            tenantId,
            IntegrationOutboxEventKinds.PartyComplianceDocumentRegistered,
            entity,
            $"Compliance document registered: {entity.DocumentKey}",
            cancellationToken);

        return Map(entity);
    }

    public async Task<(PartyComplianceDocumentResponse Metadata, FileStream Stream)> OpenDocumentContentAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PartyComplianceDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == documentId,
                cancellationToken)
            ?? throw new StlApiException("party_compliance_document.not_found", "Compliance document was not found.", 404);

        if (string.IsNullOrWhiteSpace(entity.StorageKey)
            || !storage.TryOpenReadStream(entity.StorageKey, out var stream)
            || stream is null)
        {
            throw new StlApiException(
                "party_compliance_document.content_missing",
                "Compliance document content is unavailable.",
                404);
        }

        return (Map(entity), stream);
    }

    public async Task<PartyComplianceDocumentResponse> ApproveAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, documentId, cancellationToken);
        if (!string.Equals(entity.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "party_compliance_document.not_pending",
                "Only pending documents can be approved.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.ReviewStatus = PartyComplianceDocumentReviewStatuses.Approved;
        entity.ReviewedByUserId = actorUserId;
        entity.ReviewedAt = now;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "party_compliance_document.approve",
            tenantId,
            actorUserId,
            "party_compliance_document",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
        await EnqueueDocumentFactEventAsync(
            tenantId,
            IntegrationOutboxEventKinds.PartyComplianceDocumentApproved,
            entity,
            $"Compliance document approved: {entity.DocumentKey}",
            cancellationToken);

        return Map(entity);
    }

    public async Task<PartyComplianceDocumentResponse> RejectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid documentId,
        RejectPartyComplianceDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, documentId, cancellationToken);
        if (!string.Equals(entity.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "party_compliance_document.not_pending",
                "Only pending documents can be rejected.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.ReviewStatus = PartyComplianceDocumentReviewStatuses.Rejected;
        entity.ReviewedByUserId = actorUserId;
        entity.ReviewedAt = now;
        entity.Notes = string.IsNullOrWhiteSpace(request.Reason)
            ? entity.Notes
            : $"{entity.Notes}\nRejected: {request.Reason.Trim()}".Trim();
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "party_compliance_document.reject",
            tenantId,
            actorUserId,
            "party_compliance_document",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);
        await EnqueueDocumentFactEventAsync(
            tenantId,
            IntegrationOutboxEventKinds.PartyComplianceDocumentRejected,
            entity,
            $"Compliance document rejected: {entity.DocumentKey}",
            cancellationToken);

        return Map(entity);
    }

    internal async Task<bool> HasApprovedRequiredDocumentAsync(
        Guid tenantId,
        Guid externalPartyId,
        string documentTypeKey,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        return await db.PartyComplianceDocuments.AnyAsync(
            x => x.TenantId == tenantId
                && x.ExternalPartyId == externalPartyId
                && x.DocumentTypeKey == documentTypeKey
                && x.ReviewStatus == PartyComplianceDocumentReviewStatuses.Approved
                && (x.ExpiresAt == null || x.ExpiresAt > asOfUtc),
            cancellationToken);
    }

    private async Task EnsurePartyExistsAsync(Guid tenantId, Guid externalPartyId, CancellationToken cancellationToken)
    {
        var exists = await db.ExternalParties.AnyAsync(
            x => x.TenantId == tenantId && x.Id == externalPartyId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }
    }

    private async Task<PartyComplianceDocument> LoadTrackedAsync(
        Guid tenantId,
        Guid documentId,
        CancellationToken cancellationToken) =>
        await db.PartyComplianceDocuments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == documentId,
            cancellationToken)
        ?? throw new StlApiException("party_compliance_document.not_found", "Compliance document was not found.", 404);

    private async Task EnqueueDocumentFactEventAsync(
        Guid tenantId,
        string eventKind,
        PartyComplianceDocument entity,
        string summary,
        CancellationToken cancellationToken) =>
        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            eventKind,
            "party_compliance_document",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.ExternalPartyId),
            cancellationToken: cancellationToken);

    private static PartyComplianceDocumentResponse Map(PartyComplianceDocument entity) =>
        new(
            entity.Id,
            entity.ExternalPartyId,
            entity.DocumentKey,
            entity.DocumentTypeKey,
            entity.Title,
            entity.Version,
            entity.ReviewStatus,
            entity.ExpiresAt,
            entity.EffectiveAt,
            entity.FileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeDocumentKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("party_compliance_document.key.required", "Document key is required.", 400);
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeDocumentTypeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("party_compliance_document.type.required", "Document type key is required.", 400);
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeTitle(string value) =>
        string.IsNullOrWhiteSpace(value) ? "Compliance document" : value.Trim();

    private static string NormalizeFileName(string value) =>
        string.IsNullOrWhiteSpace(value) ? "document.pdf" : value.Trim();

    private static string NormalizeContentType(string value) =>
        string.IsNullOrWhiteSpace(value) ? "application/pdf" : value.Trim();

    private static string NormalizeNotes(string? value) => value?.Trim() ?? string.Empty;

    private static byte[] DecodeOptionalContent(string? contentBase64)
    {
        if (string.IsNullOrWhiteSpace(contentBase64))
        {
            return [];
        }

        try
        {
            return Convert.FromBase64String(contentBase64.Trim());
        }
        catch (FormatException)
        {
            throw new StlApiException(
                "party_compliance_document.invalid_content",
                "Document content must be valid base64.",
                400);
        }
    }
}
