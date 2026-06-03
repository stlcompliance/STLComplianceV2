using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class IncidentAttachment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid IncidentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";

    public long SizeBytes { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid UploadedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
