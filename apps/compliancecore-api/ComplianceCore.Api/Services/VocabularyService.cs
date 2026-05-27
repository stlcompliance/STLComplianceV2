using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class VocabularyService(
    ComplianceCoreDbContext db,
    IComplianceCoreAuditService auditService)
{
    public async Task EnsureVocabularyTypesSeededAsync(CancellationToken cancellationToken = default)
    {
        if (await db.VocabularyTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var type in VocabularyTypeCatalog.SystemTypes)
        {
            db.VocabularyTypes.Add(new VocabularyType
            {
                Id = Guid.NewGuid(),
                TypeKey = type.TypeKey,
                Label = type.Label,
                Description = type.Description,
                SortOrder = type.SortOrder,
                IsActive = true,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VocabularyTypeResponse>> ListTypesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureVocabularyTypesSeededAsync(cancellationToken);

        return await db.VocabularyTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new VocabularyTypeResponse(
                x.TypeKey,
                x.Label,
                x.Description,
                x.SortOrder,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VocabularyTermResponse>> ListTermsAsync(
        Guid tenantId,
        string? vocabularyTypeKey = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureVocabularyTypesSeededAsync(cancellationToken);

        var query = db.VocabularyTerms
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive);

        if (!string.IsNullOrWhiteSpace(vocabularyTypeKey))
        {
            var normalizedTypeKey = vocabularyTypeKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.VocabularyTypeKey == normalizedTypeKey);
        }

        var terms = await query
            .OrderBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var termIds = terms.Select(x => x.Id).ToList();
        var aliases = await db.VocabularyAliases
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && termIds.Contains(x.VocabularyTermId) && x.IsActive)
            .ToListAsync(cancellationToken);

        var aliasesByTerm = aliases
            .GroupBy(x => x.VocabularyTermId)
            .ToDictionary(g => g.Key, g => g.Select(a => a.AliasText).ToList());

        return terms
            .Select(term => MapTermResponse(
                term,
                aliasesByTerm.TryGetValue(term.Id, out var termAliases)
                    ? termAliases
                    : []))
            .ToList();
    }

    public async Task<VocabularyTermResponse> CreateTermAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateVocabularyTermRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureVocabularyTypesSeededAsync(cancellationToken);

        var termKey = NormalizeTermKey(request.TermKey);
        var label = NormalizeLabel(request.Label);
        var vocabularyTypeKey = NormalizeVocabularyTypeKey(request.VocabularyTypeKey);
        var description = NormalizeDescription(request.Description);

        var exists = await db.VocabularyTerms.AnyAsync(
            x => x.TenantId == tenantId && x.TermKey == termKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "vocabulary.duplicate",
                "A vocabulary term with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new VocabularyTerm
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TermKey = termKey,
            Label = label,
            VocabularyTypeKey = vocabularyTypeKey,
            Description = description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.VocabularyTerms.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "vocabulary.term.create",
            tenantId,
            actorUserId,
            "vocabulary_term",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapTermResponse(entity, []);
    }

    public async Task<VocabularyAliasResponse> CreateAliasAsync(
        Guid tenantId,
        Guid? actorUserId,
        CreateVocabularyAliasRequest request,
        CancellationToken cancellationToken = default)
    {
        var aliasText = NormalizeAliasText(request.AliasText);

        var term = await db.VocabularyTerms.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == request.VocabularyTermId && x.IsActive,
            cancellationToken);
        if (term is null)
        {
            throw new StlApiException("vocabulary.term_not_found", "Vocabulary term was not found.", 404);
        }

        var duplicate = await db.VocabularyAliases.AnyAsync(
            x => x.TenantId == tenantId
                && x.VocabularyTermId == request.VocabularyTermId
                && x.AliasText == aliasText
                && x.IsActive,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "vocabulary.alias_duplicate",
                "This alias already exists for the vocabulary term.",
                409);
        }

        var entity = new VocabularyAlias
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VocabularyTermId = request.VocabularyTermId,
            AliasText = aliasText,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.VocabularyAliases.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "vocabulary.alias.create",
            tenantId,
            actorUserId,
            "vocabulary_alias",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return new VocabularyAliasResponse(
            entity.Id,
            entity.VocabularyTermId,
            entity.AliasText,
            entity.IsActive,
            entity.CreatedAt);
    }

    private static VocabularyTermResponse MapTermResponse(VocabularyTerm entity, IReadOnlyList<string> aliases) =>
        new(
            entity.Id,
            entity.TermKey,
            entity.Label,
            entity.VocabularyTypeKey,
            entity.Description,
            entity.IsActive,
            aliases,
            entity.CreatedAt);

    private static string NormalizeTermKey(string termKey)
    {
        var normalized = termKey.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 64)
        {
            throw new StlApiException(
                "vocabulary.validation",
                "Term key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeLabel(string label)
    {
        var trimmed = label.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "vocabulary.validation",
                "Term label must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeVocabularyTypeKey(string vocabularyTypeKey)
    {
        var normalized = vocabularyTypeKey.Trim().ToLowerInvariant();
        if (!VocabularyTypeCatalog.TypeKeys.Contains(normalized))
        {
            throw new StlApiException(
                "vocabulary.validation",
                "Vocabulary type key is not recognized.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        var trimmed = description.Trim();
        if (trimmed.Length < 4 || trimmed.Length > 1024)
        {
            throw new StlApiException(
                "vocabulary.validation",
                "Description must be between 4 and 1024 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeAliasText(string aliasText)
    {
        var trimmed = aliasText.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "vocabulary.validation",
                "Alias text must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }
}
