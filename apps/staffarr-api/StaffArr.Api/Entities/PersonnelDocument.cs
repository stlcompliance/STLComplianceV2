using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonnelDocument : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public string DocumentTypeKey { get; set; } = "other";

    public string Title { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string Status { get; set; } = "active";

    public Guid UploadedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
