namespace NexArr.Api.Entities;

public sealed class AiSession
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActorPersonId { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string Surface { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AiMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActorPersonId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UserInputRedacted { get; set; } = string.Empty;
    public string OutputRedacted { get; set; } = string.Empty;
    public string ContextJson { get; set; } = "{}";
    public string Outcome { get; set; } = string.Empty;
    public string? ProviderResponseId { get; set; }
    public string? ProviderRequestId { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public string? ErrorCode { get; set; }
    public string? SafeMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AiActionProposal
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActorPersonId { get; set; }
    public string ProductKey { get; set; } = string.Empty;
    public string ActionCategory { get; set; } = string.Empty;
    public string Status { get; set; } = "preview";
    public string ProposalJson { get; set; } = "{}";
    public string RequiredPermissionsJson { get; set; } = "[]";
    public string ReviewReasonsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}

public sealed class AiAuditEvent
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ActorPersonId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? ReasonCode { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public string? CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class ImportBatch
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ActorPersonId { get; set; }
    public string Status { get; set; } = "uploaded";
    public string DestinationProductHint { get; set; } = "unknown";
    public string SourceLabel { get; set; } = string.Empty;
    public string ReviewPolicyJson { get; set; } = "{}";
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ProcessingStartedAt { get; set; }
    public DateTimeOffset? ProcessingCompletedAt { get; set; }
}

public sealed class ImportFile
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid TenantId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string? RecordArrRecordId { get; set; }
    public string? RecordArrFileId { get; set; }
    public string? RecordArrStorageKey { get; set; }
    public int? PageCount { get; set; }
    public int? SheetCount { get; set; }
    public int? RowCount { get; set; }
    public string Status { get; set; } = "retained";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ImportClassification
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid ImportFileId { get; set; }
    public Guid TenantId { get; set; }
    public string DestinationProduct { get; set; } = "unknown";
    public string EntityType { get; set; } = "unknown";
    public decimal Confidence { get; set; }
    public bool RequiresReview { get; set; } = true;
    public string ReviewReasonsJson { get; set; } = "[]";
    public string? Notes { get; set; }
    public string ProviderOutcome { get; set; } = "not_called";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ImportExtractedField
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid ImportFileId { get; set; }
    public Guid TenantId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string? RawValue { get; set; }
    public string? NormalizedValue { get; set; }
    public decimal Confidence { get; set; }
    public bool RequiresReview { get; set; } = true;
    public string ReviewReasonsJson { get; set; } = "[]";
    public string SourceLocationJson { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ImportProposedRecord
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid TenantId { get; set; }
    public string DestinationProduct { get; set; } = "unknown";
    public string EntityType { get; set; } = "unknown";
    public string Operation { get; set; } = "create";
    public decimal Confidence { get; set; }
    public string ReviewStatus { get; set; } = "review_required";
    public bool RequiresReview { get; set; } = true;
    public string ReviewReasonsJson { get; set; } = "[]";
    public string ProposedPayloadJson { get; set; } = "{}";
    public string? DeterministicPayloadJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ImportMatchCandidate
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid ImportProposedRecordId { get; set; }
    public Guid TenantId { get; set; }
    public string SourceProduct { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string MatchReasonsJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ImportReviewDecision
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid ImportProposedRecordId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ReviewerPersonId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CorrectedPayloadJson { get; set; }
    public DateTimeOffset DecidedAt { get; set; }
}

public sealed class ImportCommitPlan
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CreatedByPersonId { get; set; }
    public Guid? ApprovedByPersonId { get; set; }
    public string Status { get; set; } = "draft";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CommittedAt { get; set; }
}

public sealed class ImportCommitStep
{
    public Guid Id { get; set; }
    public Guid ImportCommitPlanId { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid ImportProposedRecordId { get; set; }
    public Guid TenantId { get; set; }
    public int StepOrder { get; set; }
    public string DestinationProduct { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string? ResultEntityId { get; set; }
    public string? ResultDisplayName { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Retryable { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class ImportAuditEvent
{
    public Guid Id { get; set; }
    public Guid ImportBatchId { get; set; }
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ActorType { get; set; } = "human";
    public Guid? ActorPersonId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? ReasonCode { get; set; }
    public string MetadataJson { get; set; } = "{}";
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class ImportMappingTemplate
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string DestinationProduct { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string MappingJson { get; set; } = "{}";
    public bool Active { get; set; } = true;
    public Guid CreatedByPersonId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
