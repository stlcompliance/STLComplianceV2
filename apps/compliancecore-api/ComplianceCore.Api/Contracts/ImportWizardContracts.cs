namespace ComplianceCore.Api.Contracts;

public sealed record CreateImportSessionRequest(
    string? ImportType = null,
    string? SourceFilename = null,
    string? Notes = null);

public sealed record ImportSessionResponse(
    Guid ImportSessionId,
    Guid TenantId,
    Guid? UploadedByPersonId,
    string SourceFilename,
    string SourceHash,
    string ImportType,
    string Status,
    string ValidationStatus,
    string MappingStatus,
    string CommitStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ValidatedAt,
    DateTimeOffset? MappedAt,
    DateTimeOffset? CommittedAt,
    DateTimeOffset? RejectedAt,
    string Notes);

public sealed record ImportSessionSourceFileResponse(
    Guid SourceFileId,
    string SourceFile,
    string OriginalFilename,
    string FileHash,
    long ByteLength,
    string ValidationStatus,
    IReadOnlyList<string> ValidationErrors);

public sealed record ImportUploadResponse(
    ImportSessionResponse Session,
    IReadOnlyList<ImportSessionSourceFileResponse> Files);

public sealed record ImportParseResponse(
    ImportSessionResponse Session,
    IReadOnlyList<ImportStagedFileSummaryResponse> Files);

public sealed record ImportStagedFileSummaryResponse(
    string SourceFile,
    int RowCount,
    string ValidationStatus,
    IReadOnlyList<string> ValidationErrors);

public sealed record ImportValidationResultsResponse(
    Guid ImportSessionId,
    string ValidationStatus,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<ImportSessionSourceFileResponse> Files,
    IReadOnlyList<ImportStagedRowResultResponse> Rows);

public sealed record ImportStagedRowResultResponse(
    Guid StagedRowId,
    string SourceFile,
    int RowNumber,
    string CanonicalKeyCandidate,
    string ValidationStatus,
    IReadOnlyList<string> ValidationErrors);

public sealed record MappingCandidateResponse(
    Guid MappingCandidateId,
    Guid StagedRowId,
    string StagedSourceFile,
    int StagedRowNumber,
    string SourceKey,
    string SourceLabel,
    Guid? EvidenceOptionId,
    string EvidenceOptionKey,
    string EvidenceOptionLabel,
    string OptionLogicGroup,
    string TargetKind,
    string TargetId,
    string TargetKey,
    string TargetLabel,
    decimal ConfidenceScore,
    string ConfidenceBand,
    IReadOnlyList<string> MatchReasons,
    IReadOnlyList<string> RiskFlags,
    string ProposedAction,
    bool SatisfiesRequirementIfConfirmed,
    bool RequiresAdditionalSupportingEvidence,
    bool RequiresConfirmation);

public sealed record EvidenceOptionProposalResponse(
    Guid EvidenceOptionId,
    string EvidenceOptionKey,
    string EvidenceOptionLabel,
    string LogicType,
    string EvidenceKind,
    string TargetKind,
    string SourceProduct,
    string SourceEntity,
    string SourceFieldOrRecordType,
    string DocumentTypeKey,
    string MaterialKey,
    string PartKey,
    string SystemKey,
    string AssetKind,
    string ExternalRegistryKey,
    string FactKey,
    bool Required,
    int Priority,
    decimal? ConfidenceHint);

public sealed record WizardSummaryResponse(
    Guid ImportSessionId,
    string SessionStatus,
    string MappingStatus,
    int TotalItems,
    int PendingItems,
    int ConfirmedItems,
    int ChangedItems,
    int SkippedItems,
    int RejectedItems,
    int BlockedItems,
    int ExactNoRiskItems,
    int HighNoRiskItems,
    int RiskFlaggedItems);

