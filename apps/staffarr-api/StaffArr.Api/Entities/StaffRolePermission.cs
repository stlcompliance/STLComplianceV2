using STLCompliance.Shared.Data;

namespace StaffArr.Api.Entities;

public sealed class StaffRolePermission : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid RoleId { get; set; }

    public string ProductKey { get; set; } = string.Empty;

    public string PermissionKey { get; set; } = string.Empty;

    public string Effect { get; set; } = "allow";

    public DateTimeOffset CreatedAt { get; set; }
}
