namespace ComplianceCore.Api.Contracts;

public sealed record RegulatoryCitationResponse(
    Guid CitationId,
    Guid RegulatoryProgramId,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    Guid? RulePackId,
    string? RulePackKey,
    string? RulePackLabel,
    string CitationKey,
    string Label,
    string SourceReference,
    string Description,
    int VersionNumber,
    Guid? SupersedesCitationId,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateRegulatoryCitationRequest(
    Guid RegulatoryProgramId,
    Guid? RulePackId,
    string CitationKey,
    string Label,
    string SourceReference,
    string Description,
    Guid? SupersedesCitationId);

public sealed record UpdateRegulatoryCitationRequest(
    string Label,
    string SourceReference,
    string Description,
    bool IsActive);

public sealed record CitationRuleLinkResponse(
    Guid RulePackId,
    string RulePackKey,
    string RulePackLabel,
    string Source);

public sealed record FactDefinitionResponse(
    Guid FactDefinitionId,
    string FactKey,
    string Label,
    string Description,
    string ValueType,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFactDefinitionRequest(
    string FactKey,
    string Label,
    string Description,
    string ValueType);

public sealed record UpdateFactDefinitionRequest(
    string Label,
    string Description,
    string ValueType,
    bool IsActive);

public sealed record FactDefinitionUsageResponse(
    int FactRequirementCount,
    int RulePackCount,
    int CitationCount);

public sealed record FactDefinitionHistoryItemResponse(
    Guid AuditEventId,
    Guid FactDefinitionId,
    string FactKey,
    string Action,
    string Result,
    Guid? ActorUserId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record ValidateFactPayloadItemRequest(
    string FactKey,
    string? Value);

public sealed record ValidateFactPayloadRequest(
    IReadOnlyList<ValidateFactPayloadItemRequest> Facts);

public sealed record ValidateFactPayloadItemResponse(
    string FactKey,
    bool IsValid,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record ValidateFactPayloadResponse(
    IReadOnlyList<ValidateFactPayloadItemResponse> Results);

public sealed record FactRequirementResponse(
    Guid FactRequirementId,
    Guid FactDefinitionId,
    string FactKey,
    string FactLabel,
    Guid? RulePackId,
    string? RulePackKey,
    Guid? CitationId,
    string? CitationKey,
    string RequirementKey,
    string ApplicabilityKey,
    string SourceProduct,
    string SourceEntity,
    string SourceFieldOrRecordType,
    string ValueType,
    string Operator,
    string ExpectedValue,
    string EvidenceKind,
    string RequiredDocumentType,
    string RetentionPeriod,
    string AuditQuestion,
    string FailureSeverity,
    bool AutomaticFailureFlag,
    bool OverrideAllowed,
    string OverridePermission,
    bool RemediationRequired,
    bool ExternallyAssertable,
    string Label,
    string Description,
    bool IsRequired,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateFactRequirementRequest(
    Guid FactDefinitionId,
    Guid? RulePackId,
    Guid? CitationId,
    string RequirementKey,
    string Label,
    string Description,
    bool IsRequired,
    string? ApplicabilityKey = null,
    string? SourceProduct = null,
    string? SourceEntity = null,
    string? SourceFieldOrRecordType = null,
    string? ValueType = null,
    string? Operator = null,
    string? ExpectedValue = null,
    string? EvidenceKind = null,
    string? RequiredDocumentType = null,
    string? RetentionPeriod = null,
    string? AuditQuestion = null,
    string? FailureSeverity = null,
    bool? AutomaticFailureFlag = null,
    bool? OverrideAllowed = null,
    string? OverridePermission = null,
    bool? RemediationRequired = null,
    bool ExternallyAssertable = false);
