namespace ComplianceCore.Api.Contracts;

public sealed record VocabularyTypeResponse(
    string TypeKey,
    string Label,
    string Description,
    int SortOrder,
    bool IsActive);

public sealed record CoreVocabularyKeyResponse(
    string Key,
    string Label,
    string Description,
    int SortOrder,
    bool IsRequired);

public sealed record CoreVocabularyKeyRegistryResponse(
    IReadOnlyList<CoreVocabularyKeyResponse> Keys);

public sealed record ValidateCoreVocabularyKeysRequest(
    IReadOnlyList<string> Keys);

public sealed record ValidateCoreVocabularyKeysResponse(
    IReadOnlyList<ValidateCoreVocabularyKeyResult> Items);

public sealed record ValidateCoreVocabularyKeyResult(
    string Key,
    bool IsKnown,
    string? ReasonCode);

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

public sealed record UpdateVocabularyTermRequest(
    string? Label = null,
    string? Description = null,
    bool? IsActive = null);

public sealed record ValidateVocabularyKeysRequest(
    IReadOnlyList<ValidateVocabularyKeyItem> Items);

public sealed record ValidateVocabularyKeyItem(
    string Family,
    string Key);

public sealed record ValidateVocabularyKeysResponse(
    IReadOnlyList<ValidateVocabularyKeyResult> Items);

public sealed record ValidateVocabularyKeyResult(
    string Family,
    string Key,
    bool IsValid,
    string? ReasonCode,
    Guid? TermId);

public sealed record VocabularyTermUsageResponse(
    string Family,
    string Key,
    int AliasCount,
    int FactRequirementCount,
    int RulePackCount,
    int CitationCount);

public sealed record VocabularyTermHistoryItemResponse(
    Guid AuditEventId,
    Guid TermId,
    string Family,
    string Key,
    string Action,
    string Result,
    Guid? ActorUserId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record CreateVocabularyAliasRequest(
    Guid VocabularyTermId,
    string AliasText);

public sealed record VocabularyAliasResponse(
    Guid AliasId,
    Guid VocabularyTermId,
    string AliasText,
    bool IsActive,
    DateTimeOffset CreatedAt);
