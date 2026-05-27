using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class PersonRoleAssignment : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid PersonId { get; set; }

    public Guid RoleTemplateId { get; set; }

    public string ScopeType { get; set; } = "tenant";

    public string? ScopeValue { get; set; }

    public string Status { get; set; } = "active";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
