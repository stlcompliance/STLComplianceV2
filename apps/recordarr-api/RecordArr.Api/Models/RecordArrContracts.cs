namespace RecordArr.Api.Models;

public sealed record RecordArrSessionResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasRecordArrEntitlement,
    IReadOnlyCollection<string> Entitlements);

public sealed record RecordArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    int RecordCount,
    int ActiveCount,
    int ReviewCount,
    int UploadSessionCount,
    int PackageCount,
    int ControlledDocumentCount,
    int LegalHoldCount,
    IReadOnlyList<RecordArrRecordResponse> RecentRecords,
    IReadOnlyList<RecordArrPackageResponse> OpenPackages,
    IReadOnlyList<RecordArrControlledDocumentResponse> ControlledDocuments,
    IReadOnlyList<RecordArrLegalHoldResponse> LegalHolds);

public sealed record RecordArrRecordResponse(
    string RecordId,
    string RecordNumber,
    string Title,
    string Description,
    string RecordType,
    string DocumentType,
    string Status,
    string Classification,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string SourceObjectDisplayName,
    string OwnerPersonId,
    string? UploadedByPersonId,
    DateTimeOffset UploadedAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string CurrentFileName,
    string CurrentMimeType,
    int VersionNumber,
    IReadOnlyList<string> Tags);

public sealed record RecordArrUploadSessionResponse(
    string UploadSessionId,
    string UploadSessionNumber,
    string SessionType,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string UploadPurpose,
    string Status,
    bool RequiresDocumentScan,
    bool RequiresOcr,
    bool RequiresManualReview,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? RevokedAt,
    IReadOnlyList<string> AllowedMimeTypes,
    int MaxUploads,
    long MaxFileSizeBytes,
    IReadOnlyList<string> UploadedRecordRefs);

public sealed record RecordArrScanProcessingResponse(
    string ScanProcessingId,
    string RecordId,
    string OriginalFileName,
    string Status,
    string ScanPurpose,
    string? EdgeCoordinates,
    string? GeneratedPdfRecordRef,
    string? OcrResultId,
    string? ExtractionResultId,
    decimal ConfidenceScore,
    DateTimeOffset? ProcessedAt,
    string? FailureReason);

public sealed record RecordArrOcrResultResponse(
    string OcrResultId,
    string RecordId,
    string FileId,
    string Engine,
    string Status,
    string Language,
    decimal ConfidenceScore,
    string FullText,
    DateTimeOffset ExtractedAt,
    string? FailureReason);

public sealed record RecordArrExtractedFieldResponse(
    string ExtractedFieldId,
    string ExtractionResultId,
    string FieldKey,
    string Label,
    string Value,
    string ValueType,
    decimal ConfidenceScore,
    string ReviewStatus,
    string? CorrectedValue,
    string? CorrectedByPersonId,
    DateTimeOffset? CorrectedAt);

public sealed record RecordArrExtractionResultResponse(
    string ExtractionResultId,
    string RecordId,
    string ExtractionType,
    string Status,
    IReadOnlyList<RecordArrExtractedFieldResponse> ExtractedFields,
    decimal ConfidenceScore,
    DateTimeOffset ExtractedAt,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt,
    string? FailureReason);

public sealed record RecordArrEvidenceMappingResponse(
    string EvidenceMappingId,
    string RecordId,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    string ComplianceRequirementRef,
    string EvidenceTypeKey,
    string Status,
    string MappingSource,
    decimal ConfidenceScore,
    string? ConfirmedByPersonId,
    DateTimeOffset? ConfirmedAt,
    string? RejectedByPersonId,
    DateTimeOffset? RejectedAt,
    string? RejectionReason,
    string? Notes);

public sealed record RecordArrPackageResponse(
    string PackageId,
    string PackageNumber,
    string Title,
    string PackageType,
    string Status,
    string SourceProduct,
    IReadOnlyList<string> SourceObjectRefs,
    IReadOnlyList<string> RecordRefs,
    string? ManifestChecksum,
    string? GeneratedPdfRecordRef,
    string? GeneratedZipFileRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? LockedAt,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset? ExpiresAt);

public sealed record RecordArrPackageManifestResponse(
    string ManifestId,
    string PackageId,
    int ManifestVersion,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<RecordArrPackageManifestEntryResponse> RecordEntries,
    IReadOnlyList<RecordArrPackageManifestEntryResponse> SourceObjectEntries,
    IReadOnlyList<RecordArrPackageManifestEntryResponse> RequirementEntries,
    string Checksum,
    string GeneratedByPersonId);

public sealed record RecordArrPackageManifestEntryResponse(
    string EntryId,
    string EntryType,
    string DisplayName,
    string? SourceProduct,
    string? SourceObjectRef,
    string? RecordRef,
    string? ComplianceRequirementRef,
    string? StatusSnapshot,
    string Checksum);

