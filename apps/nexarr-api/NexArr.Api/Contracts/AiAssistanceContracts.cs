using System.Text.Json;
using STLCompliance.Shared.SmartImport;

namespace NexArr.Api.Contracts;

public sealed record AiAssistantMessageRequest(
    Guid? SessionId,
    Guid? TenantId,
    string ProductKey,
    string Surface,
    string Route,
    string Category,
    string Message,
    JsonElement? PageContext,
    IReadOnlyList<string>? AllowedBehaviors);

public sealed record AiAssistantMessageResponse(
    Guid SessionId,
    Guid MessageId,
    string Outcome,
    string Answer,
    string? ErrorCode,
    string? SafeMessage,
    IReadOnlyList<string> RequiredReviewReasons);

public sealed record AiAdminDiagnosticResponse(
    string Status,
    bool OpenAiConfigured,
    string Model,
    string Message);

public sealed record AiActionPreviewRequest(
    Guid SessionId,
    string ProductKey,
    string ActionCategory,
    JsonElement Proposal);

public sealed record AiActionPreviewResponse(
    Guid ProposalId,
    string Status,
    IReadOnlyList<string> RequiredPermissions,
    IReadOnlyList<string> RequiredReviewReasons,
    JsonElement Proposal);

public sealed record RecordArrSmartImportRetainSourceRequest(
    Guid TenantId,
    Guid UploadedByPersonId,
    Guid ImportBatchId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string ContentBase64,
    string DestinationProductHint);

public sealed record RecordArrSmartImportRetainSourceResponse(
    string RecordId,
    string FileId,
    string StorageKey,
    string Status);

public sealed record SmartImportCreateCommitPlanRequest(IReadOnlyList<Guid>? ProposedRecordIds);

public sealed record SmartImportManualFieldMapping(
    string SourceField,
    string TargetField);

public sealed record SmartImportManualMappingOverrideRequest(
    IReadOnlyList<SmartImportManualFieldMapping> FieldMappings,
    string? Notes);

public sealed record SmartImportManualMappingOverrideResponse(
    Guid BatchId,
    int MappingCount,
    int UpdatedCount,
    int SkippedCount,
    int TotalProposedRecordCount);

public sealed record SmartImportBulkReviewDecisionRequest(
    IReadOnlyList<Guid>? ProposedRecordIds,
    string Decision,
    string? Notes);

public sealed record SmartImportBulkReviewDecisionResponse(
    Guid BatchId,
    string Decision,
    int RequestedCount,
    int UpdatedCount,
    int SkippedCount,
    int TotalProposedRecordCount);

public sealed record SmartImportCommitResult(
    Guid CommitPlanId,
    string Status,
    int CompletedStepCount,
    int FailedStepCount,
    IReadOnlyList<SmartImportCommitStepResult> Steps);

public sealed record SmartImportCommitStepResult(
    Guid CommitStepId,
    string DestinationProduct,
    string EntityType,
    string Operation,
    string Status,
    string? ResultEntityId,
    string? ErrorCode,
    string? ErrorMessage,
    bool Retryable);
