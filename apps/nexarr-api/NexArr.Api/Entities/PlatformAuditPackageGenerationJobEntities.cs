namespace NexArr.Api.Entities;

public sealed class PlatformAuditPackageGenerationJob
{
    public Guid Id { get; set; }

    public Guid? ScopeTenantId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public string Status { get; set; } = PlatformAuditPackageGenerationJobStatuses.Pending;

    public string Format { get; set; } = string.Empty;

    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }

    public string? FilterJson { get; set; }

    public Guid? PackageId { get; set; }

    public byte[]? ArtifactZip { get; set; }

    public string? ArtifactJson { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

public static class PlatformAuditPackageGenerationJobStatuses
{
    public const string Pending = "pending";

    public const string Processing = "processing";

    public const string Completed = "completed";

    public const string Failed = "failed";
}

public static class PlatformAuditPackageGenerationFormats
{
    public const string Zip = "zip";

    public const string Json = "json";
}
