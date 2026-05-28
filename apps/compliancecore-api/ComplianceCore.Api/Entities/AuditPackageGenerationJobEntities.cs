using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class AuditPackageGenerationJob : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public string Status { get; set; } = AuditPackageGenerationJobStatuses.Pending;

    public string Format { get; set; } = string.Empty;

    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }

    public Guid? PackageId { get; set; }

    public byte[]? ArtifactZip { get; set; }

    public string? ArtifactJson { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

public static class AuditPackageGenerationJobStatuses
{
    public const string Pending = "pending";

    public const string Processing = "processing";

    public const string Completed = "completed";

    public const string Failed = "failed";
}

public static class AuditPackageGenerationFormats
{
    public const string Zip = "zip";

    public const string Json = "json";
}
