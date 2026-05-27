using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class CertificationService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "readiness",
        "safety",
        "compliance",
        "operational"
    };

    private static readonly HashSet<string> AllowedPersonCertificationStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "expired",
        "revoked"
    };

    public async Task<IReadOnlyList<CertificationDefinitionResponse>> ListDefinitionsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        await StaffArrReadinessCertificationSeed.EnsureBaselineDefinitionsAsync(db, tenantId, cancellationToken);

        return await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .Select(x => MapDefinition(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CertificationDefinitionResponse> UpsertDefinitionAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertCertificationDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = NormalizeCertificationKey(request.CertificationKey);
        var normalizedName = NormalizeName(request.Name, "Certification name");
        var normalizedDescription = NormalizeDescription(request.Description);
        var normalizedCategory = NormalizeCategory(request.Category);
        ValidateDefaultValidityDays(request.DefaultValidityDays);

        var entity = await db.CertificationDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.CertificationKey == normalizedKey,
            cancellationToken);

        if (entity is null)
        {
            entity = new CertificationDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CertificationKey = normalizedKey,
                Name = normalizedName,
                Description = normalizedDescription,
                Category = normalizedCategory,
                DefaultValidityDays = request.DefaultValidityDays,
                Status = "active",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            db.CertificationDefinitions.Add(entity);
        }
        else
        {
            entity.Name = normalizedName;
            entity.Description = normalizedDescription;
            entity.Category = normalizedCategory;
            entity.DefaultValidityDays = request.DefaultValidityDays;
            entity.Status = "active";
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "certification_definition.upsert",
            tenantId,
            actorUserId,
            "certification_definition",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapDefinition(entity);
    }

    public async Task<IReadOnlyList<PersonCertificationResponse>> ListPersonCertificationsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var records = await db.PersonCertifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == personId)
            .OrderByDescending(x => x.GrantedAt)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return [];
        }

        var definitionIds = records.Select(x => x.CertificationDefinitionId).Distinct().ToArray();
        var definitions = await db.CertificationDefinitions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && definitionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return records
            .Select(record =>
            {
                definitions.TryGetValue(record.CertificationDefinitionId, out var definition);
                return MapPersonCertification(record, definition);
            })
            .ToList();
    }

    public async Task<PersonCertificationResponse> GrantManualCertificationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        GrantPersonCertificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var definition = await db.CertificationDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.CertificationDefinitionId,
            cancellationToken);
        if (definition is null)
        {
            throw new StlApiException(
                "certification_definition.not_found",
                "Certification definition was not found.",
                404);
        }

        if (!string.Equals(definition.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "certification_definition.inactive",
                "Certification definition must be active to grant a record.",
                409);
        }

        var grantedAt = request.GrantedAt ?? DateTimeOffset.UtcNow;
        var expiresAt = request.ExpiresAt;
        if (expiresAt is null && definition.DefaultValidityDays is int validityDays)
        {
            expiresAt = grantedAt.AddDays(validityDays);
        }

        if (expiresAt is not null && expiresAt <= grantedAt)
        {
            throw new StlApiException(
                "person_certification.validation",
                "Expiration must be after the grant date.",
                400);
        }

        var duplicateActive = await db.PersonCertifications.AnyAsync(
            x => x.TenantId == tenantId
                && x.PersonId == personId
                && x.CertificationDefinitionId == definition.Id
                && x.Status == "active",
            cancellationToken);
        if (duplicateActive)
        {
            throw new StlApiException(
                "person_certification.duplicate",
                "An active certification record already exists for this definition.",
                409);
        }

        var entity = new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PersonId = personId,
            CertificationDefinitionId = definition.Id,
            SourceType = "manual",
            Status = "active",
            GrantedAt = grantedAt,
            ExpiresAt = expiresAt,
            Notes = NormalizeNotes(request.Notes),
            GrantedByUserId = actorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.PersonCertifications.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "person_certification.grant",
            tenantId,
            actorUserId,
            "person_certification",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapPersonCertification(entity, definition);
    }

    public async Task<PersonCertificationResponse> UpdatePersonCertificationAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid personId,
        Guid personCertificationId,
        UpdatePersonCertificationRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(tenantId, personId, cancellationToken);

        var entity = await db.PersonCertifications.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PersonId == personId && x.Id == personCertificationId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("person_certification.not_found", "Person certification was not found.", 404);
        }

        var normalizedStatus = NormalizePersonCertificationStatus(request.Status);
        if (request.ExpiresAt is not null && request.ExpiresAt <= entity.GrantedAt)
        {
            throw new StlApiException(
                "person_certification.validation",
                "Expiration must be after the grant date.",
                400);
        }

        entity.Status = normalizedStatus;
        if (request.ExpiresAt is not null)
        {
            entity.ExpiresAt = request.ExpiresAt;
        }

        entity.Notes = NormalizeNotes(request.Notes) ?? entity.Notes;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "person_certification.update",
            tenantId,
            actorUserId,
            "person_certification",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        var definition = await db.CertificationDefinitions
            .AsNoTracking()
            .FirstAsync(x => x.TenantId == tenantId && x.Id == entity.CertificationDefinitionId, cancellationToken);

        return MapPersonCertification(entity, definition);
    }

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

    private static CertificationDefinitionResponse MapDefinition(CertificationDefinition entity) =>
        new(
            entity.Id,
            entity.CertificationKey,
            entity.Name,
            entity.Description,
            entity.Category,
            entity.DefaultValidityDays,
            entity.Status,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static PersonCertificationResponse MapPersonCertification(
        PersonCertification entity,
        CertificationDefinition? definition) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.CertificationDefinitionId,
            definition?.CertificationKey ?? entity.CertificationDefinitionId.ToString(),
            definition?.Name ?? "Unknown certification",
            definition?.Category ?? "unknown",
            entity.SourceType,
            entity.Status,
            PersonCertificationEffectiveStatus.Resolve(entity),
            entity.GrantedAt,
            entity.ExpiresAt,
            entity.Notes,
            entity.GrantedByUserId,
            entity.ExternalPublicationId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeCertificationKey(string certificationKey)
    {
        if (string.IsNullOrWhiteSpace(certificationKey))
        {
            throw new StlApiException("certification_definition.validation", "Certification key is required.", 400);
        }

        var normalized = certificationKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 128)
        {
            throw new StlApiException(
                "certification_definition.validation",
                "Certification key length is invalid.",
                400);
        }

        if (normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '.' || ch == '_')))
        {
            throw new StlApiException(
                "certification_definition.validation",
                "Certification key contains invalid characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "readiness";
        }

        var normalized = category.Trim().ToLowerInvariant();
        if (!AllowedCategories.Contains(normalized))
        {
            throw new StlApiException(
                "certification_definition.validation",
                "Category must be readiness, safety, compliance, or operational.",
                400);
        }

        return normalized;
    }

    private static void ValidateDefaultValidityDays(int? defaultValidityDays)
    {
        if (defaultValidityDays is null)
        {
            return;
        }

        if (defaultValidityDays <= 0 || defaultValidityDays > 3650)
        {
            throw new StlApiException(
                "certification_definition.validation",
                "Default validity days must be between 1 and 3650.",
                400);
        }
    }

    private static string NormalizePersonCertificationStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new StlApiException("person_certification.validation", "Status is required.", 400);
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedPersonCertificationStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "person_certification.validation",
                "Status must be active, expired, or revoked.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException("validation", $"{fieldName} is required.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > 128)
        {
            throw new StlApiException("validation", $"{fieldName} must be 128 characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 512)
        {
            throw new StlApiException("validation", "Description must be 512 characters or less.", 400);
        }

        return normalized;
    }

    private static string? NormalizeNotes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 1024)
        {
            throw new StlApiException("validation", "Notes must be 1024 characters or less.", 400);
        }

        return normalized;
    }
}
