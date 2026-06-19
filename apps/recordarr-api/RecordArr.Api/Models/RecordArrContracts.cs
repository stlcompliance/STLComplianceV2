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
    string DocumentClass,
    string DocumentType,
    string DocumentSubtype,
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
    IReadOnlyList<string> Tags,
    string CurrentFileRef,
    IReadOnlyList<string> FileRefs,
    string CurrentVersionRef,
    IReadOnlyList<string> SourceObjectRefs,
    IReadOnlyList<string> MetadataRefs,
    IReadOnlyList<string> VersionRefs,
    IReadOnlyList<string> OcrResultRefs,
    IReadOnlyList<string> ExtractionResultRefs,
    IReadOnlyList<string> EvidenceMappingRefs,
    IReadOnlyList<string> PackageRefs,
    string? RetentionPolicyRef,
    string? RetentionStatusRef,
    IReadOnlyList<string> LegalHoldRefs,
    string? AccessPolicyRef,
    IReadOnlyList<string> ComplianceRefs,
    IReadOnlyList<RecordArrAuditTrailEntryResponse> AuditTrail,
    DateTimeOffset? ArchivedAt = null,
    DateTimeOffset? PurgedAt = null)
{
    public RecordArrRecordRefResponse? RecordRef { get; init; }
}

public sealed record RecordArrRecordRefResponse(
    string RecordarrRecordId,
    string RecordNumberSnapshot,
    string TitleSnapshot,
    string RecordTypeSnapshot,
    string DocumentClassSnapshot,
    string DocumentTypeSnapshot,
    string DocumentSubtypeSnapshot,
    string StatusSnapshot,
    string ClassificationSnapshot,
    int VersionSnapshot,
    DateTimeOffset? ExpiresAtSnapshot,
    string? RetentionStatusSnapshot,
    DateTimeOffset LastResolvedAt);

public sealed record RecordArrFileRenditionResponse(
    string RenditionId,
    string FileId,
    string RecordId,
    string RenditionType,
    string StorageKey,
    string MimeType,
    long SizeBytes,
    int? PageCount,
    string Status,
    DateTimeOffset GeneratedAt);

public sealed record RecordArrFileResponse(
    string FileId,
    string TenantId,
    string RecordId,
    string FileNumber,
    string StorageProvider,
    string StorageKey,
    string OriginalFilename,
    string NormalizedFilename,
    string Extension,
    string MimeType,
    long SizeBytes,
    string ChecksumSha256,
    int? PageCount,
    int? ImageWidth,
    int? ImageHeight,
    int? DurationSeconds,
    DateTimeOffset UploadedAt,
    string UploadedByPersonId,
    string VirusScanStatus,
    string ProcessingStatus,
    string EncryptionStatus,
    DateTimeOffset? DeletedAt,
    string? DeleteReason,
    IReadOnlyList<RecordArrFileRenditionResponse> Renditions);

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

