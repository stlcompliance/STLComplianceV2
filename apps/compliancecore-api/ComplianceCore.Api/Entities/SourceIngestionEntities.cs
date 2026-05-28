using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class SourceIngestionBatch : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string IngestionType { get; set; } = string.Empty;

    public string Phase { get; set; } = string.Empty;

    public bool DryRun { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TotalJobs { get; set; }

    public int SuccessCount { get; set; }

    public int ErrorCount { get; set; }

    public int SkippedCount { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public string? SourceProduct { get; set; }

    public Guid? PublicationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<SourceIngestionJob> Jobs { get; set; } = [];
}

public sealed class SourceIngestionJob
{
    public Guid Id { get; set; }

    public Guid BatchId { get; set; }

    public int RowIndex { get; set; }

    public string JobKey { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public string? ErrorCode { get; set; }

    public string? Message { get; set; }

    public SourceIngestionBatch? Batch { get; set; }
}

public static class SourceIngestionTypes
{
    public const string FactSources = "fact_sources";

    public const string ProductFacts = "product_facts";
}

public static class SourceIngestionPhases
{
    public const string Validate = "validate";

    public const string Commit = "commit";
}

public static class SourceIngestionBatchStatuses
{
    public const string Completed = "completed";

    public const string Partial = "partial";

    public const string Failed = "failed";
}

public static class SourceIngestionJobStatuses
{
    public const string Validated = "validated";

    public const string Created = "created";

    public const string Accepted = "accepted";

    public const string Skipped = "skipped";

    public const string Error = "error";
}
