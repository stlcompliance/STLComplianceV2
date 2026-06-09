namespace NexArr.Api.Entities;

public sealed class ReferenceDataset
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string OwnerService { get; set; } = string.Empty;

    public string Status { get; set; } = ReferenceDatasetStatuses.Draft;

    public string? CurrentPublishedVersion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ReferenceSource
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string ConnectorType { get; set; } = string.Empty;

    public int AuthorityRank { get; set; }

    public string RefreshCadence { get; set; } = string.Empty;

    public string? TermsNotes { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class IngestionJob
{
    public Guid Id { get; set; }

    public Guid DatasetId { get; set; }

    public Guid SourceId { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? RequestedByPersonId { get; set; }

    public string Status { get; set; } = ReferenceImportStatuses.Pending;

    public string? RawObjectKey { get; set; }

    public string? FileName { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? ErrorSummary { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ReferenceDataset Dataset { get; set; } = null!;

    public ReferenceSource Source { get; set; } = null!;
}

public sealed class StagingRecord
{
    public Guid Id { get; set; }

    public Guid JobId { get; set; }

    public Guid? TargetDatasetId { get; set; }

    public int? RowNumber { get; set; }

    public string RawPayloadJson { get; set; } = "{}";

    public string NormalizedPayloadJson { get; set; } = "{}";

    public string ProposedEntityType { get; set; } = string.Empty;

    public string? ProposedCanonicalKey { get; set; }

    public decimal Confidence { get; set; }

    public string Status { get; set; } = ReferenceStagingStatuses.NeedsReview;

    public string? ReviewReason { get; set; }

    public Guid? ReviewerPersonId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public Guid? ReferenceEntityId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public IngestionJob Job { get; set; } = null!;

    public ReferenceDataset? TargetDataset { get; set; }
}

public sealed class ReferenceEntity
{
    public Guid Id { get; set; }

    public Guid DatasetId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public string CanonicalKey { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Status { get; set; } = ReferenceEntityStatuses.Active;

    public string NormalizedFieldsJson { get; set; } = "{}";

    public Guid? FirstSeenSourceId { get; set; }

    public Guid? CurrentVersionId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ReferenceDataset Dataset { get; set; } = null!;
}

public sealed class ReferenceEntityVersion
{
    public Guid Id { get; set; }

    public Guid ReferenceEntityId { get; set; }

    public int Version { get; set; }

    public string FieldsJson { get; set; } = "{}";

    public string SourceEvidenceJson { get; set; } = "{}";

    public DateOnly? EffectiveDate { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public Guid? SupersededByVersionId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ReferenceCrosswalk
{
    public Guid Id { get; set; }

    public Guid ReferenceEntityId { get; set; }

    public string ExternalSystem { get; set; } = string.Empty;

    public string ExternalKey { get; set; } = string.Empty;

    public Guid? SourceId { get; set; }

    public decimal Confidence { get; set; }

    public string Status { get; set; } = ReferenceCrosswalkStatuses.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class TenantReferenceOverlay
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ReferenceEntityId { get; set; }

    public string? LocalName { get; set; }

    public string? LocalStatus { get; set; }

    public bool Hidden { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ProductMapping
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public Guid ReferenceEntityId { get; set; }

    public string LocalEntityType { get; set; } = string.Empty;

    public string LocalEntityId { get; set; } = string.Empty;

    public string MappingStatus { get; set; } = ReferenceMappingStatuses.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ReferencePublishEvent
{
    public Guid Id { get; set; }

    public Guid DatasetId { get; set; }

    public string PublishedVersion { get; set; } = string.Empty;

    public Guid? PublishedByPersonId { get; set; }

    public string Summary { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class ReferenceAuditEvent
{
    public Guid Id { get; set; }

    public Guid? ActorPersonId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string? BeforeSnapshotJson { get; set; }

    public string? AfterSnapshotJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public static class ReferenceDatasetStatuses
{
    public const string Draft = "draft";
    public const string Ready = "ready";
    public const string Published = "published";
    public const string Archived = "archived";
}

public static class ReferenceImportStatuses
{
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string ReviewRequired = "review_required";
    public const string Completed = "completed";
    public const string Failed = "failed";
}

public static class ReferenceStagingStatuses
{
    public const string NeedsReview = "needs_review";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Merged = "merged";
    public const string Escalated = "escalated";
}

public static class ReferenceEntityStatuses
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Superseded = "superseded";
}

public static class ReferenceCrosswalkStatuses
{
    public const string Active = "active";
    public const string Pending = "pending";
    public const string Retired = "retired";
}

public static class ReferenceMappingStatuses
{
    public const string Active = "active";
    public const string Pending = "pending";
    public const string Retired = "retired";
}
