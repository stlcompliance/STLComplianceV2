namespace NexArr.Api.Contracts;

public sealed record ReferenceDatasetResponse(
    Guid Id,
    string Key,
    string Name,
    string Category,
    string OwnerService,
    string Status,
    string? CurrentPublishedVersion,
    int SourceCount,
    int EntityCount,
    int PendingReviewCount,
    int FailedImportCount,
    DateTimeOffset? LastPublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceSourceResponse(
    Guid Id,
    string Key,
    string Name,
    string SourceType,
    string ConnectorType,
    int AuthorityRank,
    string RefreshCadence,
    string? TermsNotes,
    bool Enabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceImportRecordInput(
    int? RowNumber,
    string RawPayloadJson,
    string? NormalizedPayloadJson,
    string ProposedEntityType,
    string? ProposedCanonicalKey,
    decimal Confidence);

public sealed record CreateReferenceImportRequest(
    Guid DatasetId,
    Guid SourceId,
    Guid? TenantId,
    Guid? RequestedByPersonId,
    string? RawObjectKey,
    string? FileName,
    IReadOnlyList<ReferenceImportRecordInput>? Records);

public sealed record ReferenceImportResponse(
    Guid Id,
    Guid DatasetId,
    string DatasetKey,
    string DatasetName,
    Guid SourceId,
    string SourceKey,
    string SourceName,
    Guid? TenantId,
    Guid? RequestedByPersonId,
    string Status,
    string? RawObjectKey,
    string? FileName,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorSummary,
    int StagingRecordCount,
    int PendingReviewCount,
    int ApprovedCount,
    int RejectedCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceStagingRecordResponse(
    Guid Id,
    Guid JobId,
    Guid DatasetId,
    string DatasetKey,
    Guid SourceId,
    string SourceKey,
    Guid? TargetDatasetId,
    string? TargetDatasetKey,
    string? TargetDatasetName,
    string? TargetOwnerService,
    int? RowNumber,
    string RawPayloadJson,
    string NormalizedPayloadJson,
    string ProposedEntityType,
    string? ProposedCanonicalKey,
    decimal Confidence,
    string Status,
    string? ReviewReason,
    Guid? ReviewerPersonId,
    DateTimeOffset? ReviewedAt,
    Guid? ReferenceEntityId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReviewDecisionRequest(
    string? Reason,
    string? DisplayName,
    string? CanonicalKey,
    string? NormalizedFieldsJson,
    string? SourceEvidenceJson,
    DateOnly? EffectiveDate,
    Guid? TargetDatasetId);

public sealed record CreateReferenceCrosswalkRequest(
    Guid ReferenceEntityId,
    string ExternalSystem,
    string ExternalKey,
    Guid? SourceId,
    decimal Confidence,
    string Status);

public sealed record ReferenceCrosswalkResponse(
    Guid Id,
    Guid ReferenceEntityId,
    string EntityType,
    string CanonicalKey,
    string DisplayName,
    string ExternalSystem,
    string ExternalKey,
    Guid? SourceId,
    string? SourceKey,
    decimal Confidence,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceTenantOverlayResponse(
    Guid Id,
    Guid TenantId,
    Guid ReferenceEntityId,
    string EntityType,
    string CanonicalKey,
    string? LocalName,
    string? LocalStatus,
    bool Hidden,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceProductMappingResponse(
    Guid Id,
    Guid TenantId,
    string ProductCode,
    Guid ReferenceEntityId,
    string EntityType,
    string CanonicalKey,
    string LocalEntityType,
    string LocalEntityId,
    string MappingStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceEntityVersionResponse(
    Guid Id,
    Guid ReferenceEntityId,
    int Version,
    string FieldsJson,
    string SourceEvidenceJson,
    DateOnly? EffectiveDate,
    DateTimeOffset? PublishedAt,
    Guid? SupersededByVersionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ReferenceEntityResponse(
    Guid Id,
    Guid DatasetId,
    string DatasetKey,
    string DatasetName,
    string EntityType,
    string CanonicalKey,
    string DisplayName,
    string Status,
    string NormalizedFieldsJson,
    Guid? FirstSeenSourceId,
    string? FirstSeenSourceKey,
    Guid? CurrentVersionId,
    int? CurrentVersion,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ReferenceEntityVersionResponse> Versions,
    IReadOnlyList<ReferenceCrosswalkResponse> Crosswalks,
    IReadOnlyList<ReferenceTenantOverlayResponse> TenantOverlays,
    IReadOnlyList<ReferenceProductMappingResponse> ProductMappings);

public sealed record ReferencePublishEventResponse(
    Guid Id,
    Guid DatasetId,
    string DatasetKey,
    string DatasetName,
    string PublishedVersion,
    Guid? PublishedByPersonId,
    string Summary,
    DateTimeOffset CreatedAt);

public sealed record ReferenceAuditEventResponse(
    Guid Id,
    Guid? ActorPersonId,
    string Action,
    string EntityType,
    Guid? EntityId,
    string? BeforeSnapshotJson,
    string? AfterSnapshotJson,
    DateTimeOffset CreatedAt);

public sealed record ReferenceLookupResponse(
    string Scope,
    string Query,
    IReadOnlyList<ReferenceEntityResponse> Matches,
    DateTimeOffset GeneratedAt);

public sealed record ReferenceCatalogSummaryResponse(
    Guid DatasetId,
    string DatasetKey,
    string DatasetName,
    string DatasetCategory,
    string Status,
    string? CurrentPublishedVersion,
    int PublishedEntityCount,
    int ActiveCrosswalkCount,
    int PendingReviewCount,
    DateTimeOffset GeneratedAt);

public sealed record ReferenceDataDashboardResponse(
    int DatasetCount,
    int SourceCount,
    int JobCount,
    int PendingReviewCount,
    int FailedImportCount,
    int PublishedEntityCount,
    int CrosswalkCount,
    int PublishEventCount,
    DateTimeOffset GeneratedAt);

public sealed record CreateReferenceDatasetRequest(
    string Key,
    string Name,
    string Category,
    string OwnerService,
    string Status);

public sealed record PublishReferenceDatasetsRequest(
    IReadOnlyList<Guid> DatasetIds,
    string? Summary);

public sealed record ReferencePublishBatchResponse(
    int RequestedCount,
    int PublishedCount,
    IReadOnlyList<ReferencePublishEventResponse> Items,
    DateTimeOffset ProcessedAt);

public sealed record CreateReferenceSourceRequest(
    string Key,
    string Name,
    string SourceType,
    string ConnectorType,
    int AuthorityRank,
    string RefreshCadence,
    string? TermsNotes,
    bool Enabled);

public sealed record CreateReferenceDatasetInputRequest(
    string? RawObjectKey,
    string? FileName,
    string? Value,
    string? ValuesText);

public sealed record UpdateReferenceEntityRequest(
    string? DisplayName,
    string? CanonicalKey,
    string? NormalizedFieldsJson,
    string? SourceEvidenceJson,
    DateOnly? EffectiveDate);

public sealed record CreateReferenceMasterCsvImportRequest(
    string CsvText,
    string? FileName,
    string? RawObjectKey);
