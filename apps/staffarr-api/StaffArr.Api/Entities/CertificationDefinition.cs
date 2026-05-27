using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class CertificationDefinition : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string CertificationKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Category { get; set; } = "readiness";

    public int? DefaultValidityDays { get; set; }

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
