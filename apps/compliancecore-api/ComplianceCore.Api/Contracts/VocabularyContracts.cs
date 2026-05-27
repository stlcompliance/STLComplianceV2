namespace ComplianceCore.Api.Contracts;

public sealed record VocabularyTypeResponse(
    string TypeKey,
    string Label,
    string Description,
    int SortOrder,
    bool IsActive);

public sealed record VocabularyTermResponse(
    Guid TermId,
    string TermKey,
    string Label,
    string VocabularyTypeKey,
    string Description,
    bool IsActive,
    IReadOnlyList<string> Aliases,
    DateTimeOffset CreatedAt);

public sealed record CreateVocabularyTermRequest(
    string TermKey,
    string Label,
    string VocabularyTypeKey,
    string Description);

public sealed record CreateVocabularyAliasRequest(
    Guid VocabularyTermId,
    string AliasText);

public sealed record VocabularyAliasResponse(
    Guid AliasId,
    Guid VocabularyTermId,
    string AliasText,
    bool IsActive,
    DateTimeOffset CreatedAt);
