namespace NexArr.Api.Contracts;

public sealed record CreatePlatformAuditPackageGenerationJobRequest(
    string Format,
    DateTimeOffset? From,
    DateTimeOffset? To,
    Guid? TenantId);

public sealed record PlatformAuditPackageGenerationJobResponse(
    Guid JobId,
    Guid? ScopeTenantId,
    string Status,
    string Format,
    DateTimeOffset? From,
    DateTimeOffset? To,
    Guid? PackageId,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    bool DownloadReady);

public sealed record PendingPlatformAuditPackageGenerationJobItem(
    Guid JobId,
    Guid? ScopeTenantId,
    string Format,
    DateTimeOffset CreatedAt);

public sealed record PendingPlatformAuditPackageGenerationJobsResponse(
    DateTimeOffset AsOf,
    int BatchSize,
    IReadOnlyList<PendingPlatformAuditPackageGenerationJobItem> Items);

public sealed record ProcessPlatformAuditPackageGenerationJobsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record PlatformAuditPackageGenerationJobResult(
    Guid JobId,
    Guid? ScopeTenantId,
    string Status,
    Guid? PackageId);

public sealed record PlatformAuditPackageGenerationJobSkip(
    Guid JobId,
    string Reason);

public sealed record ProcessPlatformAuditPackageGenerationJobsResponse(
    DateTimeOffset AsOf,
    int BatchSize,
    int CandidatesFound,
    int CompletedCount,
    int FailedCount,
    int SkippedCount,
    IReadOnlyList<PlatformAuditPackageGenerationJobResult> Results,
    IReadOnlyList<PlatformAuditPackageGenerationJobSkip> Skipped);
