namespace ComplianceCore.Api.Contracts;

public sealed record FactSourceIngestionRowRequest(
    Guid FactDefinitionId,
    string SourceKey,
    string SourceType,
    string Label,
    string Description,
    string? ProductKey,
    string? ProductReference,
    string ConfigJson,
    int Priority);

public sealed record FactSourceBulkIngestionRequest(
    IReadOnlyList<FactSourceIngestionRowRequest> Sources);

public sealed record ProductFactBulkIngestionRequest(
    Guid TenantId,
    Guid PublicationId,
    string SourceProduct,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ProductFactPublicationItemRequest> Facts);

public sealed record SourceIngestionJobResult(
    int RowIndex,
    string JobKey,
    string Status,
    string? EntityType,
    Guid? EntityId,
    string? ErrorCode,
    string? Message);

public sealed record SourceIngestionBatchResponse(
    Guid BatchId,
    string IngestionType,
    string Phase,
    bool DryRun,
    int TotalJobs,
    int SuccessCount,
    int ErrorCount,
    int SkippedCount,
    string Status,
    IReadOnlyList<SourceIngestionJobResult> Jobs);

public sealed record SourceIngestionBatchSummary(
    Guid BatchId,
    string IngestionType,
    string Phase,
    bool DryRun,
    string Status,
    int TotalJobs,
    int SuccessCount,
    int ErrorCount,
    int SkippedCount,
    string? SourceProduct,
    Guid? PublicationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record SourceIngestionBatchDetailResponse(
    Guid BatchId,
    string IngestionType,
    string Phase,
    bool DryRun,
    string Status,
    int TotalJobs,
    int SuccessCount,
    int ErrorCount,
    int SkippedCount,
    string? SourceProduct,
    Guid? PublicationId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<SourceIngestionJobResult> Jobs);
