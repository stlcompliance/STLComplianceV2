using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffRoleScope : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RoleId { get; set; }

    public string ScopeType { get; set; } = "tenant";

    public string? ScopeRefId { get; set; }

    public string? ScopeRefSnapshot { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