public sealed record WizardItemResponse(
    Guid ItemId,
    Guid StagedRowId,
    string Status,
    string RequirementKey,
    string EvidenceKey,
    string Label,
    string AuditQuestion,
    string CitationKey,
    string RulePackKey,
    string ComplianceKeyOrDomain,
    string RequiredEvidenceKind,
    string EvidenceLogic,
    EvidenceOptionProposalResponse SuggestedEvidencePath,
    IReadOnlyList<EvidenceOptionProposalResponse> OtherAcceptableEvidencePaths,
    string SourceProduct,
    string SourceEntity,
    string SourceFieldOrRecordType,
    string SuggestedTarget,
    string TargetKind,
    decimal ConfidenceScore,
    string ConfidenceBand,
    IReadOnlyList<string> MatchReasons,
    IReadOnlyList<string> RiskFlags,
    string ConfirmationPrompt,
    string WhatWillHappenIfConfirmed,
    bool OverrideAllowed,
    bool RemediationRequired,
    string ExceptionProofPrompt,
    IReadOnlyDictionary<string, string> SourceRow,
    IReadOnlyDictionary<string, string> TargetRecord);

public sealed record MappingDecisionResponse(
    Guid MappingDecisionId,
    Guid ImportSessionId,
    Guid StagedRowId,
    Guid? MappingCandidateId,
    string Decision,
    Guid? SelectedEvidenceOptionId,
    string SelectedEvidenceOptionKey,
    string SelectedTargetKind,
    string SelectedTargetId,
    string SelectedTargetKey,
    string EvidenceMappingPurpose,
    string ExceptionExemptionKey,
    IReadOnlyList<string> ResidualRequirements,
    bool OverrideUsed,
    string OverrideReason,
    Guid DecidedByPersonId,
    DateTimeOffset DecidedAt);

public sealed record SelectEvidenceOptionRequest(
    string EvidenceOptionKey);

public sealed record SelectTargetRequest(
    string TargetKind,
    string TargetId,
    string TargetKey,
    string TargetLabel);

public sealed record CreateTargetRequest(
    string TargetKind,
    IReadOnlyDictionary<string, string> Payload);

public sealed record SupportingEvidenceRequest(
    string TargetKind,
    string TargetKey,
    string TargetLabel,
    IReadOnlyDictionary<string, string>? Payload = null);

public sealed record ExceptionProofMappingRequest(
    string ExceptionExemptionKey,
    string TargetKind,
    string TargetKey,
    string TargetLabel,
    IReadOnlyDictionary<string, string>? Payload = null,
    IReadOnlyList<string>? ResidualRequirements = null);

public sealed record ForceMapRequest(
    string TargetKind,
    string TargetId,
    string TargetKey,
    string TargetLabel,
    string OverrideReason,
    bool RiskAcknowledged);

public sealed record BulkConfirmMappingsRequest(
    string ConfidenceBand,
    bool SummaryConfirmed = false);

public sealed record CommitPreviewResponse(
    Guid ImportSessionId,
    int TotalDecisions,
    int ExistingDocumentsMapped,
    int NewDocumentsToCreate,
    int ExistingMaterialsMapped,
    int NewMaterialsToCreate,
    int ExistingPartsMapped,
    int NewPartsToCreate,
    int ExistingSystemsOrAssetsMapped,
    int NewSystemsAssetsOrReferencesToCreate,
    int FactDefinitionsToCreateOrUpdate,
    int FactRequirementsToCreateOrUpdate,
    int EvidenceOptionGroupsToCreateOrUpdate,
    int EvidenceOptionsToCreateOrUpdate,
    int EvidenceReferencesToCreateOrUpdate,
    int ExceptionProofMappings,
    int ExceptionExemptionRecordsToCreateOrUpdate,
    int OverridesUsed,
    int SkippedRows,
    int RejectedRows,
    IReadOnlyList<string> UnresolvedBlockers,
    IReadOnlyList<CommitPreviewActionResponse> Actions);

public sealed record CommitPreviewActionResponse(
    string Action,
    string SourceKey,
    string TargetKind,
    string TargetKey,
    string Summary,
    string EvidenceMappingPurpose,
    string ExceptionExemptionKey,
    IReadOnlyList<string> ResidualRequirements,
    bool OverrideUsed);

public sealed record ImportCompletionReportResponse(
    Guid ImportSessionId,
    string Status,
    int CreatedCount,
    int UpdatedCount,
    int SkippedCount,
    int RejectedCount,
    int OverrideCount,
    int EvidenceMappingsCreated,
    int NewDocumentsMaterialsPartsSystemsCreated,
    int ExistingDocumentsMaterialsPartsSystemsMapped,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors,
    string AuditLogReference);
