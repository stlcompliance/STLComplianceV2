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

    public Task<VocabularyTermResponse> CreateTermForFamilyAsync(
        Guid tenantId,
        Guid? actorUserId,
        string family,
        CreateVocabularyTermRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedFamily = NormalizeVocabularyTypeKey(family);
        var normalizedRequestFamily = NormalizeVocabularyTypeKey(request.VocabularyTypeKey);
        if (!string.Equals(normalizedFamily, normalizedRequestFamily, StringComparison.Ordinal))
        {
            throw new StlApiException(
                "vocabulary.family_mismatch",
                "Vocabulary family route must match the request vocabulary type key.",
                400);
        }

        return CreateTermAsync(tenantId, actorUserId, request, cancellationToken);
    }

    public async Task<VocabularyTermResponse> UpdateTermForFamilyAsync(
        Guid tenantId,
        Guid? actorUserId,
        string family,
        string termKey,
        UpdateVocabularyTermRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedFamily = NormalizeVocabularyTypeKey(family);
        var normalizedTermKey = NormalizeTermKey(termKey);

        var term = await db.VocabularyTerms.FirstOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.VocabularyTypeKey == normalizedFamily
                && x.TermKey == normalizedTermKey,
            cancellationToken);
        if (term is null)
        {
            throw new StlApiException("vocabulary.term_not_found", "Vocabulary term was not found.", 404);
        }

        if (request.Label is not null)
        {
            term.Label = NormalizeLabel(request.Label);
        }

        if (request.Description is not null)
        {
            term.Description = NormalizeDescription(request.Description);
        }

        if (request.IsActive is not null)
        {
            term.IsActive = request.IsActive.Value;
        }

        term.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "vocabulary.term.update",
            tenantId,
            actorUserId,
            "vocabulary_term",
            term.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        var aliases = await LoadActiveAliasesAsync(tenantId, term.Id, cancellationToken);
        return MapTermResponse(term, aliases);
    }

    public async Task<ValidateVocabularyKeysResponse> ValidateKeysAsync(
        Guid tenantId,
        ValidateVocabularyKeysRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new StlApiException(
                "vocabulary.validation",
                "At least one vocabulary key must be supplied.",
                400);
        }

        var results = new List<ValidateVocabularyKeyResult>();
        foreach (var item in request.Items)
        {
            var family = item.Family.Trim().ToLowerInvariant();
            var key = item.Key.Trim().ToLowerInvariant();

            if (!VocabularyTypeCatalog.TypeKeys.Contains(family))
            {
                results.Add(new ValidateVocabularyKeyResult(family, key, false, "unknown_family", null));
                continue;
            }

            if (key.Length == 0)
            {
                results.Add(new ValidateVocabularyKeyResult(family, key, false, "missing_key", null));
                continue;
            }

            var term = await db.VocabularyTerms
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId
                        && x.VocabularyTypeKey == family
                        && x.TermKey == key,
                    cancellationToken);

            results.Add(term is null || !term.IsActive
                ? new ValidateVocabularyKeyResult(family, key, false, term is null ? "unknown_key" : "inactive_key", term?.Id)
                : new ValidateVocabularyKeyResult(family, key, true, null, term.Id));
        }

        return new ValidateVocabularyKeysResponse(results);
    }

    public async Task<VocabularyTermUsageResponse> GetUsageForFamilyAsync(
        Guid tenantId,
        string family,
        string termKey,
        CancellationToken cancellationToken = default)
    {
        var term = await LoadTermForFamilyAsync(tenantId, family, termKey, asNoTracking: true, cancellationToken);

        var aliasCount = await db.VocabularyAliases
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId
                    && x.VocabularyTermId == term.Id
                    && x.IsActive,
                cancellationToken);

        var requirements = await db.FactRequirements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .ToListAsync(cancellationToken);

        var matchingRequirements = requirements
            .Where(x => FactRequirementContractRules
                .SplitCsv(x.ExpectedValue)
                .Any(value => string.Equals(value, term.TermKey, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new VocabularyTermUsageResponse(
            term.VocabularyTypeKey,
            term.TermKey,
            aliasCount,
            matchingRequirements.Count,
            matchingRequirements.Where(x => x.RulePackId.HasValue).Select(x => x.RulePackId!.Value).Distinct().Count(),
            matchingRequirements.Where(x => x.CitationId.HasValue).Select(x => x.CitationId!.Value).Distinct().Count());
    }

    public async Task<IReadOnlyList<VocabularyTermHistoryItemResponse>> ListHistoryForFamilyAsync(
        Guid tenantId,
        string family,
        string termKey,
        CancellationToken cancellationToken = default)
    {
        var term = await LoadTermForFamilyAsync(tenantId, family, termKey, asNoTracking: true, cancellationToken);

        return await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.TargetType == "vocabulary_term"
                && x.TargetId == term.Id.ToString())
            .OrderByDescending(x => x.OccurredAt)
            .Select(x => new VocabularyTermHistoryItemResponse(
                x.Id,
                term.Id,
                term.VocabularyTypeKey,
                term.TermKey,
                x.Action,
                x.Result,
                x.ActorUserId,
                x.CorrelationId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);
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

    private async Task<VocabularyTerm> LoadTermForFamilyAsync(
        Guid tenantId,
        string family,
        string termKey,
        bool asNoTracking,
        CancellationToken cancellationToken = default)
    {
        var normalizedFamily = NormalizeVocabularyTypeKey(family);
        var normalizedTermKey = NormalizeTermKey(termKey);
        var query = db.VocabularyTerms.Where(
            x => x.TenantId == tenantId
                && x.VocabularyTypeKey == normalizedFamily
                && x.TermKey == normalizedTermKey);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken)
            ?? throw new StlApiException("vocabulary.term_not_found", "Vocabulary term was not found.", 404);
    }

    private async Task<IReadOnlyList<string>> LoadActiveAliasesAsync(
        Guid tenantId,
        Guid termId,
        CancellationToken cancellationToken = default) =>
        await db.VocabularyAliases
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VocabularyTermId == termId && x.IsActive)
            .OrderBy(x => x.AliasText)
            .Select(x => x.AliasText)
            .ToListAsync(cancellationToken);

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
