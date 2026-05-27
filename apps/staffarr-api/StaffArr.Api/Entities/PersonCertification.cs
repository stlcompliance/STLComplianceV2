using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonCertification : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid CertificationDefinitionId { get; set; }

    public string SourceType { get; set; } = "manual";

    public string Status { get; set; } = "active";

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Notes { get; set; }

    public Guid? GrantedByUserId { get; set; }

    public Guid? ExternalPublicationId { get; set; }

    public Guid? LastExternalLifecyclePublicationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
