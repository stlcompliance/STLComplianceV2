using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class CertificationPublication : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public string QualificationKey { get; set; } = string.Empty;

    public string QualificationName { get; set; } = string.Empty;

    public string PublicationType { get; set; } = "training_blocker";

    public string BlockerType { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "published";

    public DateTimeOffset? ExpiresAt { get; set; }

    public DateTimeOffset PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
