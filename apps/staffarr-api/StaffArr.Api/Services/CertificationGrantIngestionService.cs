using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class CertificationGrantIngestionService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<CertificationGrantIngestionResponse> IngestAsync(
        IngestCertificationGrantRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(request.TenantId, request.PersonId, cancellationToken);

        var qualificationKey = NormalizeQualificationKey(request.QualificationKey);
        var qualificationName = NormalizeQualificationName(request.QualificationName);
        var trainingDefinitionName = NormalizeTrainingDefinitionName(request.TrainingDefinitionName);
        var notes = NormalizeNotes(request.Notes, trainingDefinitionName);
        var grantedAt = request.GrantedAt == default ? DateTimeOffset.UtcNow : request.GrantedAt;

        var existingByPublication = await db.PersonCertifications.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.ExternalPublicationId == request.TrainarrPublicationId,
            cancellationToken);
        if (existingByPublication is not null)
        {
            var existingDefinition = await db.CertificationDefinitions.AsNoTracking()
                .FirstAsync(x => x.Id == existingByPublication.CertificationDefinitionId, cancellationToken);
            return MapResponse(existingByPublication, existingDefinition);
        }

        var definition = await ResolveOrCreateDefinitionAsync(
            request.TenantId,
            qualificationKey,
            qualificationName,
            cancellationToken);

        var expiresAt = request.ExpiresAt;
        if (expiresAt is null && definition.DefaultValidityDays is int validityDays)
        {
            expiresAt = grantedAt.AddDays(validityDays);
        }

        if (expiresAt is not null && expiresAt <= grantedAt)
        {
            throw new StlApiException(
                "certification_grants.validation",
                "Expiration must be after the grant date.",
                400);
        }

        var duplicateActive = await db.PersonCertifications.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.PersonId == request.PersonId
                && x.CertificationDefinitionId == definition.Id
                && x.Status == "active",
            cancellationToken);
        if (duplicateActive is not null)
        {
            duplicateActive.Status = "revoked";
            duplicateActive.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var entity = new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            PersonId = request.PersonId,
            CertificationDefinitionId = definition.Id,
            SourceType = "trainarr_publication",
            Status = "active",
            GrantedAt = grantedAt,
            ExpiresAt = expiresAt,
            Notes = notes,
            GrantedByUserId = null,
            ExternalPublicationId = request.TrainarrPublicationId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.PersonCertifications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "person_certification.trainarr_grant",
            request.TenantId,
            null,
            "person_certification",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity, definition);
    }

    private async Task<CertificationDefinition> ResolveOrCreateDefinitionAsync(
        Guid tenantId,
        string qualificationKey,
        string qualificationName,
        CancellationToken cancellationToken)
    {
        var definition = await db.CertificationDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.CertificationKey == qualificationKey,
            cancellationToken);
        if (definition is not null)
        {
            return definition;
        }

        var trainarrKey = $"trainarr.{qualificationKey}";
        definition = await db.CertificationDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.CertificationKey == trainarrKey,
            cancellationToken);
        if (definition is not null)
        {
            return definition;
        }

        var category = qualificationKey.StartsWith("readiness.", StringComparison.OrdinalIgnoreCase)
            ? "readiness"
            : "compliance";
        var now = DateTimeOffset.UtcNow;
        definition = new CertificationDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CertificationKey = trainarrKey,
            Name = qualificationName,
            Description = $"TrainArr qualification mirrored as StaffArr certification ({qualificationKey}).",
            Category = category,
            DefaultValidityDays = category == "readiness" ? 365 : 730,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.CertificationDefinitions.Add(definition);
        await db.SaveChangesAsync(cancellationToken);
        return definition;
    }

    private static CertificationGrantIngestionResponse MapResponse(
        PersonCertification entity,
        CertificationDefinition definition) =>
        new(
            entity.Id,
            definition.Id,
            entity.ExternalPublicationId!.Value,
            entity.SourceType,
            entity.Status);

    private async Task EnsurePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        var exists = await db.People.AnyAsync(
            x => x.TenantId == tenantId && x.Id == personId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("people.not_found", "Person was not found.", 404);
        }
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length < 3 || normalized.Length > 128)
        {
            throw new StlApiException(
                "certification_grants.validation",
                "Qualification key must be between 3 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeQualificationName(string qualificationName)
    {
        var trimmed = qualificationName.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "certification_grants.validation",
                "Qualification name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeTrainingDefinitionName(string trainingDefinitionName)
    {
        var trimmed = trainingDefinitionName.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "certification_grants.validation",
                "Training definition name must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeNotes(string? notes, string trainingDefinitionName)
    {
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var trimmed = notes.Trim();
            if (trimmed.Length > 1024)
            {
                throw new StlApiException(
                    "certification_grants.validation",
                    "Notes must be 1024 characters or less.",
                    400);
            }

            return trimmed;
        }

        return $"Granted via TrainArr assignment completion ({trainingDefinitionName}).";
    }
}