public sealed record RecordArrCaptureRequestResponse(
    string CaptureRequestId,
    string TenantId,
    string SourceProduct,
    string SourceObjectRef,
    string CaptureType,
    string Title,
    string Instructions,
    bool Required,
    string Status,
    string? UploadSessionRef,
    string? EvidenceRequirementRef,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record RecordArrEdgeDetectionResultResponse(
    string EdgeDetectionResultId,
    string ScanProcessingId,
    string Status,
    decimal ConfidenceScore,
    int PageIndex,
    string? Corners,
    DateTimeOffset DetectedAt,
    bool RequiresManualCorrection);

public sealed record RecordArrImageEnhancementSettingsResponse(
    string SettingsId,
    string ScanProcessingId,
    bool CropApplied,
    bool PerspectiveCorrectionApplied,
    bool ContrastAdjusted,
    bool BrightnessAdjusted,
    bool GrayscaleApplied,
    bool NoiseReductionApplied,
    bool SharpenApplied,
    bool BackgroundCleaned,
    string OutputFormat);

public sealed record RecordArrScanProcessingResponse(
    string ScanProcessingId,
    string RecordId,
    string OriginalFileName,
    string Status,
    string ScanPurpose,
    string? EdgeCoordinates,
    string? ManualEdgeCoordinates,
    string? CorrectedByPersonId,
    DateTimeOffset? CorrectedAt,
    string? OriginalFileRef,
    string? GeneratedPdfFileRef,
    string? GeneratedPdfRecordRef,
    string? OcrResultId,
    string? ExtractionResultId,
    RecordArrEdgeDetectionResultResponse? EdgeDetectionResult,
    RecordArrImageEnhancementSettingsResponse? EnhancementSettings,
    decimal ConfidenceScore,
    DateTimeOffset? ProcessedAt,
    string? FailureReason);

public sealed record RecordArrOcrPageResultResponse(
    string PageResultId,
    string OcrResultId,
    int PageNumber,
    string Text,
    decimal ConfidenceScore,
    int Width,
    int Height,
    IReadOnlyList<string> Blocks);

public sealed record RecordArrOcrResultResponse(
    string OcrResultId,
    string RecordId,
    string FileId,
    string Engine,
    string Status,
    string Language,
    decimal ConfidenceScore,
    string FullText,
    IReadOnlyList<RecordArrOcrPageResultResponse> PageResults,
    IReadOnlyList<string> BlockResults,
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
    int? PageNumber,
    string? BoundingBox,
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

public sealed record RecordArrEvidenceCoverageResponse(
    string EvidenceCoverageId,
    string TenantId,
    string SourceProduct,
    string SourceObjectRef,
    string ComplianceCoreRequirementRef,
    string Status,
    IReadOnlyList<string> RecordRefs,
    IReadOnlyList<string> MissingEvidenceTypes,
    IReadOnlyList<string> InvalidRecordRefs,
    DateTimeOffset EvaluatedAt,
    string EvaluationRef);

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

public sealed record RecordArrRecordMetadataResponse(
    string MetadataId,
    string RecordId,
    string Key,
    string Value,
    string ValueType,
    string Source,
    decimal ConfidenceScore,
    bool Verified,
    string? VerifiedByPersonId,
    DateTimeOffset? VerifiedAt);

public sealed record RecordArrRecordLinkResponse(
    string RecordLinkId,
    string RecordId,
    string? LinkedRecordId,
    string? SourceObjectRef,
    string LinkType,
    DateTimeOffset CreatedAt,
    string CreatedByPersonId);

public sealed record RecordArrRecordCommentResponse(
    string CommentId,
    string RecordId,
    string Body,
    string Visibility,
    DateTimeOffset CreatedAt,
    string CreatedByPersonId,
    DateTimeOffset? EditedAt,
    string? EditedByPersonId);

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
    string DocumentClass,
    string DocumentType,
    string DocumentSubtype,
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
    string? NextVersionRef,
    string? FileRef);

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

public sealed record RecordArrReminderResponse(
    string ReminderId,
    string ReminderType,
    string Status,
    string Title,
    string Description,
    string? RecordId,
    string? ControlledDocumentId,
    string? VersionId,
    string? PersonId,
    DateTimeOffset? DueAt,
    DateTimeOffset CreatedAt,
    string SourceRef);

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

public sealed record RecordArrSignatureRecordResponse(
    string SignatureRecordId,
    string TenantId,
    string RecordId,
    string SignaturePurpose,
    string? SignerPersonId,
    string? SignerExternalName,
    string? SignerTitle,
    string AttestationText,
    string SignatureFileRef,
    DateTimeOffset SignedAt,
    string CapturedByPersonId,
    string SourceProduct,
    string SourceObjectRef,
    string? GeoCoordinates,
    string? DeviceSnapshot);

public sealed record RecordArrPhotoEvidenceResponse(
    string PhotoEvidenceId,
    string TenantId,
    string RecordId,
    string PhotoPurpose,
    string SourceProduct,
    string SourceObjectRef,
    DateTimeOffset CapturedAt,
    string CapturedByPersonId,
    string? GeoCoordinates,
    string? DeviceSnapshot,
    string? Notes);

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
