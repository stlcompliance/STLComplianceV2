using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffRole : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string RoleType { get; set; } = "tenant_role";

    public bool IsSystem { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