public sealed record RecordArrRetentionPolicyResponse(
    string RetentionPolicyId,
    string PolicyKey,
    string Title,
    string Description,
    string RecordTypeApplicability,
    string DocumentTypeApplicability,
    string SourceProductApplicability,
    int RetainFor,
    string RetentionUnit,
    string RetentionStartTrigger,
    string DisposalAction,
    bool LegalHoldOverrides,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RecordArrRetentionStatusResponse(
    string RetentionStatusId,
    string RecordId,
    string RetentionPolicyRef,
    string Status,
    DateTimeOffset RetentionStartAt,
    DateTimeOffset? RetentionExpiresAt,
    DateTimeOffset? NextReviewAt,
    DateTimeOffset? LastReviewedAt,
    string? ReviewedByPersonId,
    string? DisposalReviewRef);

public sealed record RecordArrDisposalReviewResponse(
    string DisposalReviewId,
    string RecordId,
    string RetentionStatusRef,
    string ProposedAction,
    string Status,
    DateTimeOffset RequestedAt,
    string RequestedByPersonId,
    string? ReviewedByPersonId,
    DateTimeOffset? ReviewedAt,
    string? DecisionReason,
    DateTimeOffset? CompletedAt);

public sealed record RecordArrLegalHoldResponse(
    string LegalHoldId,
    string HoldNumber,
    string Title,
    string Description,
    string Status,
    string HoldType,
    IReadOnlyList<string> ScopeRules,
    IReadOnlyList<string> RecordRefs,
    string SourceProduct,
    string SourceObjectType,
    string SourceObjectId,
    DateTimeOffset CreatedAt,
    string CreatedByPersonId,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? ReleasedAt,
    string? ReleasedByPersonId,
    string? ReleaseReason);

public sealed record RecordArrControlledDocumentResponse(
    string ControlledDocumentId,
    string DocumentNumber,
    string RecordId,
    string Title,
    string Description,
    string ControlledDocumentType,
    string Status,
    string OwnerPersonId,
    string DepartmentOrgUnitId,
    string StaffarrSiteId,
    string CurrentVersionId,
    int ReviewIntervalDays,
    DateTimeOffset? NextReviewAt,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? ExpiresAt,
    string? SupersedesDocumentRef,
    string? SupersededByDocumentRef,
    bool AcknowledgementRequired,
    IReadOnlyList<string> RelatedRecordRefs,
    IReadOnlyList<RecordArrAuditTrailEntryResponse> AuditTrail);

public sealed record RecordArrAuditTrailEntryResponse(
    string AuditTrailEntryId,
    string Action,
    string ActorPersonId,
    DateTimeOffset OccurredAt,
    string Details);

public sealed record RecordArrControlledDocumentVersionResponse(
    string VersionId,
    string ControlledDocumentId,
    int VersionNumber,
    string VersionLabel,
    string Status,
    string FileName,
    DateTimeOffset CreatedAt,
    string CreatedByPersonId,
    DateTimeOffset? SubmittedForReviewAt,
    DateTimeOffset? ApprovedAt,
    string? ApprovedByPersonId,
    DateTimeOffset? EffectiveAt,
    DateTimeOffset? SupersededAt,
    string? ChangeSummary,
    string? PreviousVersionRef,
    string? NextVersionRef);

public sealed record RecordArrDocumentReviewResponse(
    string DocumentReviewId,
    string ControlledDocumentId,
    string VersionId,
    string ReviewType,
    string Status,
    string RequestedByPersonId,
    string ReviewerPersonId,
    DateTimeOffset RequestedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset? ReviewedAt,
    string? DecisionReason,
    string? Comments);

public sealed record RecordArrDocumentDistributionResponse(
    string DistributionId,
    string ControlledDocumentId,
    string VersionId,
    string DistributionType,
    string TargetRef,
    string Status,
    DateTimeOffset? DistributedAt,
    DateTimeOffset? AcknowledgedAt,
    string? AcknowledgementRef);

public sealed record RecordArrDocumentAcknowledgementResponse(
    string AcknowledgementId,
    string ControlledDocumentId,
    string VersionId,
    string PersonId,
    string Status,
    DateTimeOffset? AcknowledgedAt,
    string? SignatureRecordRef,
    string? AttestationText,
    DateTimeOffset? DueAt);

public sealed record RecordArrAccessPolicyResponse(
    string AccessPolicyId,
    string RecordId,
    string PolicyType,
    string Status,
    IReadOnlyList<string> ReadRules,
    IReadOnlyList<string> WriteRules,
    IReadOnlyList<string> DownloadRules,
    IReadOnlyList<string> ShareRules,
    IReadOnlyList<string> ExportRules,
    IReadOnlyList<string> PurgeRules);

public sealed record RecordArrAccessGrantResponse(
    string AccessGrantId,
    string RecordId,
    string GranteeType,
    string GranteeRef,
    string Permission,
    string Status,
    string GrantedByPersonId,
    DateTimeOffset GrantedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? RevokeReason);

public sealed record RecordArrExternalShareResponse(
    string ExternalShareId,
    string ShareNumber,
    string RecordId,
    string SharePurpose,
    string Status,
    string RecipientName,
    string RecipientEmail,
    IReadOnlyList<string> AllowedActions,
    DateTimeOffset CreatedAt,
    string CreatedByPersonId,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? RevokedAt,
    string? RevokedByPersonId,
    DateTimeOffset? LastAccessedAt,
    int AccessCount);

public sealed record RecordArrRedactionResponse(
    string RedactionId,
    string SourceRecordId,
    string RedactedRecordId,
    string RedactionReason,
    string Status,
    string RedactedByPersonId,
    DateTimeOffset RedactedAt,
    IReadOnlyList<string> RedactionRules);

public sealed record RecordArrAccessLogResponse(
    string AccessLogId,
    string RecordId,
    string Action,
    string Result,
    string? ActorPersonId,
    string? ActorServiceClientId,
    string? ExternalShareId,
    DateTimeOffset OccurredAt,
    string? SourceIp,
    string? UserAgent,
    string? ReasonCode);
