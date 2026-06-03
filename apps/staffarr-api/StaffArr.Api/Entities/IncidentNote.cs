using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class IncidentNote : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid IncidentId { get; set; }

    public string NoteTypeKey { get; set; } = "note";

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Status { get; set; } = "open";

    public DateTimeOffset? DueAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
