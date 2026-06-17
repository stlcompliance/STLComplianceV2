namespace NexArr.Api.Entities;

public static class TenantIntegrationStatuses
{
    public const string NotConfigured = "not_configured";
    public const string Configured = "configured";
    public const string Connected = "connected";
    public const string Degraded = "degraded";
    public const string Disabled = "disabled";
    public const string NeedsReview = "needs_review";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NotConfigured,
        Configured,
        Connected,
        Degraded,
        Disabled,
        NeedsReview,
    };
}

public static class TenantIntegrationDirections
{
    public const string Inbound = "inbound";
    public const string Outbound = "outbound";
    public const string Bidirectional = "bidirectional";
    public const string ReadOnly = "read_only";
    public const string Writeback = "writeback";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Inbound,
        Outbound,
        Bidirectional,
        ReadOnly,
        Writeback,
    };
}

public static class TenantIntegrationSyncRunStatuses
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
    public const string SourceUnavailable = "source_unavailable";
    public const string NeedsReview = "needs_review";
    public const string DeadLetter = "dead_letter";
}

public static class TenantIntegrationTriggerKinds
{
    public const string Manual = "manual";
    public const string Worker = "worker";
    public const string Webhook = "webhook";
    public const string File = "file";
    public const string Callback = "callback";
}

public sealed class TenantIntegrationConnection
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string Status { get; set; } = TenantIntegrationStatuses.NotConfigured;

    public string SyncDirection { get; set; } = TenantIntegrationDirections.ReadOnly;

    public bool WritebacksEnabled { get; set; }

    public bool ManualMappingRequired { get; set; }

    public string ConfigurationJson { get; set; } = "{}";

    public DateTimeOffset? LastSuccessfulSyncAt { get; set; }

    public DateTimeOffset? LastFailedSyncAt { get; set; }

    public string? LastErrorCategory { get; set; }

    public string? LastErrorMessage { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid ModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ICollection<TenantIntegrationCredential> Credentials { get; set; } = [];
}

public sealed class TenantIntegrationCredential
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string CredentialKind { get; set; } = string.Empty;

    public string EncryptedPayload { get; set; } = string.Empty;

    public string EncryptionKeyId { get; set; } = string.Empty;

    public string RedactedLabel { get; set; } = string.Empty;

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? LastValidatedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public Guid ModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TenantIntegrationConnection Connection { get; set; } = null!;
}

public sealed class TenantIntegrationExternalMapping
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string OwningProductKey { get; set; } = string.Empty;

    public string StlEntityType { get; set; } = string.Empty;

    public string StlEntityId { get; set; } = string.Empty;

    public string ExternalEntityType { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string MappingStatus { get; set; } = "active";

    public string SyncDirection { get; set; } = TenantIntegrationDirections.ReadOnly;

    public DateTimeOffset? LastVerifiedAt { get; set; }

    public DateTimeOffset? LastSyncAt { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TenantIntegrationConnection Connection { get; set; } = null!;
}

public sealed class TenantIntegrationSyncRun
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string Status { get; set; } = TenantIntegrationSyncRunStatuses.Queued;

    public string Direction { get; set; } = TenantIntegrationDirections.ReadOnly;

    public string TriggeredBy { get; set; } = TenantIntegrationTriggerKinds.Manual;

    public Guid? TriggeredByUserId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public int AttemptCount { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public int SnapshotCount { get; set; }

    public int MappingCount { get; set; }

    public string? ErrorCategory { get; set; }

    public string? ErrorMessage { get; set; }

    public string DestinationProductsJson { get; set; } = "[]";

    public string ResultSummaryJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TenantIntegrationConnection Connection { get; set; } = null!;
}

public sealed class TenantIntegrationIntakeAttempt
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string IntakeKind { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string Status { get; set; } = "received";

    public string SourceRoute { get; set; } = string.Empty;

    public string PayloadHash { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? ErrorCategory { get; set; }

    public string? ErrorMessage { get; set; }
}

public sealed class TenantIntegrationProviderHealth
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string Status { get; set; } = "unknown";

    public DateTimeOffset CheckedAt { get; set; }

    public double? LatencyMs { get; set; }

    public string? ErrorCategory { get; set; }

    public string? ErrorMessage { get; set; }

    public string MetadataJson { get; set; } = "{}";

    public TenantIntegrationConnection Connection { get; set; } = null!;
}

public sealed class TenantIntegrationManualMappingTemplate
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ConnectionId { get; set; }

    public string ProviderKey { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string SourceEntityType { get; set; } = string.Empty;

    public string TargetProductKey { get; set; } = string.Empty;

    public string TargetEntityType { get; set; } = string.Empty;

    public string MappingJson { get; set; } = "{}";

    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }

    public Guid ModifiedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public TenantIntegrationConnection Connection { get; set; } = null!;
}
