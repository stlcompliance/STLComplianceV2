namespace StaffArr.Api.Contracts;

public sealed record CreateAuditPackageGenerationJobRequest(
    string Format,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Action = null,
    string? Result = null,
    string? TargetType = null,
    Guid? ActorUserId = null);

public sealed record AuditPackageGenerationJobResponse(
    Guid JobId,
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

public sealed record PendingAuditPackageGenerationJobItem(
    Guid JobId,
    Guid TenantId,
    string Format,
    DateTimeOffset CreatedAt);

public sealed record PendingAuditPackageGenerationJobsResponse(
    DateTimeOffset AsOf,
    int BatchSize,
    IReadOnlyList<PendingAuditPackageGenerationJobItem> Items);

public sealed record ProcessAuditPackageGenerationJobsRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record AuditPackageGenerationJobResult(
    Guid JobId,
    Guid TenantId,
    string Status,
    Guid? PackageId);

public sealed record AuditPackageGenerationJobSkip(
    Guid JobId,
    string Reason);

public sealed record ProcessAuditPackageGenerationJobsResponse(
    DateTimeOffset AsOf,
    int BatchSize,
    int CandidatesFound,
    int CompletedCount,
    int FailedCount,
    int SkippedCount,
    IReadOnlyList<AuditPackageGenerationJobResult> Results,
    IReadOnlyList<AuditPackageGenerationJobSkip> Skipped);
