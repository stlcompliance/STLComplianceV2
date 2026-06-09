using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetExternalIdentifier : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string SourceSystem { get; set; } = string.Empty;

    public string IdentifierType { get; set; } = string.Empty;

    public string IdentifierValue { get; set; } = string.Empty;

    public string NormalizedValue { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsVerified { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public DateTimeOffset ObservedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetEnrichmentSnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string SnapshotType { get; set; } = string.Empty;

    public string? SourceObjectRef { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = "{}";

    public DateTimeOffset CapturedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetEnrichmentSuggestion : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public Guid? SnapshotId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string FieldKey { get; set; } = string.Empty;

    public string FieldLabel { get; set; } = string.Empty;

    public string? CurrentValue { get; set; }

    public string? ProposedValue { get; set; }

    public string Reason { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public string Status { get; set; } = "pending";

    public string? ReviewedByPersonId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AssetRecallSnapshot : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string CampaignNumber { get; set; } = string.Empty;

    public string? ActionNumber { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public string Component { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Consequence { get; set; } = string.Empty;

    public string Remedy { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string? ModelYear { get; set; }

    public string? Make { get; set; }

    public string? Model { get; set; }

    public string? ReportReceivedDate { get; set; }

    public string Status { get; set; } = "active";

    public Guid? QualityHoldId { get; set; }

    public DateTimeOffset CapturedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ExternalProviderCacheEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string CacheKey { get; set; } = string.Empty;

    public string OperationKey { get; set; } = string.Empty;

    public string RequestJson { get; set; } = "{}";

    public string ResponseJson { get; set; } = "{}";

    public int? StatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset LastFetchedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class ExternalProviderAuditLogEntry : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid? AssetId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string OperationKey { get; set; } = string.Empty;

    public string CacheKey { get; set; } = string.Empty;

    public string ResultStatus { get; set; } = string.Empty;

    public int? DurationMs { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
