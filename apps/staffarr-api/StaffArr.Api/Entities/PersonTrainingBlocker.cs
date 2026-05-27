using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonTrainingBlocker : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid TrainarrPublicationId { get; set; }

    public string QualificationKey { get; set; } = string.Empty;

    public string QualificationName { get; set; } = string.Empty;

    public string BlockerType { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "active";

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset? ClearedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
