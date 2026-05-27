using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingCitationService(
    TrainArrDbContext db,
    ComplianceCoreCitationClient citationClient,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingCitationAttachmentResponse>> ListAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        bool includeMetadata,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var attachments = await db.TrainingCitationAttachments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == entityType && x.EntityId == entityId)
            .OrderBy(x => x.CitationKey)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return await MapResponsesAsync(tenantId, attachments, includeMetadata, cancellationToken);
    }

    public async Task<TrainingCitationAttachmentResponse> AttachAsync(
        Guid tenantId,
        Guid actorUserId,
        string entityType,
        Guid entityId,
        AttachTrainingCitationRequest request,
        bool validateWithComplianceCore,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var citationId = request.ComplianceCoreCitationId;
        if (citationId == Guid.Empty)
        {
            throw new StlApiException(
                "citations.validation",
                "Compliance Core citation id is required.",
                400);
        }

        var citationKey = NormalizeCitationKey(request.CitationKey);
        var citationVersion = request.CitationVersion is > 0 ? request.CitationVersion.Value : 1;

        if (validateWithComplianceCore)
        {
            var lookup = await citationClient.LookupAsync(
                new ComplianceCoreCitationLookupPayload(tenantId, [citationId]),
                cancellationToken);
            var match = lookup.FirstOrDefault(x => x.CitationId == citationId);
            if (match is null)
            {
                throw new StlApiException(
                    "citations.not_found",
                    "Citation was not found in Compliance Core for this tenant.",
                    404);
            }

            if (!string.Equals(match.CitationKey, citationKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "citations.key_mismatch",
                    "Citation key does not match the Compliance Core citation record.",
                    400);
            }

            citationVersion = match.VersionNumber;
        }

        var duplicate = await db.TrainingCitationAttachments.AnyAsync(
            x => x.TenantId == tenantId
                && x.EntityType == entityType
                && x.EntityId == entityId
                && x.ComplianceCoreCitationId == citationId,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "citations.duplicate",
                "This citation is already attached to the training entity.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TrainingCitationAttachment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = entityId,
            ComplianceCoreCitationId = citationId,
            CitationKey = citationKey,
            CitationVersion = citationVersion,
            AttachedByUserId = actorUserId,
            CreatedAt = now,
        };

        db.TrainingCitationAttachments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "citation.attach",
            tenantId,
            actorUserId,
            entityType,
            entityId.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var responses = await MapResponsesAsync(tenantId, [entity], includeMetadata: true, cancellationToken);
        return responses[0];
    }

    public async Task RemoveAsync(
        Guid tenantId,
        Guid? actorUserId,
        string entityType,
        Guid entityId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        ValidateEntityType(entityType);
        await EnsureEntityExistsAsync(tenantId, entityType, entityId, cancellationToken);

        var attachment = await db.TrainingCitationAttachments.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.Id == attachmentId
                && x.EntityType == entityType
                && x.EntityId == entityId,
            cancellationToken);
        if (attachment is null)
        {
            throw new StlApiException("citations.not_found", "Citation attachment was not found.", 404);
        }

        db.TrainingCitationAttachments.Remove(attachment);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "citation.detach",
            tenantId,
            actorUserId,
            entityType,
            entityId.ToString(),
            "success",
            cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<TrainingCitationAttachmentResponse>> MapResponsesAsync(
        Guid tenantId,
        IReadOnlyList<TrainingCitationAttachment> attachments,
        bool includeMetadata,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<Guid, ComplianceCoreCitationLookupItem> metadataById =
            new Dictionary<Guid, ComplianceCoreCitationLookupItem>();
        if (includeMetadata && attachments.Count > 0)
        {
            try
            {
                var lookup = await citationClient.LookupAsync(
                    new ComplianceCoreCitationLookupPayload(
                        tenantId,
                        attachments.Select(x => x.ComplianceCoreCitationId).Distinct().ToList()),
                    cancellationToken);
                metadataById = lookup.ToDictionary(x => x.CitationId);
            }
            catch (StlApiException)
            {
                metadataById = new Dictionary<Guid, ComplianceCoreCitationLookupItem>();
            }
        }

        return attachments
            .Select(attachment =>
            {
                TrainingCitationMetadataResponse? metadata = null;
                if (metadataById.TryGetValue(attachment.ComplianceCoreCitationId, out var item))
                {
                    metadata = new TrainingCitationMetadataResponse(
                        item.Label,
                        item.SourceReference,
                        item.Description,
                        item.RegulatoryProgramKey,
                        item.RulePackKey,
                        item.IsActive);
                }

                return new TrainingCitationAttachmentResponse(
                    attachment.Id,
                    attachment.EntityType,
                    attachment.EntityId,
                    attachment.ComplianceCoreCitationId,
                    attachment.CitationKey,
                    attachment.CitationVersion,
                    attachment.CreatedAt,
                    metadata);
            })
            .ToList();
    }

    private async Task EnsureEntityExistsAsync(
        Guid tenantId,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var exists = entityType switch
        {
            TrainingCitationEntityTypes.TrainingDefinition => await db.TrainingDefinitions.AnyAsync(
                x => x.TenantId == tenantId && x.Id == entityId,
                cancellationToken),
            TrainingCitationEntityTypes.TrainingProgram => await db.TrainingPrograms.AnyAsync(
                x => x.TenantId == tenantId && x.Id == entityId,
                cancellationToken),
            TrainingCitationEntityTypes.TrainingAssignment => await db.TrainingAssignments.AnyAsync(
                x => x.TenantId == tenantId && x.Id == entityId,
                cancellationToken),
            _ => false,
        };

        if (!exists)
        {
            throw new StlApiException(
                "citations.entity_not_found",
                "Training entity was not found for citation attachment.",
                404);
        }
    }

    private static void ValidateEntityType(string entityType)
    {
        if (!TrainingCitationEntityTypeExtensions.IsSupported(entityType))
        {
            throw new StlApiException(
                "citations.entity_type_invalid",
                "Entity type must be training_definition, training_program, or training_assignment.",
                400);
        }
    }

    private static string NormalizeCitationKey(string citationKey)
    {
        var trimmed = citationKey.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "citations.validation",
                "Citation key must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }
}

public static class TrainingCitationEntityTypeExtensions
{
    public static bool IsSupported(string entityType) =>
        string.Equals(entityType, TrainingCitationEntityTypes.TrainingDefinition, StringComparison.Ordinal)
        || string.Equals(entityType, TrainingCitationEntityTypes.TrainingProgram, StringComparison.Ordinal)
        || string.Equals(entityType, TrainingCitationEntityTypes.TrainingAssignment, StringComparison.Ordinal);
}
